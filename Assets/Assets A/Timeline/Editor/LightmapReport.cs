using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GslvMission.EditorTools
{
    /// <summary>
    /// Says how the open scene's baked lighting landed: every lightmapped renderer's triangles are looked up in its lightmap
    /// and the dark area counted by material and by renderer, the road in full detail, with the lights, the probes and
    /// pictures from the viewer's place. Changes nothing in the scene. Menu GSLV/Checks/Lightmap Report, or the trigger file;
    /// the report goes to %TEMP%\GSLV_Lightmap_Report.txt and the pictures to %TEMP%\gslv_shots.
    /// </summary>
    [InitializeOnLoad]
    static class LightmapReport
    {
        const string Trigger = "Assets/Assets A/Timeline/Editor/LIGHTMAP_REPORT_ONCE.txt";
        const string RoadMaterial = "c9e3db2d289d9a54d8b4cb88a10cd9e9";   // Roads.mat
        static readonly string ReportFile = Path.Combine(Path.GetTempPath(), "GSLV_Lightmap_Report.txt");
        static readonly string ShotFolder = Path.Combine(Path.GetTempPath(), "gslv_shots");
        const float Dark = 0.04f;   // of the lightmap's stored value: under this a lit albedo renders close to black

        static LightmapReport()
        {
            if (File.Exists(Trigger)) EditorApplication.update += RunFromTrigger;   // update, not delayCall: only update ticks with the editor in the background
        }

        static void RunFromTrigger()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;   // next tick
            EditorApplication.update -= RunFromTrigger;
            if (!File.Exists(Trigger)) return;
            AssetDatabase.DeleteAsset(Trigger);
            if (File.Exists(Trigger)) File.Delete(Trigger);
            Run();
        }

        class Stat
        {
            public string name;
            public int renderers;
            public float area, dark, sum;
        }

        class Map
        {
            public int w, h;
            public Color[] px;
        }

        [MenuItem("GSLV/Checks/Lightmap Report")]
        static void Run()
        {
            var sb = new StringBuilder();
            try { Report(sb); }
            catch (Exception e) { sb.AppendLine("REPORT FAILED: " + e); }
            File.WriteAllText(ReportFile, sb.ToString());
            Debug.Log("[GSLV] Lightmap report: " + ReportFile);
        }

        static void Report(StringBuilder sb)
        {
            var scene = SceneManager.GetActiveScene();
            sb.AppendLine("LIGHTMAP REPORT " + DateTime.Now + "  scene " + scene.path);
            var roots = scene.GetRootGameObjects();
            var maps = ReadLightmaps(sb);

            sb.AppendLine("\nLIGHTS:");
            foreach (var l in roots.SelectMany(r => r.GetComponentsInChildren<Light>(true)))
                sb.AppendLine(string.Format("  {0}  active {1}  {2}  {3}  intensity {4:0.00}  colour {5}  shadows {6}  culling {7}  dir {8}", PathOf(l.transform), l.isActiveAndEnabled, l.type, l.lightmapBakeType,
                    l.intensity, ColorUtility.ToHtmlStringRGB(l.color), l.shadows, l.cullingMask, l.transform.forward.ToString("F2")));
            sb.AppendLine(string.Format("ambient: {0}, intensity {1:0.00}, flat {2}, sky {3}; reflections {4:0.00}; fog {5}; sun {6}", RenderSettings.ambientMode, RenderSettings.ambientIntensity, RenderSettings.ambientLight,
                RenderSettings.skybox != null ? RenderSettings.skybox.name : "none", RenderSettings.reflectionIntensity, RenderSettings.fog, RenderSettings.sun != null ? RenderSettings.sun.name : "none"));
            var probes = LightmapSettings.lightProbes;
            sb.AppendLine("light probes baked: " + (probes != null ? probes.count : 0) + "; probe groups: " + roots.SelectMany(r => r.GetComponentsInChildren<LightProbeGroup>(true)).Count());

            // every lightmapped renderer's triangles, looked up in its lightmap
            var byMaterial = new Dictionary<string, Stat>();
            var byRenderer = new List<Stat>();
            var notMapped = new Dictionary<string, Stat>();
            var roads = new List<MeshRenderer>();
            foreach (var mr in roots.SelectMany(r => r.GetComponentsInChildren<MeshRenderer>(false)))
            {
                var mf = mr.GetComponent<MeshFilter>();
                if (!mr.enabled || mf == null || mf.sharedMesh == null) continue;
                if (mr.sharedMaterials.Any(m => m != null && AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m)) == RoadMaterial)) roads.Add(mr);
                if (mr.lightmapIndex < 0 || mr.lightmapIndex >= maps.Count)
                {
                    bool gi = GameObjectUtility.AreStaticEditorFlagsSet(mr.gameObject, StaticEditorFlags.ContributeGI);
                    string key = string.Format("GI static {0}, receive {1}, probes {2}, lightmap index {3}", gi, mr.receiveGI, mr.lightProbeUsage, mr.lightmapIndex);
                    Stat n;
                    if (!notMapped.TryGetValue(key, out n)) notMapped[key] = n = new Stat { name = key };
                    n.renderers++;
                    continue;
                }
                var stat = new Stat { name = PathOf(mr.transform), renderers = 1 };
                Measure(mr, mf.sharedMesh, maps[mr.lightmapIndex], (sub, area, value) =>
                {
                    stat.area += area; stat.sum += value * area;
                    if (value < Dark) stat.dark += area;
                    var mat = sub < mr.sharedMaterials.Length && mr.sharedMaterials[sub] != null ? mr.sharedMaterials[sub] : null;
                    string key = mat != null ? mat.name + " [" + mat.shader.name + "]" : "(no material)";
                    Stat s;
                    if (!byMaterial.TryGetValue(key, out s)) byMaterial[key] = s = new Stat { name = key };
                    s.area += area; s.sum += value * area;
                    if (value < Dark) s.dark += area;
                });
                byRenderer.Add(stat);
            }

            sb.AppendLine("\nTHE ROAD:");
            foreach (var mr in roads) sb.Append(Detail(mr, maps));
            if (roads.Count == 0) sb.AppendLine("  no renderer uses Roads.mat");

            sb.AppendLine("\nTERRAINS:");
            foreach (var t in roots.SelectMany(r => r.GetComponentsInChildren<Terrain>(false)))
            {
                int i = t.lightmapIndex;
                sb.AppendLine(string.Format("  {0}  lightmap {1}  scale/offset {2}  at {3}  size {4}  data {5}", PathOf(t.transform), i, t.lightmapScaleOffset.ToString("F4"), t.transform.position.ToString("F2"), t.terrainData.size, AssetDatabase.GetAssetPath(t.terrainData)));
                if (i < 0 || i >= maps.Count) continue;
                var so = t.lightmapScaleOffset;
                float sum = 0f; int dark = 0, n = 0;
                for (int y = 0; y < 100; y++)
                    for (int x = 0; x < 100; x++)
                    {
                        float v = Sample(maps[i], (x + 0.5f) / 100f * so.x + so.z, (y + 0.5f) / 100f * so.y + so.w);
                        sum += v; n++;
                        if (v < Dark) dark++;
                    }
                sb.AppendLine(string.Format("     mean {0:0.000}, dark {1:0.0}%", sum / n, 100f * dark / n));
            }

            sb.AppendLine("\nNOT LIGHTMAPPED (renderers):");
            foreach (var n in notMapped.Values.OrderByDescending(n => n.renderers)) sb.AppendLine(string.Format("  {0,5}  {1}", n.renderers, n.name));

            sb.AppendLine(string.Format("\nBY MATERIAL (dark = stored value under {0}), most dark area first:", Dark));
            foreach (var s in byMaterial.Values.OrderByDescending(s => s.dark).Take(40))
                sb.AppendLine(string.Format("  dark {0,5:0.0}% of {1,10:0} m2  mean {2:0.000}  {3}", 100f * s.dark / Mathf.Max(s.area, 1e-3f), s.area, s.sum / Mathf.Max(s.area, 1e-3f), s.name));
            sb.AppendLine("\nBY RENDERER, most dark area first:");
            foreach (var s in byRenderer.OrderByDescending(s => s.dark).Take(40))
                sb.AppendLine(string.Format("  dark {0,5:0.0}% of {1,10:0} m2  mean {2:0.000}  {3}", 100f * s.dark / Mathf.Max(s.area, 1e-3f), s.area, s.sum / Mathf.Max(s.area, 1e-3f), s.name));
            float all = byRenderer.Sum(s => s.area), allDark = byRenderer.Sum(s => s.dark);
            sb.AppendLine(string.Format("all lightmapped meshes: {0} renderers, {1:0} m2, dark {2:0.0}%", byRenderer.Count, all, 100f * allDark / Mathf.Max(all, 1e-3f)));

            Shots(scene, roads, sb);
        }

        static List<Map> ReadLightmaps(StringBuilder sb)
        {
            var maps = new List<Map>();
            var data = LightmapSettings.lightmaps;
            sb.AppendLine(string.Format("LIGHTMAPS: {0}, mode {1}, atlas {2}, {3} texels/m, padding {4}, compressed {5}, direct/indirect samples {6}/{7}, bounces {8}, backend {9}",
                data.Length, LightmapSettings.lightmapsMode, LightmapEditorSettings.maxAtlasSize, LightmapEditorSettings.bakeResolution, LightmapEditorSettings.padding, LightmapEditorSettings.textureCompression,
                LightmapEditorSettings.directSampleCount, LightmapEditorSettings.indirectSampleCount, LightmapEditorSettings.bounces, LightmapEditorSettings.lightmapper));
            Directory.CreateDirectory(ShotFolder);
            for (int i = 0; i < data.Length; i++)
            {
                var tex = data[i].lightmapColor;
                if (tex == null) { maps.Add(null); sb.AppendLine("  " + i + ": missing"); continue; }
                var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                Graphics.Blit(tex, rt);
                var read = new Texture2D(tex.width, tex.height, TextureFormat.RGBAFloat, false, true);
                RenderTexture.active = rt;
                read.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);
                var map = new Map { w = tex.width, h = tex.height, px = read.GetPixels() };
                maps.Add(map);
                sb.AppendLine(string.Format("  {0}: {1}x{2} {3}  mean {4:0.000}  zero texels {5:0.0}%  {6}", i, tex.width, tex.height, tex.format, map.px.Average(c => Value(c)), 100f * map.px.Count(c => Value(c) < 0.004f) / map.px.Length, AssetDatabase.GetAssetPath(tex)));
                File.WriteAllBytes(Path.Combine(ShotFolder, "lightmap_" + i + ".png"), read.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(read);
            }
            return maps;
        }

        static float Value(Color c)
        {
            return Mathf.Max(c.r, Mathf.Max(c.g, c.b));
        }

        static float Sample(Map map, float u, float v)
        {
            int x = Mathf.Clamp(Mathf.FloorToInt(u * map.w), 0, map.w - 1), y = Mathf.Clamp(Mathf.FloorToInt(v * map.h), 0, map.h - 1);
            return Value(map.px[y * map.w + x]);
        }

        // each triangle at its middle and halfway to each corner: (submesh, world area of that piece, the lightmap's value there)
        static void Measure(MeshRenderer mr, Mesh mesh, Map map, Action<int, float, float> each)
        {
            if (map == null) return;
            var uv = mesh.uv2.Length > 0 ? mesh.uv2 : mesh.uv;
            if (uv.Length == 0) return;
            var m4 = mr.transform.localToWorldMatrix;
            var v = mesh.vertices;
            var so = mr.lightmapScaleOffset;
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                var idx = mesh.GetTriangles(s);
                for (int i = 0; i + 2 < idx.Length; i += 3)
                {
                    float area = Vector3.Cross(m4.MultiplyVector(v[idx[i + 1]] - v[idx[i]]), m4.MultiplyVector(v[idx[i + 2]] - v[idx[i]])).magnitude * 0.5f;
                    if (area <= 0f) continue;
                    Vector2 a = uv[idx[i]], b = uv[idx[i + 1]], c = uv[idx[i + 2]], mid = (a + b + c) / 3f;
                    foreach (var p in new[] { mid, (mid + a) * 0.5f, (mid + b) * 0.5f, (mid + c) * 0.5f })
                        each(s, area * 0.25f, Sample(map, p.x * so.x + so.z, p.y * so.y + so.w));
                }
            }
        }

        static string Detail(MeshRenderer mr, List<Map> maps)
        {
            var sb = new StringBuilder();
            var mf = mr.GetComponent<MeshFilter>();
            var mesh = mf.sharedMesh;
            var uv1 = mesh.uv2;
            var uv = uv1.Length > 0 ? uv1 : mesh.uv;
            sb.AppendLine("  " + PathOf(mr.transform));
            sb.AppendLine(string.Format("     mesh {0} ({1}), {2} vertices, {3} triangles, own lightmap UVs {4}, scale {5}", mesh.name, AssetDatabase.GetAssetPath(mesh), mesh.vertexCount, mesh.triangles.Length / 3, uv1.Length > 0, mr.transform.lossyScale.ToString("F3")));
            var mi = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(DepthSafeScenery.OriginalOf(mesh))) as ModelImporter;
            if (mi != null) sb.AppendLine(string.Format("     model {0}: generate lightmap UVs {1}, mesh compression {2}, normals {3}", mi.assetPath, mi.generateSecondaryUV, mi.meshCompression, mi.importNormals));
            sb.AppendLine(string.Format("     lightmap {0}, scale/offset {1}, scale in lightmap {2}, receive {3}, GI static {4}, batching static {5}, shadows {6}, probes {7}", mr.lightmapIndex, mr.lightmapScaleOffset.ToString("F5"), mr.scaleInLightmap, mr.receiveGI,
                GameObjectUtility.AreStaticEditorFlagsSet(mr.gameObject, StaticEditorFlags.ContributeGI), GameObjectUtility.AreStaticEditorFlagsSet(mr.gameObject, StaticEditorFlags.BatchingStatic), mr.shadowCastingMode, mr.lightProbeUsage));
            foreach (var m in mr.sharedMaterials.Where(m => m != null))
                sb.AppendLine(string.Format("     material {0}: shader {1}, keywords '{2}', queue {3}, lightmap flags {4}, double-sided GI {5}, colour {6}, texture {7}", m.name, m.shader.name, string.Join(" ", m.shaderKeywords), m.renderQueue, m.globalIlluminationFlags,
                    m.doubleSidedGI, m.color, m.mainTexture != null ? AssetDatabase.GetAssetPath(m.mainTexture) : "none"));
            if (uv.Length > 0)
            {
                Vector2 lo = uv[0], hi = uv[0];
                foreach (var p in uv) { lo = Vector2.Min(lo, p); hi = Vector2.Max(hi, p); }
                var idx = mesh.triangles;
                var v = mesh.vertices;
                var m4 = mr.transform.localToWorldMatrix;
                float uvArea = 0f, world = 0f; int up = 0, down = 0;
                var normals = mesh.normals;
                for (int i = 0; i + 2 < idx.Length; i += 3)
                {
                    Vector2 e1 = uv[idx[i + 1]] - uv[idx[i]], e2 = uv[idx[i + 2]] - uv[idx[i]];
                    uvArea += Mathf.Abs(e1.x * e2.y - e1.y * e2.x) * 0.5f;
                    var geo = Vector3.Cross(m4.MultiplyVector(v[idx[i + 1]] - v[idx[i]]), m4.MultiplyVector(v[idx[i + 2]] - v[idx[i]]));
                    world += geo.magnitude * 0.5f;
                    if (geo.y > 0f) up++; else down++;
                }
                var so = mr.lightmapScaleOffset;
                float texels = uvArea * so.x * so.y * 1024f * 1024f;
                sb.AppendLine(string.Format("     UVs from {0} to {1}, summed area {2:0.000} of the unit square (over 1 = charts overlap), world area {3:0} m2, about {4:0.00} texels/m; faces up {5}, down {6}; normals {7}, bounds {8}", lo.ToString("F3"), hi.ToString("F3"), uvArea, world,
                    Mathf.Sqrt(texels / Mathf.Max(world, 1e-3f)), up, down, normals.Length, mr.bounds));
            }
            if (mr.lightmapIndex >= 0 && mr.lightmapIndex < maps.Count)
            {
                float area = 0f, dark = 0f, sum = 0f, lowest = float.MaxValue, highest = 0f;
                Measure(mr, mesh, maps[mr.lightmapIndex], (sub, a, value) =>
                {
                    area += a; sum += value * a;
                    if (value < Dark) dark += a;
                    lowest = Mathf.Min(lowest, value); highest = Mathf.Max(highest, value);
                });
                sb.AppendLine(string.Format("     lightmap there: mean {0:0.000}, range {1:0.000}-{2:0.000}, dark {3:0.0}%", sum / Mathf.Max(area, 1e-3f), lowest, highest, 100f * dark / Mathf.Max(area, 1e-3f)));
            }
            return sb.ToString();
        }

        static void Shots(Scene scene, List<MeshRenderer> roads, StringBuilder sb)
        {
            var eye = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "CenterEyeAnchor");
            if (eye == null) { sb.AppendLine("no CenterEyeAnchor: no pictures"); return; }
            var go = new GameObject("Lightmap Report Camera") { hideFlags = HideFlags.HideAndDontSave };
            var cam = go.AddComponent<Camera>();
            var src = eye.GetComponent<Camera>();
            if (src != null) cam.CopyFrom(src);
            cam.enabled = false;
            cam.stereoTargetEye = StereoTargetEyeMask.None;
            cam.fieldOfView = 90f;
            cam.aspect = 1f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = Mathf.Max(cam.farClipPlane, 6000f);
            var rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            var tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
            try
            {
                cam.targetTexture = rt;
                Action<string, Vector3, Quaternion> shot = (name, pos, rot) =>
                {
                    cam.transform.SetPositionAndRotation(pos, rot);
                    cam.Render();
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0, false);
                    RenderTexture.active = null;
                    File.WriteAllBytes(Path.Combine(ShotFolder, name + ".png"), tex.EncodeToPNG());
                };
                for (int yaw = 0; yaw < 360; yaw += 90) shot("view_yaw" + yaw, eye.position, Quaternion.Euler(25f, yaw, 0f));
                shot("view_above", eye.position + Vector3.up * 120f, Quaternion.Euler(90f, 0f, 0f));
                if (roads.Count > 0)
                {
                    var b = roads[0].bounds;
                    foreach (var mr in roads) b.Encapsulate(mr.bounds);
                    var look = Quaternion.LookRotation(b.center - eye.position);
                    shot("view_road", eye.position, look);
                    shot("view_road_top", b.center + Vector3.up * Mathf.Max(b.extents.x, b.extents.z), Quaternion.Euler(90f, 0f, 0f));
                }
                sb.AppendLine("\npictures: " + ShotFolder + "\\view_*.png and lightmap_*.png");
            }
            finally
            {
                cam.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(go);
                rt.Release();
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        static string PathOf(Transform t)
        {
            var parts = new List<string>();
            for (; t != null; t = t.parent) parts.Insert(0, t.name);
            return string.Join("/", parts.ToArray());
        }
    }
}
