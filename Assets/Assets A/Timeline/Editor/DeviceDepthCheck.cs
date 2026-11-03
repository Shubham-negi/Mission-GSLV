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
    /// Shows in the editor what only the headset shows. The editor draws with a reversed floating-point depth buffer,
    /// the headset with a 24-bit one; drawing the launch pad twice from the learner's viewpoints, once into a 24-bit
    /// depth buffer and once into a 32-bit float one, the pixels that differ are the ones that z-fight on the headset.
    /// An id pass says which objects own them. Also renders the engine flames close up. Changes nothing in the scene.
    /// Started by a trigger file (or the menu); reports go to %TEMP%\GSLV_Depth_Check.txt and %TEMP%\gslv_shots.
    /// </summary>
    [InitializeOnLoad]
    static class DeviceDepthCheck
    {
        const string Trigger = "Assets/Assets A/Timeline/Editor/DEPTH_CHECK_ONCE.txt";
        const string FlameTrigger = "Assets/Assets A/Timeline/Editor/FLAME_SHOTS_ONCE.txt";
        const string IdShaderPath = "Assets/Assets A/Timeline/Editor/Object Id.shader";
        const string DollyClipPath = "Assets/Assets A/Timeline/Animation/Camera Dolly Moves.anim";
        static readonly string ReportFile = Path.Combine(Path.GetTempPath(), "GSLV_Depth_Check.txt");
        static readonly string ShotFolder = Path.Combine(Path.GetTempPath(), "gslv_shots");
        const int Size = 768;
        const float Fov = 90f, DeviceNear = 0.3f;
        const float Steps = 2f;   // depth rounding, in 24-bit steps: interpolation and float error on the headset add about one
        // where the learner stands while on the ground: the camera dolly's holds, then the first seconds of the climb
        static readonly float[] Times = { 0f, 30f, 56f, 74f, 83f, 97f, 114f, 126f, 145f, 158f, 161f, 164f, 168f, 173f };

        // update, not delayCall: with the editor in the background only update keeps ticking
        static DeviceDepthCheck()
        {
            if (File.Exists(Trigger) || File.Exists(FlameTrigger)) EditorApplication.update += RunFromTrigger;
        }

        static void RunFromTrigger()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating
                || File.Exists("Assets/Assets A/Timeline/Editor/BUILD_TIMELINE_ONCE.txt")) return;   // next tick; a queued Timeline rebuild goes first
            EditorApplication.update -= RunFromTrigger;
            bool flames = File.Exists(FlameTrigger), depth = File.Exists(Trigger);
            foreach (var t in new[] { Trigger, FlameTrigger })
            {
                AssetDatabase.DeleteAsset(t);
                if (File.Exists(t)) File.Delete(t);
            }
            if (flames) FlameShots();
            if (depth) Run();
        }

        static string PathOf(Transform t)
        {
            var parts = new List<string>();
            for (; t != null; t = t.parent) parts.Insert(0, t.name);
            return string.Join("/", parts.ToArray());
        }

        static Transform Find(Scene scene, string name)
        {
            return scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name);
        }

        static Camera MakeCamera(Scene scene, int w, int h)
        {
            var eye = Find(scene, "CenterEyeAnchor");
            var go = new GameObject("Depth Check Camera") { hideFlags = HideFlags.HideAndDontSave };
            var cam = go.AddComponent<Camera>();
            var src = eye != null ? eye.GetComponent<Camera>() : null;
            if (src != null) cam.CopyFrom(src);
            cam.enabled = false;
            cam.stereoTargetEye = StereoTargetEyeMask.None;
            cam.fieldOfView = Fov;
            cam.aspect = (float)w / h;
            cam.allowMSAA = false;
            cam.allowHDR = false;
            return cam;
        }

        static void SavePng(RenderTexture rt, string file, Func<int, Color32, Color32> paint = null)
        {
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            RenderTexture.active = null;
            if (paint != null)
            {
                var px = tex.GetPixels32();
                for (int i = 0; i < px.Length; i++) px[i] = paint(i, px[i]);
                tex.SetPixels32(px);
            }
            Directory.CreateDirectory(ShotFolder);
            File.WriteAllBytes(Path.Combine(ShotFolder, file), tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        // =====================================================================================
        // Engine flames, close up: copies of the flames (never saved) are run for a moment and photographed.
        // =====================================================================================
        [MenuItem("GSLV/Checks/Render Engine Flames")]
        static void FlameShots()
        {
            var sb = new StringBuilder("FLAME SHOTS " + DateTime.Now + "\n");
            var made = new List<GameObject>();
            Camera cam = null;
            RenderTexture rt = null, flat = null;
            try
            {
                var scene = SceneManager.GetActiveScene();
                bool wasDirty = scene.isDirty;
                var holder = Find(scene, "Engine Flames");
                if (holder == null) throw new Exception("no 'Engine Flames' in " + scene.path);
                var nozzleMid = Vector3.zero;
                int n = 0;
                foreach (var ps in holder.GetComponentsInChildren<ParticleSystem>(true).Where(p => p.name.StartsWith("Flame ") && !p.name.Contains("C25")))
                {
                    var copy = UnityEngine.Object.Instantiate(ps.gameObject, ps.transform.position, ps.transform.rotation);
                    foreach (var t in copy.GetComponentsInChildren<Transform>(true)) t.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    copy.SetActive(true);
                    var cps = copy.GetComponent<ParticleSystem>();
                    cps.Simulate(1.5f, true, true, true);
                    made.Add(copy);
                    sb.AppendLine(string.Format("{0}: at {1} local {2}  particles {3}", ps.name, ps.transform.position.ToString("F3"), ps.transform.localPosition.ToString("F3"), cps.particleCount));
                    nozzleMid += ps.transform.position;
                    n++;
                }
                if (n == 0) throw new Exception("no flames under Engine Flames");
                nozzleMid /= n;
                var target = nozzleMid + Vector3.up * 0.5f;

                const int w = 1280, h = 960;
                cam = MakeCamera(scene, w, h);
                cam.fieldOfView = 50f;
                cam.nearClipPlane = 0.3f;
                rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                flat = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
                var eye = Find(scene, "CenterEyeAnchor");
                Vector3 toViewer = eye != null ? Vector3.ProjectOnPlane(eye.position - target, Vector3.up).normalized : Vector3.forward;
                var views = new[] { new { name = "front", yaw = 0f, dist = 15f, up = 1.5f }, new { name = "left", yaw = -40f, dist = 15f, up = 1.5f },
                    new { name = "right", yaw = 40f, dist = 15f, up = 1.5f }, new { name = "low", yaw = 20f, dist = 10f, up = -1.0f }, new { name = "far", yaw = 0f, dist = 40f, up = 6f } };
                foreach (var v in views)
                {
                    var dir = Quaternion.Euler(0f, v.yaw, 0f) * toViewer;
                    cam.transform.position = target + dir * v.dist + Vector3.up * v.up;
                    cam.transform.LookAt(target);
                    cam.targetTexture = rt;
                    cam.Render();
                    Graphics.Blit(rt, flat);
                    SavePng(flat, "flames_" + v.name + ".png");
                }
                sb.AppendLine("shots: " + ShotFolder + "\\flames_*.png  (scene dirty before " + wasDirty + ", after " + scene.isDirty + ")");
                var flameMat = holder.GetComponentsInChildren<ParticleSystemRenderer>(true).Select(r => r.sharedMaterial).FirstOrDefault(m => m != null);
                sb.Append(StretchProbe(flameMat));
            }
            catch (Exception e) { sb.AppendLine("FAILED: " + e); }
            finally
            {
                foreach (var g in made) UnityEngine.Object.DestroyImmediate(g);
                if (cam != null) { cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(cam.gameObject); }
                if (rt != null) rt.Release();
                if (flat != null) flat.Release();
            }
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "GSLV_Flame_Shots.txt"), sb.ToString());
            Debug.Log("[GSLV] Flame shots: " + ShotFolder);
        }

        // Where Unity draws a stretched particle relative to the particle itself: one particle at a known point moving
        // straight down at 20 m/s, photographed side-on, for a few renderer settings.
        static string StretchProbe(Material flameMat)
        {
            var sb = new StringBuilder("\nSTRETCH PROBE (one particle moving down at 20 m/s, size 1, length scale 3.2, speed scale 0.16; offsets from the particle, + = up)\n");
            const int layer = 31, px = 512;
            const float half = 12f;
            var origin = new Vector3(0f, -3000f, 0f);
            var go = new GameObject("Stretch Probe") { hideFlags = HideFlags.HideAndDontSave, layer = layer };
            go.transform.position = origin;
            var camGo = new GameObject("Stretch Probe Camera") { hideFlags = HideFlags.HideAndDontSave };
            var rt = new RenderTexture(px, px, 24, RenderTextureFormat.ARGB32);
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false);
            var white = new Material(Shader.Find("Mobile/Particles/Additive")) { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var ps = go.AddComponent<ParticleSystem>();
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = ps.main;
                main.playOnAwake = false;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 4;
                var em = ps.emission; em.enabled = false;
                var shape = ps.shape; shape.enabled = false;
                var r = go.GetComponent<ParticleSystemRenderer>();
                r.renderMode = ParticleSystemRenderMode.Stretch;
                r.cameraVelocityScale = 0f;

                var cam = camGo.AddComponent<Camera>();
                cam.enabled = false;
                cam.orthographic = true;
                cam.orthographicSize = half;
                cam.cullingMask = 1 << layer;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.nearClipPlane = 0.1f; cam.farClipPlane = 100f;
                cam.transform.position = origin + Vector3.back * 30f;
                cam.transform.rotation = Quaternion.identity;
                cam.targetTexture = rt;

                var variants = new[]
                {
                    new { name = "default", pivot = Vector3.zero, length = 3.2f, speed = 0.16f },
                    new { name = "pivot y +1", pivot = new Vector3(0, 1, 0), length = 3.2f, speed = 0.16f },
                    new { name = "pivot z +1", pivot = new Vector3(0, 0, 1), length = 3.2f, speed = 0.16f },
                    new { name = "pivot x +1", pivot = new Vector3(1, 0, 0), length = 3.2f, speed = 0.16f },
                    new { name = "negative scales", pivot = Vector3.zero, length = -3.2f, speed = -0.16f },
                };
                foreach (var mat in new[] { white, flameMat })
                {
                    if (mat == null) continue;
                    r.sharedMaterial = mat;
                    foreach (var v in variants)
                    {
                        r.pivot = v.pivot;
                        r.lengthScale = v.length;
                        r.velocityScale = v.speed;
                        ps.Simulate(0.01f, true, true);   // a live (empty) system: emitting into a stopped one does nothing in edit mode
                        ps.SetParticles(new[] { new ParticleSystem.Particle { position = origin, velocity = new Vector3(0f, -20f, 0f), startSize = 1f,
                            startLifetime = 100f, remainingLifetime = 99f, startColor = Color.white } }, 1);
                        cam.Render();
                        RenderTexture.active = rt;
                        tex.ReadPixels(new Rect(0, 0, px, px), 0, 0, false);
                        RenderTexture.active = null;
                        var p = tex.GetPixels32();
                        int top = -1, bottom = -1, left = px, right = -1;
                        for (int y = 0; y < px; y++)
                            for (int x = 0; x < px; x++)
                            {
                                var c = p[y * px + x];
                                if (c.r + c.g + c.b < 30) continue;
                                if (bottom < 0) bottom = y;
                                top = y;
                                left = Math.Min(left, x); right = Math.Max(right, x);
                            }
                        float m = 2f * half / px;
                        sb.AppendLine(top < 0 ? string.Format("  {0,-16} {1,-14} nothing drawn (particles {2})", mat.name, v.name, ps.particleCount)
                            : string.Format("  {0,-16} {1,-14} from {2:+0.00;-0.00} to {3:+0.00;-0.00} m, width {4:0.00} m, centred at x {5:+0.00;-0.00}",
                                mat.name, v.name, (bottom - px / 2) * m, (top + 1 - px / 2) * m, (right - left + 1) * m, ((left + right + 1) * 0.5f - px / 2) * m));
                    }
                }
            }
            catch (Exception e) { sb.AppendLine("  FAILED: " + e.Message); }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(camGo);
                rt.Release();
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(white);
            }
            return sb.ToString();
        }

        // =====================================================================================
        // Device depth check
        // =====================================================================================
        class Fight
        {
            public string a, b;
            public int ia, ib;
            public int pixels, poses;
            public float minDist = float.MaxValue, maxDist;
            public int lastPose = -1;
        }

        class Pose
        {
            public string name;
            public Vector3 pos;
            public Quaternion rot;
            public int flagged;
        }

        [MenuItem("GSLV/Checks/Device Depth Check")]
        static void Run()
        {
            var sb = new StringBuilder();
            try { Check(sb); }
            catch (Exception e) { sb.AppendLine("CHECK FAILED: " + e); }
            File.WriteAllText(ReportFile, sb.ToString());
            Debug.Log("[GSLV] Device depth check: " + ReportFile);
        }

        static void Check(StringBuilder sb)
        {
            var scene = SceneManager.GetActiveScene();
            sb.AppendLine("DEVICE DEPTH CHECK " + DateTime.Now + "  scene " + scene.path + "  " + Size + "px, fov " + Fov + ", near " + DeviceNear);
            var env = scene.GetRootGameObjects().FirstOrDefault(r => r.name == "ENVIRONMENT");
            if (env == null) throw new Exception("no ENVIRONMENT root");
            var idShader = AssetDatabase.LoadAssetAtPath<Shader>(IdShaderPath);
            if (idShader == null || !idShader.isSupported) throw new Exception("id shader missing or unsupported: " + IdShaderPath);

            // ---- the scenery, numbered
            var names = new List<string> { "(sky / not scenery)" };
            var renderers = new List<Renderer>();
            foreach (var r in env.GetComponentsInChildren<Renderer>(false).Where(r => r.enabled && (r is MeshRenderer)))
            {
                renderers.Add(r);
                names.Add(PathOf(r.transform).Replace("ENVIRONMENT/", "") + " [" + string.Join(", ", r.sharedMaterials.Select(m => m != null ? m.name : "none").Distinct().ToArray()) + "]");
            }
            var terrains = env.GetComponentsInChildren<Terrain>(false).Where(t => t.enabled).ToList();
            var mpb = new MaterialPropertyBlock();
            for (int i = 0; i < renderers.Count; i++)
            {
                mpb.Clear();
                mpb.SetVector("_GslvId", new Vector4(i + 1, 0, 1, 0));
                renderers[i].SetPropertyBlock(mpb);
            }
            foreach (var t in terrains)
            {
                names.Add("TERRAIN " + PathOf(t.transform).Replace("ENVIRONMENT/", ""));
                mpb.Clear();
                mpb.SetVector("_GslvId", new Vector4(names.Count - 1, 0, 1, 0));
                t.SetSplatMaterialPropertyBlock(mpb);
            }
            Shader.SetGlobalVector("_GslvId", new Vector4(0, 0, 0.5f, 0));   // everything else (the rocket, the UI...)
            sb.AppendLine(string.Format("scenery: {0} mesh renderers, {1} terrains", renderers.Count, terrains.Count));

            // ---- the viewpoints
            var poses = new List<Pose>();
            var eye = Find(scene, "CenterEyeAnchor");
            var dolly = Find(scene, "Camera Dolly");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(DollyClipPath);
            if (eye == null) throw new Exception("no CenterEyeAnchor");
            Func<float, Vector3> eyeAt = time => eye.position;
            if (dolly != null && clip != null)
            {
                var curves = new[] { "x", "y", "z" }.Select(c => AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("Camera Dolly", typeof(Transform), "m_LocalPosition." + c))).ToArray();
                if (curves.All(c => c != null))
                {
                    var parent = dolly.parent;
                    var rest = dolly.localPosition;
                    eyeAt = time =>
                    {
                        var local = new Vector3(curves[0].Evaluate(time), curves[1].Evaluate(time), curves[2].Evaluate(time));
                        var delta = local - rest;
                        return eye.position + (parent != null ? parent.TransformVector(delta) : delta);
                    };
                }
                else sb.AppendLine("dolly curves not found: using the eye's resting place only");
            }
            foreach (float time in Times)
            {
                var p = eyeAt(time);
                for (int pitch = 0; pitch <= 30; pitch += 30)
                    for (int yaw = 0; yaw < 360; yaw += 45)
                        poses.Add(new Pose { name = string.Format("t{0:000} yaw{1:000} down{2:00}", time, yaw, pitch), pos = p, rot = Quaternion.Euler(pitch, yaw, 0f) });
            }

            // ---- render and compare: the id pass once with the depth the headset would store (rounded to its 24-bit
            // steps) and once without rounding; where the two show a different face, the headset can't tell them apart.
            // (The editor's own "24-bit" render targets are 32-bit float, so the rounding is done in the shader.)
            var cam = MakeCamera(scene, Size, Size);
            var c32 = new RenderTexture(Size, Size, 32, RenderTextureFormat.ARGB32);
            var exact = new RenderTexture(Size, Size, 32, RenderTextureFormat.ARGBFloat);
            var rounded = new RenderTexture(Size, Size, 32, RenderTextureFormat.ARGBFloat);
            var readI = new Texture2D(Size, Size, TextureFormat.RGBAFloat, false);
            var bufE = new float[Size * Size * 4];
            var bufQ = new float[Size * Size * 4];
            var fights = new Dictionary<long, Fight>();
            var flaggedMaps = new Dictionary<int, bool[]>();
            try
            {
                Action<RenderTexture, float[]> readId = (rt, buf) => { RenderTexture.active = rt; readI.ReadPixels(new Rect(0, 0, Size, Size), 0, 0, false); RenderTexture.active = null; readI.GetRawTextureData<float>().CopyTo(buf); };
                var clearFlags = cam.clearFlags;
                var background = cam.backgroundColor;
                Func<Pose, float, float, int[]> compare = (pose, near, steps) =>
                {
                    cam.transform.SetPositionAndRotation(pose.pos, pose.rot);
                    cam.nearClipPlane = near;
                    cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.clear;
                    Shader.SetGlobalFloat("_GslvQuant", 0f);
                    cam.targetTexture = exact; cam.RenderWithShader(idShader, "RenderType");
                    Shader.SetGlobalFloat("_GslvQuant", steps);
                    cam.targetTexture = rounded; cam.RenderWithShader(idShader, "RenderType");
                    Shader.SetGlobalFloat("_GslvQuant", 0f);
                    cam.targetTexture = null;
                    readId(exact, bufE); readId(rounded, bufQ);
                    var flagged = new List<int>();
                    for (int px = 0; px < Size * Size; px++)
                        if (bufE[px * 4] != bufQ[px * 4] || bufE[px * 4 + 1] != bufQ[px * 4 + 1]) flagged.Add(px);
                    // attribution: the true front surface and what the headset may draw instead
                    var result = new int[flagged.Count * 3];
                    for (int k = 0; k < flagged.Count; k++)
                    {
                        int px = flagged[k];
                        result[k * 3] = (int)bufE[px * 4];
                        result[k * 3 + 1] = (int)bufQ[px * 4];
                        result[k * 3 + 2] = (int)(bufE[px * 4 + 2] * 10f);   // decimetres
                    }
                    lastFlagged = flagged;
                    return result;
                };

                // calibration: the rounding must matter more with the near plane at 1 cm, or it isn't doing anything
                var calib = poses.First();
                int at30 = compare(calib, DeviceNear, Steps).Length / 3;
                int at001 = compare(calib, 0.01f, Steps).Length / 3;
                int one = compare(calib, DeviceNear, 1f).Length / 3;
                sb.AppendLine(string.Format("calibration ({0}): differing pixels rounding to {1} steps at near {2} = {3}, at near 0.01 = {4}; 1 step at near {2} = {5}{6}",
                    calib.name, Steps, DeviceNear, at30, at001, one, at001 <= at30 ? "   << NO DIFFERENCE: the rounding has no effect, results below mean nothing" : ""));

                for (int pi = 0; pi < poses.Count; pi++)
                {
                    var pose = poses[pi];
                    var res = compare(pose, DeviceNear, Steps);
                    pose.flagged = res.Length / 3;
                    if (pose.flagged > 0)
                    {
                        var map = new bool[Size * Size];
                        foreach (int px in lastFlagged) map[px] = true;
                        flaggedMaps[pi] = map;
                    }
                    for (int k = 0; k < pose.flagged; k++)
                    {
                        int ta = res[k * 3], tb = res[k * 3 + 1];
                        float dist = res[k * 3 + 2] / 10f;
                        int lo = Math.Min(ta, tb), hi = Math.Max(ta, tb);
                        long key = (long)lo * 100000 + hi;
                        Fight f;
                        if (!fights.TryGetValue(key, out f))
                            fights[key] = f = new Fight { a = Name(names, lo), b = lo == hi ? "(itself)" : Name(names, hi), ia = lo, ib = hi };
                        f.pixels++;
                        if (f.lastPose != pi) { f.poses++; f.lastPose = pi; }
                        if (dist > 0f) { f.minDist = Mathf.Min(f.minDist, dist); f.maxDist = Mathf.Max(f.maxDist, dist); }
                    }
                }

                // ---- report
                int total = poses.Sum(p => p.flagged);
                sb.AppendLine(string.Format("viewpoints: {0}, flickering pixels: {1} ({2:0.000}% of all drawn)", poses.Count, total, 100.0 * total / (poses.Count * (double)Size * Size)));
                sb.AppendLine("\nWHO FIGHTS (true front surface vs what the headset may draw instead):");
                foreach (var f in fights.Values.OrderByDescending(f => f.pixels).Take(60))
                    sb.AppendLine(string.Format("{0,8} px {1,4} views  {2,5:0}-{3,4:0} m   {4}\n                                      vs {5}", f.pixels, f.poses,
                        f.minDist == float.MaxValue ? 0 : f.minDist, f.maxDist, f.a, f.b));
                sb.AppendLine("\nHOW THE TOP PAIRS OF SEPARATE MESHES MEET:");
                foreach (var f in fights.Values.Where(f => f.ia != f.ib && f.ia > 0 && f.ib <= renderers.Count).OrderByDescending(f => f.pixels).Take(4))
                    sb.Append(PairGeometry(renderers[f.ia - 1], renderers[f.ib - 1]));
                sb.AppendLine("\nWORST VIEWPOINTS:");
                var worst = Enumerable.Range(0, poses.Count).OrderByDescending(i => poses[i].flagged).Take(10).ToList();
                foreach (int i in worst)
                    sb.AppendLine(string.Format("{0,8} px  {1}  eye {2}", poses[i].flagged, poses[i].name, poses[i].pos.ToString("F1")));

                // pictures of the worst views, flickering pixels in magenta
                foreach (int i in worst.Take(8).Where(i => poses[i].flagged > 0))
                {
                    var pose = poses[i];
                    cam.transform.SetPositionAndRotation(pose.pos, pose.rot);
                    cam.nearClipPlane = DeviceNear;
                    cam.clearFlags = clearFlags; cam.backgroundColor = background;
                    cam.targetTexture = c32; cam.Render(); cam.targetTexture = null;
                    var map = flaggedMaps[i];
                    SavePng(c32, "depth_" + pose.name.Replace(" ", "_") + ".png", (px, c) => map[px] ? new Color32(255, 0, 255, 255) : c);
                }
                sb.AppendLine("pictures: " + ShotFolder + "\\depth_*.png");
            }
            finally
            {
                foreach (var r in renderers) if (r != null) r.SetPropertyBlock(null);
                foreach (var t in terrains) if (t != null) t.SetSplatMaterialPropertyBlock(null);
                cam.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(cam.gameObject);
                foreach (var rt in new[] { c32, exact, rounded }) rt.Release();
                UnityEngine.Object.DestroyImmediate(readI);
                Shader.SetGlobalFloat("_GslvQuant", 0f);
            }

            // ---- cutout and transparent scenery: shimmer that is not z-fighting
            sb.AppendLine("\nALPHA-TESTED / TRANSPARENT SCENERY (by material):");
            foreach (var g in renderers.SelectMany(r => r.sharedMaterials.Where(m => m != null && m.renderQueue >= 2450).Select(m => new { r, m })).GroupBy(x => x.m))
            {
                var m = g.Key;
                sb.AppendLine(string.Format("  {0} ({1}, queue {2}, keywords '{3}', cutoff {4}) on {5} renderers, e.g. {6}", m.name, m.shader.name, m.renderQueue,
                    string.Join(" ", m.shaderKeywords), m.HasProperty("_Cutoff") ? m.GetFloat("_Cutoff").ToString("0.00") : "-", g.Count(), PathOf(g.First().r.transform)));
            }
        }

        static List<int> lastFlagged = new List<int>();

        // the faces of two meshes that lie within 30 cm of each other and overlap: their angle, gap, and whether they cross
        static string PairGeometry(Renderer ra, Renderer rb)
        {
            Func<Renderer, List<Vector3[]>> tris = r =>
            {
                var list = new List<Vector3[]>();
                var mf = r.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) return list;
                var m4 = r.transform.localToWorldMatrix;
                var v = mf.sharedMesh.vertices.Select(p => m4.MultiplyPoint3x4(p)).ToArray();
                var idx = mf.sharedMesh.triangles;
                for (int i = 0; i + 2 < idx.Length; i += 3) list.Add(new[] { v[idx[i]], v[idx[i + 1]], v[idx[i + 2]] });
                return list;
            };
            var A = tris(ra); var B = tris(rb);
            var sb = new StringBuilder(string.Format("  {0} ({1} faces) / {2} ({3} faces)\n", PathOf(ra.transform), A.Count, PathOf(rb.transform), B.Count));
            var bb = B.Select(t => { var b = new Bounds(t[0], Vector3.zero); b.Encapsulate(t[1]); b.Encapsulate(t[2]); b.Expand(0.6f); return b; }).ToArray();
            var buckets = new[] { 1f, 5f, 15f, 30f, 90f };
            var count = new int[buckets.Length]; var crossing = new int[buckets.Length];
            var minGap = Enumerable.Repeat(float.MaxValue, buckets.Length).ToArray();
            var area = new float[buckets.Length];
            Vector3 sample = Vector3.zero;
            foreach (var a in A)
            {
                var na = Vector3.Cross(a[1] - a[0], a[2] - a[0]);
                float aa = na.magnitude * 0.5f;
                if (aa < 1e-5f) continue;
                na /= aa * 2f;
                var ab = new Bounds(a[0], Vector3.zero); ab.Encapsulate(a[1]); ab.Encapsulate(a[2]);
                for (int j = 0; j < B.Count; j++)
                {
                    if (!bb[j].Intersects(ab)) continue;
                    var b = B[j];
                    var nb = Vector3.Cross(b[1] - b[0], b[2] - b[0]);
                    if (nb.sqrMagnitude < 1e-10f) continue;
                    nb.Normalize();
                    float angle = Vector3.Angle(na, nb); angle = Mathf.Min(angle, 180f - angle);
                    float d0 = Vector3.Dot(na, b[0] - a[0]), d1 = Vector3.Dot(na, b[1] - a[0]), d2 = Vector3.Dot(na, b[2] - a[0]);
                    float near = Mathf.Min(Mathf.Abs(d0), Mathf.Min(Mathf.Abs(d1), Mathf.Abs(d2)));
                    if (near > 0.3f) continue;
                    // does B's centre project inside A (a cheap overlap test)?
                    var c = (b[0] + b[1] + b[2]) / 3f; c -= na * Vector3.Dot(na, c - a[0]);
                    if (!Inside(c, a, na)) continue;
                    int k = 0; while (angle > buckets[k]) k++;
                    count[k]++;
                    area[k] += Mathf.Min(aa, Vector3.Cross(b[1] - b[0], b[2] - b[0]).magnitude * 0.5f);
                    bool cross = Mathf.Max(d0, Mathf.Max(d1, d2)) > 0.001f && Mathf.Min(d0, Mathf.Min(d1, d2)) < -0.001f;
                    if (cross) crossing[k]++;
                    minGap[k] = Mathf.Min(minGap[k], near);
                    sample = c;
                }
            }
            for (int k = 0; k < buckets.Length; k++)
                if (count[k] > 0)
                    sb.AppendLine(string.Format("     faces under {0,2:0} deg apart: {1,5} pairs ({2} crossing), {3:0.00} m2, closest corner {4:0.000} m", buckets[k], count[k], crossing[k], area[k], minGap[k]));
            sb.AppendLine("     e.g. at " + sample.ToString("F2"));
            return sb.ToString();
        }

        static bool Inside(Vector3 p, Vector3[] t, Vector3 n)
        {
            for (int i = 0; i < 3; i++)
                if (Vector3.Dot(Vector3.Cross(t[(i + 1) % 3] - t[i], p - t[i]), n) < 0f) return false;
            return true;
        }

        static string Name(List<string> names, int id)
        {
            return id >= 0 && id < names.Count ? names[id] : "(id " + id + ")";
        }
    }
}
