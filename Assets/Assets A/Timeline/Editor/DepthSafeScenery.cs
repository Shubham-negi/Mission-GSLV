using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GslvMission.EditorTools
{
    /// <summary>
    /// Stops the launch-pad scenery flickering on the headset. The editor draws with a reversed floating-point depth buffer,
    /// the headset (OpenGL ES) with a 24-bit one whose precision falls with distance squared: faces modelled a few
    /// millimetres or centimetres in front of others (facade panels on walls, markings on the road, plates on the tower) look
    /// right here and swap places from frame to frame there. For every such pair the headset cannot separate at its distance
    /// from the viewer, the front face (the one the editor shows) is moved forward until it can, at most 0.3 m and well under
    /// a pixel at that distance. The changed meshes are copies in Art/Depth Safe Meshes, so the FBX models stay as they are
    /// and running this again starts from them. The road also gets a depth-biased material so the terrain under it stays under.
    /// Menu GSLV/Fix Scenery Flicker, or the trigger file; the report goes to %TEMP%\GSLV_Scenery_Fix.txt.
    /// </summary>
    [InitializeOnLoad]
    static class DepthSafeScenery
    {
        const string Trigger = "Assets/Assets A/Timeline/Editor/FIX_SCENERY_ONCE.txt";
        const string MeshFolder = "Assets/Assets A/Art/Depth Safe Meshes";
        const string BiasShader = "GSLV/Standard (Depth Bias)";
        static readonly string[] BiasedMaterials = { "Assets/Assets A/Art/FBX/Launch pad/rocket env/City/Materials/Roads.mat" };
        static readonly string ReportFile = Path.Combine(Path.GetTempPath(), "GSLV_Scenery_Fix.txt");

        const float Cell = 8f, MaxGap = 0.3f, MinDot = 0.995f, MinArea = 0.0005f;
        const float Steps = 8f;        // depth-buffer steps to keep between two faces (interpolation and float error eat a few)
        const float Margin = 30f;      // the viewer moves about the pad during the mission: plan for this much further away
        const float MaxPush = 0.3f, PushStep = 0.01f;

        static DepthSafeScenery()
        {
            if (File.Exists(Trigger)) EditorApplication.delayCall += RunFromTrigger;
        }

        static void RunFromTrigger()
        {
            if (!File.Exists(Trigger)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += RunFromTrigger;
                return;
            }
            AssetDatabase.DeleteAsset(Trigger);
            if (File.Exists(Trigger)) File.Delete(Trigger);
            Run();
        }

        class Part
        {
            public MeshRenderer renderer;
            public MeshFilter filter;
            public Mesh original;
            public string path;
            public int fixedFaces;
            public float maxPush, minDist = float.MaxValue, maxDist;
        }

        class Tri
        {
            public Vector3 a, b, c, n;
            public float d, area;
            public int part, sub, index;   // index: the triangle's number within its submesh
        }

        [MenuItem("GSLV/Fix Scenery Flicker")]
        static void Run()
        {
            var sb = new StringBuilder();
            try { Fix(sb); }
            catch (Exception e) { sb.AppendLine("FIX FAILED: " + e); }
            File.WriteAllText(ReportFile, sb.ToString());
            Debug.Log("[GSLV] Scenery flicker fix: " + ReportFile);
        }

        static void Fix(StringBuilder sb)
        {
            var scene = EditorSceneManager.GetActiveScene();
            sb.AppendLine("SCENERY FLICKER FIX " + DateTime.Now + "  scene " + scene.path);
            var roots = scene.GetRootGameObjects();
            var env = roots.FirstOrDefault(r => r.name == "ENVIRONMENT");
            if (env == null) throw new Exception("no ENVIRONMENT root in the open scene");
            var eyeT = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "CenterEyeAnchor");
            if (eyeT == null) throw new Exception("no CenterEyeAnchor (QWR rig) in the open scene");
            Vector3 eye = eyeT.position;
            var settings = Object.FindObjectOfType<HeadsetRenderSettings>();
            float near = settings != null ? settings.nearClip : 0.3f;
            sb.AppendLine(string.Format("viewer at {0}, headset near plane {1} m", eye.ToString("F1"), near));

            // the opaque scenery, each back on its FBX mesh
            var parts = new List<Part>();
            foreach (var mr in env.GetComponentsInChildren<MeshRenderer>(false))
            {
                var mf = mr.GetComponent<MeshFilter>();
                if (!mr.enabled || mf == null || mf.sharedMesh == null) continue;
                if (mr.sharedMaterials.Any(m => m != null && m.renderQueue >= 2450)) continue;   // cutout and transparent foliage
                var original = OriginalOf(mf.sharedMesh);
                if (original.blendShapeCount > 0 || original.HasVertexAttribute(VertexAttribute.BlendWeight)) continue;
                parts.Add(new Part { renderer = mr, filter = mf, original = original, path = PathOf(mr.transform) });
            }

            // Move front faces forward, then look again: a middle layer moved toward the one above it makes a new close pair,
            // so the layers above it move on the next round.
            var tris = Triangles(parts, p => p.original);
            var push = new Dictionary<int, float>();   // triangle -> how far it has moved forward
            var rounds = new List<int>();
            for (int round = 0; round < 8; round++)
            {
                var add = new Dictionary<int, float>();
                int fights = 0;
                foreach (var f in Fights(tris, eye, near))
                {
                    fights++;
                    float done;
                    push.TryGetValue(f.front, out done);
                    float p = Mathf.Min(MaxPush - done, Mathf.Ceil((f.need - f.gap) / PushStep) * PushStep);
                    float old;
                    if (p > 0f && (!add.TryGetValue(f.front, out old) || old < p)) add[f.front] = p;
                    var part = parts[tris[f.front].part];
                    part.minDist = Mathf.Min(part.minDist, f.dist);
                    part.maxDist = Mathf.Max(part.maxDist, f.dist);
                }
                rounds.Add(fights);
                if (add.Count == 0) break;
                foreach (var kv in add)
                {
                    var t = tris[kv.Key];
                    var dv = t.n * kv.Value;
                    t.a += dv; t.b += dv; t.c += dv; t.d += kv.Value;
                    float done;
                    push.TryGetValue(kv.Key, out done);
                    push[kv.Key] = done + kv.Value;
                }
            }
            sb.AppendLine(string.Format("{0} renderer parts, {1} triangles; overlapping face pairs too close for the headset, round by round: {2}",
                parts.Count, tris.Count, string.Join(" > ", rounds.Select(n => n.ToString()).ToArray())));

            if (!AssetDatabase.IsValidFolder(MeshFolder)) AssetDatabase.CreateFolder(Path.GetDirectoryName(MeshFolder).Replace('\\', '/'), Path.GetFileName(MeshFolder));
            var keep = new HashSet<string>();
            var byPart = push.GroupBy(kv => tris[kv.Key].part).ToDictionary(g => g.Key, g => g.ToList());
            for (int pi = 0; pi < parts.Count; pi++)
            {
                var part = parts[pi];
                Mesh target = part.original;
                List<KeyValuePair<int, float>> moves;
                if (byPart.TryGetValue(pi, out moves))
                {
                    target = Pushed(part, moves.Select(kv => new KeyValuePair<Tri, float>(tris[kv.Key], kv.Value)).ToList());
                    string assetPath = MeshFolder + "/" + Safe(part.path) + ".asset";
                    if (AssetDatabase.LoadAssetAtPath<Mesh>(assetPath) != null) AssetDatabase.DeleteAsset(assetPath);
                    AssetDatabase.CreateAsset(target, assetPath);
                    string guid; long id;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(part.original, out guid, out id);
                    var imp = AssetImporter.GetAtPath(assetPath);
                    imp.userData = guid + ":" + id;
                    imp.SaveAndReimport();
                    target = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                    keep.Add(assetPath);
                    part.fixedFaces = moves.Count;
                    part.maxPush = moves.Max(kv => kv.Value);
                }
                if (part.filter.sharedMesh != target)
                {
                    Undo.RecordObject(part.filter, "Fix Scenery Flicker");
                    part.filter.sharedMesh = target;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(part.filter);
                }
            }
            foreach (var stale in AssetDatabase.FindAssets("t:Mesh", new[] { MeshFolder }).Select(AssetDatabase.GUIDToAssetPath).Where(p => !keep.Contains(p)).ToList())
                AssetDatabase.DeleteAsset(stale);

            sb.AppendLine("moved faces (count, largest move, distance from the viewer):");
            foreach (var part in parts.Where(p => p.fixedFaces > 0).OrderByDescending(p => p.fixedFaces))
                sb.AppendLine(string.Format("  {0,6} faces  {1:0.00} m  {2:0}-{3:0} m  {4}", part.fixedFaces, part.maxPush, part.minDist, part.maxDist, part.path));

            // the road lies on the terrain (in places a few cm under it): a depth bias keeps it on top
            var shader = Shader.Find(BiasShader);
            if (shader == null) sb.AppendLine("WARNING: shader '" + BiasShader + "' not found; the road keeps its material");
            else
                foreach (var mp in BiasedMaterials)
                {
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(mp);
                    if (mat == null) { sb.AppendLine("WARNING: no material at " + mp); continue; }
                    if (mat.shader != shader)
                    {
                        Undo.RecordObject(mat, "Fix Scenery Flicker");
                        mat.shader = shader;
                        EditorUtility.SetDirty(mat);
                    }
                    sb.AppendLine("depth-biased material: " + mp);
                }

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            sb.AppendLine(EditorSceneManager.SaveScene(scene) ? "Scene saved." : "SCENE NOT SAVED");

            // check: the same search on the meshes now in the scene
            var check = Triangles(parts, p => p.filter.sharedMesh);
            var left = Fights(check, eye, near).ToList();
            sb.AppendLine(string.Format("check on the saved meshes: {0} face pairs under {1} depth steps apart ({2} before); under 4 steps: {3}, under 2: {4}",
                left.Count, Steps, rounds[0], left.Count(f => f.gap < f.need / 2f), left.Count(f => f.gap < f.need / 4f)));
            foreach (var g in left.GroupBy(f => parts[check[f.front].part].path).OrderByDescending(g => g.Count()).Take(12))
                sb.AppendLine(string.Format("  {0,5} pairs  {1:0}-{2:0} m  gap/need {3:0.00}-{4:0.00}  {5}", g.Count(), g.Min(f => f.dist), g.Max(f => f.dist),
                    g.Min(f => f.gap / f.need), g.Max(f => f.gap / f.need), g.Key));
        }

        static Mesh Pushed(Part part, List<KeyValuePair<Tri, float>> moves)
        {
            var src = part.original;
            var m = Object.Instantiate(src);
            m.name = src.name + " (depth safe)";
            var verts = new List<Vector3>(); m.GetVertices(verts);
            var normals = new List<Vector3>(); bool hasN = src.HasVertexAttribute(VertexAttribute.Normal); if (hasN) m.GetNormals(normals);
            var tangents = new List<Vector4>(); bool hasT = src.HasVertexAttribute(VertexAttribute.Tangent); if (hasT) m.GetTangents(tangents);
            var colors = new List<Color>(); bool hasC = src.HasVertexAttribute(VertexAttribute.Color); if (hasC) m.GetColors(colors);
            var uvs = new List<Vector4>[8];
            for (int ch = 0; ch < 8; ch++)
                if (src.HasVertexAttribute(VertexAttribute.TexCoord0 + ch)) { uvs[ch] = new List<Vector4>(); m.GetUVs(ch, uvs[ch]); }

            var w2l = part.renderer.transform.worldToLocalMatrix;
            var subs = new int[m.subMeshCount][];
            for (int s = 0; s < subs.Length; s++) subs[s] = m.GetTriangles(s);
            var copies = new Dictionary<string, int>();   // a corner moved by the same amount is shared, so moved neighbours stay joined
            foreach (var mv in moves)
            {
                var t = mv.Key;
                Vector3 local = w2l.MultiplyVector(t.n * mv.Value);
                var idx = subs[t.sub];
                for (int k = 0; k < 3; k++)
                {
                    int vi = idx[t.index * 3 + k];
                    string key = vi + "|" + Mathf.RoundToInt(local.x * 1000f) + "|" + Mathf.RoundToInt(local.y * 1000f) + "|" + Mathf.RoundToInt(local.z * 1000f);
                    int ni;
                    if (!copies.TryGetValue(key, out ni))
                    {
                        ni = verts.Count;
                        verts.Add(verts[vi] + local);
                        if (hasN) normals.Add(normals[vi]);
                        if (hasT) tangents.Add(tangents[vi]);
                        if (hasC) colors.Add(colors[vi]);
                        for (int ch = 0; ch < 8; ch++) if (uvs[ch] != null) uvs[ch].Add(uvs[ch][vi]);
                        copies[key] = ni;
                    }
                    idx[t.index * 3 + k] = ni;
                }
            }
            if (verts.Count > 65535) m.indexFormat = IndexFormat.UInt32;
            m.SetVertices(verts);
            if (hasN) m.SetNormals(normals);
            if (hasT) m.SetTangents(tangents);
            if (hasC) m.SetColors(colors);
            for (int ch = 0; ch < 8; ch++)
            {
                if (uvs[ch] == null) continue;
                int dim = src.GetVertexAttributeDimension(VertexAttribute.TexCoord0 + ch);
                if (dim == 2) m.SetUVs(ch, uvs[ch].Select(v => (Vector2)v).ToList());
                else if (dim == 3) m.SetUVs(ch, uvs[ch].Select(v => (Vector3)v).ToList());
                else m.SetUVs(ch, uvs[ch]);
            }
            for (int s = 0; s < subs.Length; s++) m.SetTriangles(subs[s], s, false);
            m.RecalculateBounds();
            return m;
        }

        // ---------------------------------------------------------------- finding the faces that fight

        struct Fight
        {
            public int front;            // the triangle to move forward
            public float gap, need, dist;
        }

        static List<Tri> Triangles(List<Part> parts, Func<Part, Mesh> meshOf)
        {
            var tris = new List<Tri>();
            for (int pi = 0; pi < parts.Count; pi++)
            {
                var mesh = meshOf(parts[pi]);
                var m4 = parts[pi].renderer.transform.localToWorldMatrix;
                var v = mesh.vertices.Select(p => m4.MultiplyPoint3x4(p)).ToArray();
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    var idx = mesh.GetTriangles(s);
                    for (int i = 0; i + 2 < idx.Length; i += 3)
                    {
                        var t = new Tri { a = v[idx[i]], b = v[idx[i + 1]], c = v[idx[i + 2]], part = pi, sub = s, index = i / 3 };
                        var cr = Vector3.Cross(t.b - t.a, t.c - t.a);
                        t.area = cr.magnitude * 0.5f;
                        if (t.area < MinArea) continue;
                        t.n = cr / (t.area * 2f);
                        t.d = Vector3.Dot(t.n, t.a);
                        tris.Add(t);
                    }
                }
            }
            return tris;
        }

        static IEnumerable<Fight> Fights(List<Tri> tris, Vector3 eye, float near)
        {
            var grid = new Dictionary<long, List<int>>();
            for (int i = 0; i < tris.Count; i++)
            {
                var t = tris[i];
                var mn = Vector3.Min(t.a, Vector3.Min(t.b, t.c)) / Cell;
                var mx = Vector3.Max(t.a, Vector3.Max(t.b, t.c)) / Cell;
                int x0 = Mathf.FloorToInt(mn.x), y0 = Mathf.FloorToInt(mn.y), z0 = Mathf.FloorToInt(mn.z);
                int x1 = Mathf.FloorToInt(mx.x), y1 = Mathf.FloorToInt(mx.y), z1 = Mathf.FloorToInt(mx.z);
                if ((long)(x1 - x0 + 1) * (y1 - y0 + 1) * (z1 - z0 + 1) > 4096) { x1 = x0; y1 = y0; z1 = z0; }
                for (int x = x0; x <= x1; x++) for (int y = y0; y <= y1; y++) for (int z = z0; z <= z1; z++)
                {
                    long key = ((long)(x + 100000) * 200003L + (y + 100000)) * 200003L + (z + 100000);
                    List<int> list;
                    if (!grid.TryGetValue(key, out list)) grid[key] = list = new List<int>();
                    list.Add(i);
                }
            }
            var seen = new HashSet<long>();
            foreach (var list in grid.Values)
            {
                for (int p = 0; p < list.Count; p++)
                for (int q = p + 1; q < list.Count; q++)
                {
                    var A = tris[list[p]]; var B = tris[list[q]];
                    if (Vector3.Dot(A.n, B.n) < MinDot) continue;
                    float ga = Vector3.Dot(A.n, B.a) - A.d, gb = Vector3.Dot(A.n, B.b) - A.d, gc = Vector3.Dot(A.n, B.c) - A.d;
                    float gmax = Mathf.Max(Mathf.Abs(ga), Mathf.Max(Mathf.Abs(gb), Mathf.Abs(gc)));
                    if (gmax > MaxGap) continue;
                    int ia = Math.Min(list[p], list[q]), ib = Math.Max(list[p], list[q]);
                    if (!seen.Add((long)ia * 50000000L + ib)) continue;
                    if (Same(A, B) || !Overlap2D(A, B)) continue;
                    const float flat = 0.0005f;
                    bool before = ga > flat || gb > flat || gc > flat, behind = ga < -flat || gb < -flat || gc < -flat;
                    if (before && behind) continue;   // the faces cross: only a sliver along the crossing fights, and moving can't part them
                    float gap = Mathf.Min(Mathf.Abs(ga), Mathf.Min(Mathf.Abs(gb), Mathf.Abs(gc)));   // at the closest corner
                    float dist = Vector3.Distance(eye, (A.a + A.b + A.c) / 3f);
                    float z = dist + Margin;
                    float need = Steps * z * z / (near * 16777216f);
                    if (gap >= need) continue;
                    int front;
                    if (before) front = list[q];          // B lies in front of A
                    else if (behind) front = list[p];
                    else if (Mathf.Abs(A.area - B.area) > 0.01f * Mathf.Max(A.area, B.area))
                        front = A.area < B.area ? list[p] : list[q];   // coplanar: the smaller piece is the one laid on the other
                    else front = ia;                      // the same face cut two ways: always the same one of the two goes forward
                    yield return new Fight { front = front, gap = gap, need = need, dist = dist };
                }
            }
        }

        static bool Same(Tri A, Tri B)
        {
            const float e = 1e-4f;
            Func<Vector3, bool> has = p => (p - A.a).sqrMagnitude < e * e || (p - A.b).sqrMagnitude < e * e || (p - A.c).sqrMagnitude < e * e;
            return has(B.a) && has(B.b) && has(B.c);
        }

        // the two faces overlap when projected onto A's plane (each shrunk a little, so faces that only share an edge don't count)
        static bool Overlap2D(Tri A, Tri B)
        {
            var u = (A.b - A.a).normalized;
            var w = Vector3.Cross(A.n, u);
            Func<Vector3, Vector2> P = p => new Vector2(Vector3.Dot(p - A.a, u), Vector3.Dot(p - A.a, w));
            var a = Shrink(new[] { P(A.a), P(A.b), P(A.c) });
            var b = Shrink(new[] { P(B.a), P(B.b), P(B.c) });
            return !Separated(a, b) && !Separated(b, a);
        }

        static Vector2[] Shrink(Vector2[] t)
        {
            var c = (t[0] + t[1] + t[2]) / 3f;
            for (int i = 0; i < 3; i++) t[i] = c + (t[i] - c) * 0.95f;
            return t;
        }

        static bool Separated(Vector2[] a, Vector2[] b)
        {
            for (int i = 0; i < 3; i++)
            {
                var e = a[(i + 1) % 3] - a[i];
                var axis = new Vector2(-e.y, e.x);
                float amin = float.MaxValue, amax = float.MinValue, bmin = float.MaxValue, bmax = float.MinValue;
                foreach (var p in a) { float d = Vector2.Dot(p, axis); amin = Mathf.Min(amin, d); amax = Mathf.Max(amax, d); }
                foreach (var p in b) { float d = Vector2.Dot(p, axis); bmin = Mathf.Min(bmin, d); bmax = Mathf.Max(bmax, d); }
                if (amax <= bmin || bmax <= amin) return true;
            }
            return false;
        }

        // ---------------------------------------------------------------- helpers

        // the FBX mesh behind a depth-safe copy (the copy's importer keeps its guid and file id)
        static Mesh OriginalOf(Mesh mesh)
        {
            string path = AssetDatabase.GetAssetPath(mesh);
            if (!path.StartsWith(MeshFolder + "/")) return mesh;
            var data = (AssetImporter.GetAtPath(path).userData ?? "").Split(':');
            long id;
            if (data.Length == 2 && long.TryParse(data[1], out id))
                foreach (var o in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(data[0])))
                {
                    string g; long l;
                    if (o is Mesh && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out g, out l) && l == id) return (Mesh)o;
                }
            throw new Exception("the original mesh of " + path + " is gone");
        }

        static string PathOf(Transform t)
        {
            var parts = new List<string>();
            for (; t != null; t = t.parent) parts.Insert(0, t.name);
            return string.Join("/", parts.ToArray());
        }

        static string Safe(string path)
        {
            var s = path.Replace("ENVIRONMENT/", "").Replace('/', '-');
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s.Trim().Replace("  ", " ");
        }
    }
}
