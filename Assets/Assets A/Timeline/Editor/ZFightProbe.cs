using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GslvMission.EditorTools
{
    /// <summary>
    /// Finds where the launch-pad scenery can z-fight on the headset. The editor draws with a reversed floating-point
    /// depth buffer, the headset (OpenGL ES) with a 24-bit one whose precision falls with distance squared, so surfaces
    /// lying a few centimetres apart look fine here and flicker there. Reports overlapping near-coplanar faces between
    /// meshes (and between a mesh's own parts), meshes resting on the terrain, and how far each lies from the viewer.
    /// Started by a trigger file; the report goes to %TEMP%\GSLV_ZFight_Probe.txt. Changes nothing.
    /// </summary>
    [InitializeOnLoad]
    static class ZFightProbe
    {
        const string Trigger = "Assets/Assets A/Timeline/Editor/PROBE_ZFIGHT_ONCE.txt";
        static readonly string ReportFile = Path.Combine(Path.GetTempPath(), "GSLV_ZFight_Probe.txt");
        const float Cell = 8f, MaxGap = 0.3f, MinDot = 0.995f, MinArea = 0.002f;

        static ZFightProbe()
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

        class Tri
        {
            public Vector3 a, b, c, n;
            public float d, area;
            public int owner;   // index into owners
        }

        class Owner
        {
            public string path, mat;
            public Bounds bounds;
        }

        class PairStat
        {
            public int count;
            public float area, minGap = float.MaxValue, maxGap, minDist = float.MaxValue, maxDist;
        }

        [MenuItem("GSLV/Probe Z-Fighting")]
        static void Run()
        {
            var sb = new StringBuilder();
            try { Probe(sb); }
            catch (Exception e) { sb.AppendLine("PROBE FAILED: " + e); }
            File.WriteAllText(ReportFile, sb.ToString());
            Debug.Log("[GSLV] Z-fight probe: " + ReportFile);
        }

        static string PathOf(Transform t)
        {
            var parts = new List<string>();
            for (; t != null; t = t.parent) parts.Insert(0, t.name);
            return string.Join("/", parts.ToArray());
        }

        static void Probe(StringBuilder sb)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            sb.AppendLine("Z-FIGHT PROBE " + DateTime.Now + "  scene " + scene.path);
            var roots = scene.GetRootGameObjects();
            var env = roots.FirstOrDefault(r => r.name == "ENVIRONMENT");
            if (env == null) throw new Exception("no ENVIRONMENT root");

            var eyeT = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "CenterEyeAnchor");
            Vector3 eye = eyeT != null ? eyeT.position : Vector3.zero;
            sb.AppendLine("eye (CenterEyeAnchor) " + eye.ToString("F2") + (eyeT == null ? "  (not found, using origin)" : ""));
            foreach (var cam in roots.SelectMany(r => r.GetComponentsInChildren<Camera>(true)))
                sb.AppendLine(string.Format("camera {0}: near {1} far {2}", PathOf(cam.transform), cam.nearClipPlane, cam.farClipPlane));

            // ---- triangles of the active scenery (not foliage: transparent leaves are another matter)
            var owners = new List<Owner>();
            var tris = new List<Tri>();
            int skipped = 0;
            foreach (var mr in env.GetComponentsInChildren<MeshRenderer>(false))
            {
                if (!mr.enabled) continue;
                var mf = mr.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                var mesh = mf.sharedMesh;
                string path = PathOf(mr.transform);
                var mats = mr.sharedMaterials;
                if (mats.Any(m => m != null && m.renderQueue >= 2450)) { skipped++; continue; }
                var m4 = mr.transform.localToWorldMatrix;
                var v = mesh.vertices.Select(p => m4.MultiplyPoint3x4(p)).ToArray();
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    var o = new Owner { path = path, mat = s < mats.Length && mats[s] != null ? mats[s].name : "(none)", bounds = mr.bounds };
                    owners.Add(o);
                    int oi = owners.Count - 1;
                    var idx = mesh.GetTriangles(s);
                    for (int i = 0; i + 2 < idx.Length; i += 3)
                    {
                        var t = new Tri { a = v[idx[i]], b = v[idx[i + 1]], c = v[idx[i + 2]], owner = oi };
                        var cr = Vector3.Cross(t.b - t.a, t.c - t.a);
                        t.area = cr.magnitude * 0.5f;
                        if (t.area < MinArea) continue;
                        t.n = cr / (t.area * 2f);
                        t.d = Vector3.Dot(t.n, t.a);
                        tris.Add(t);
                    }
                }
            }
            sb.AppendLine(string.Format("scenery: {0} renderer parts, {1} triangles (skipped {2} transparent/cutout renderers)", owners.Count, tris.Count, skipped));

            // ---- overlapping near-coplanar faces
            var grid = new Dictionary<long, List<int>>();
            for (int i = 0; i < tris.Count; i++)
            {
                var t = tris[i];
                var mn = Vector3.Min(t.a, Vector3.Min(t.b, t.c)) / Cell;
                var mx = Vector3.Max(t.a, Vector3.Max(t.b, t.c)) / Cell;
                int x0 = Mathf.FloorToInt(mn.x), y0 = Mathf.FloorToInt(mn.y), z0 = Mathf.FloorToInt(mn.z);
                int x1 = Mathf.FloorToInt(mx.x), y1 = Mathf.FloorToInt(mx.y), z1 = Mathf.FloorToInt(mx.z);
                long cells = (long)(x1 - x0 + 1) * (y1 - y0 + 1) * (z1 - z0 + 1);
                if (cells > 4096) { x1 = x0; y1 = y0; z1 = z0; }   // huge faces: one cell, partners still find them from theirs
                for (int x = x0; x <= x1; x++) for (int y = y0; y <= y1; y++) for (int z = z0; z <= z1; z++)
                {
                    long key = ((long)(x + 100000) * 200003L + (y + 100000)) * 200003L + (z + 100000);
                    List<int> list;
                    if (!grid.TryGetValue(key, out list)) grid[key] = list = new List<int>();
                    list.Add(i);
                }
            }
            var seen = new HashSet<long>();
            var pairs = new Dictionary<string, PairStat>();
            long tested = 0;
            foreach (var list in grid.Values)
            {
                if (list.Count < 2) continue;
                for (int p = 0; p < list.Count; p++)
                for (int q = p + 1; q < list.Count; q++)
                {
                    var A = tris[list[p]]; var B = tris[list[q]];
                    if (Vector3.Dot(A.n, B.n) < MinDot) continue;
                    float g = Mathf.Max(Mathf.Abs(Vector3.Dot(A.n, B.a) - A.d), Mathf.Max(Mathf.Abs(Vector3.Dot(A.n, B.b) - A.d), Mathf.Abs(Vector3.Dot(A.n, B.c) - A.d)));
                    if (g > MaxGap) continue;
                    if (g < 1e-4f && A.owner == B.owner) continue;   // the same surface (a quad's two halves)
                    int ia = Math.Min(list[p], list[q]), ib = Math.Max(list[p], list[q]);
                    if (!seen.Add((long)ia * 50000000L + ib)) continue;
                    tested++;
                    if (!Overlap2D(A, B)) continue;
                    var oa = owners[A.owner]; var ob = owners[B.owner];
                    string ka = oa.path + " [" + oa.mat + "]", kb = ob.path + " [" + ob.mat + "]";
                    string key = string.CompareOrdinal(ka, kb) <= 0 ? ka + "\n        vs " + kb : kb + "\n        vs " + ka;
                    PairStat st;
                    if (!pairs.TryGetValue(key, out st)) pairs[key] = st = new PairStat();
                    st.count++;
                    st.area += Mathf.Min(A.area, B.area);
                    st.minGap = Mathf.Min(st.minGap, g);
                    st.maxGap = Mathf.Max(st.maxGap, g);
                    float dist = Vector3.Distance(eye, (A.a + A.b + A.c) / 3f);
                    st.minDist = Mathf.Min(st.minDist, dist);
                    st.maxDist = Mathf.Max(st.maxDist, dist);
                }
            }
            sb.AppendLine(string.Format("coplanar candidates tested: {0}, overlapping groups: {1}", tested, pairs.Count));
            sb.AppendLine("  (z-fight from: the distance beyond which a 24-bit depth buffer cannot separate the gap, ~4 depth steps; near 0.1 / 0.3)");
            foreach (var kv in pairs.OrderByDescending(k => k.Value.area).Take(60))
            {
                var st = kv.Value;
                sb.AppendLine(string.Format("{0,8:0.0} m2 {1,6} faces  gap {2:0.000}-{3:0.000} m  dist {4:0}-{5:0} m  z-fight from {6:0}/{7:0} m\n        {8}",
                    st.area, st.count, st.minGap, st.maxGap, st.minDist, st.maxDist, FightFrom(st.maxGap, 0.1f), FightFrom(st.maxGap, 0.3f), kv.Key));
            }

            // ---- scenery lying on the terrain
            var terrains = UnityEngine.Object.FindObjectsOfType<Terrain>();
            foreach (var tr in terrains)
            {
                var td = tr.terrainData;
                sb.AppendLine(string.Format("terrain {0}: pos {1} size {2} heightmap {3} pixelError {4} basemap {5} trees {6} detail layers {7} layers {8} material {9}",
                    PathOf(tr.transform), tr.transform.position.ToString("F1"), td.size.ToString("F0"), td.heightmapResolution, tr.heightmapPixelError, tr.basemapDistance,
                    td.treeInstanceCount, td.detailPrototypes.Length, td.terrainLayers.Length, tr.materialTemplate != null ? tr.materialTemplate.name : tr.materialType.ToString()));
            }
            var onTerrain = new Dictionary<string, PairStat>();
            var below = new Dictionary<string, int>();
            foreach (var t in tris)
            {
                if (t.n.y < 0.8f) continue;
                var cen = (t.a + t.b + t.c) / 3f;
                foreach (var p in new[] { cen, t.a, t.b, t.c })
                {
                    var tr = terrains.FirstOrDefault(x => Contains(x, p));
                    if (tr == null) continue;
                    float h = tr.SampleHeight(p) + tr.transform.position.y;
                    float gap = p.y - h;
                    if (Mathf.Abs(gap) > 0.5f) continue;
                    var o = owners[t.owner];
                    string key = o.path + " [" + o.mat + "]";
                    PairStat st;
                    if (!onTerrain.TryGetValue(key, out st)) onTerrain[key] = st = new PairStat();
                    st.count++;
                    st.area += t.area / 4f;
                    st.minGap = Mathf.Min(st.minGap, gap);
                    st.maxGap = Mathf.Max(st.maxGap, gap);
                    float dist = Vector3.Distance(eye, p);
                    st.minDist = Mathf.Min(st.minDist, dist);
                    st.maxDist = Mathf.Max(st.maxDist, dist);
                    if (gap < 0f) below[key] = (below.ContainsKey(key) ? below[key] : 0) + 1;
                }
            }
            sb.AppendLine("scenery within 0.5 m of the terrain surface (up-facing faces; negative gap = terrain pokes through):");
            foreach (var kv in onTerrain.OrderByDescending(k => k.Value.area).Take(40))
            {
                var st = kv.Value;
                sb.AppendLine(string.Format("{0,8:0.0} m2 {1,6} samples ({2} below)  gap {3:0.000} .. {4:0.000} m  dist {5:0}-{6:0} m\n        {7}",
                    st.area, st.count, below.ContainsKey(kv.Key) ? below[kv.Key] : 0, st.minGap, st.maxGap, st.minDist, st.maxDist, kv.Key));
            }

            // ---- where the scenery sits from the viewer
            sb.AppendLine("scenery groups (closest point / centre distance from the eye):");
            foreach (Transform g in env.transform.Cast<Transform>().SelectMany(c => c.Cast<Transform>().Concat(new[] { c })))
            {
                var rs = g.GetComponentsInChildren<Renderer>(false);
                if (rs.Length == 0) continue;
                var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
                sb.AppendLine(string.Format("  {0,6:0} {1,6:0} m  {2,5} renderers  size {3}  {4}", Mathf.Sqrt(b.SqrDistance(eye)), Vector3.Distance(b.center, eye), rs.Length, b.size.ToString("F0"), PathOf(g)));
            }
        }

        static float FightFrom(float gap, float near)
        {
            // depth step at distance z for a 24-bit buffer: z^2 / (near * 2^24); fights when the gap is under ~4 steps
            return Mathf.Sqrt(Mathf.Max(gap, 0.0005f) * near * 16777216f / 4f);
        }

        static bool Contains(Terrain t, Vector3 p)
        {
            var o = t.transform.position; var s = t.terrainData.size;
            return p.x >= o.x && p.x <= o.x + s.x && p.z >= o.z && p.z <= o.z + s.z;
        }

        // the two faces overlap when projected onto A's plane (each shrunk a little so faces that only share an edge don't count)
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
    }
}
