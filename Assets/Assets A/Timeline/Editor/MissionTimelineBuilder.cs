using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GslvSpace;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GslvMission.EditorTools
{
    /// <summary>
    /// GSLV > Timeline > Build Mission Timeline. Sorts the Hierarchy into folders, builds the mission UI,
    /// highlights, effects and space additions, and lays the whole storyboard on one Timeline
    /// (Assets/Assets A/Timeline/GSLV Mission.playable). The launch is one continuous full-scale flight of the
    /// scene's GSLV, which carries and deploys the 'Satellite' placed inside it, followed by a chase camera; the
    /// orbit map (the space set) is shown for the transfer and final orbits and the quiz. Everything it writes
    /// lives in Assets/Assets A and in the open scene (which must be in Assets/Assets A).
    /// </summary>
    [InitializeOnLoad]
    static class MissionTimelineBuilder
    {
        const string Folder = "Assets/Assets A/Timeline";
        const string AnimFolder = Folder + "/Animation";
        const string MatFolder = Folder + "/Materials";
        const string TexFolder = Folder + "/Textures";
        const string TimelinePath = Folder + "/GSLV Mission.playable";
        const string Trigger = Folder + "/Editor/BUILD_TIMELINE_ONCE.txt";
        const string MainScenePath = "Assets/Assets A/Scene/Scene 1.unity";
        const string VoFolder = "Assets/Assets A/VoiceOver";
        const string SfxFolder = "Assets/Assets A/SFX";
        static string GslvModelPath { get { return AssetDatabase.GUIDToAssetPath("58a30b1eac53f624ebfacef5314fbd33"); } }   // gslv rocket model com.fbx, wherever it is moved
        const string MeshFolder = Folder + "/Meshes";
        const string MergedName = "Merged Parts";
        const string ProbePath = "Assets/Assets A/Space/Models/Probe.FBX";
        const string ProbeTexPath = "Assets/Assets A/Space/Models/Probe.png";
        const string KitFolder = "Assets/Assets A/UI/GSLV UI Kit";   // sprites of the Mission GSLV UI sheets (drawn by Source~/MakeUiKit.ps1)
        const float SkinGlow = 16f;          // transparent glow margin around the kit's 9-slice skins, in canvas pixels
        const string UndoName = "Build Mission Timeline";

        const float UiScale = 0.8f;          // the whole VR UI, as a share of its first size (positions stay)
        const float Px = 0.0065f * UiScale;  // metres per canvas pixel
        const float ButtonScale = 0.85f;     // the action buttons (Explore, Launch, Deploy, end menu), on top of UiScale
        const float UiDistance = 8f;
        const float SideBearing = 24f;      // degrees right of the learner's forward direction

        // sound levels (Timeline clip / one-shot volumes, 0-1) and the launch roar's duck under the lift-off voice-over
        const string ClickSound = "Mission_GSLV_Rocket_UI_Button_Click.wav";
        const string TickSound = "Mission_GSLV_Task_Check_Confirmed.wav";
        const string AlarmSound = "warnig alarm .mp3";
        const float ClickVolume = 0.8f, TickVolume = 0.7f, AlarmVolume = 0.4f;
        const float LaunchLoudVolume = 1f, LaunchBedVolume = 0.22f;
        const double LaunchLoudHold = 1.5;   // s at full volume after lift-off
        const double LaunchDuckFade = 1.5;   // s to fade down to the bed
        const double LaunchVoGap = 0.2;      // s between the end of the fade and the lift-off voice-over

        // a part swells a little about its centre while the voice-over introduces it
        const float EmphasisScale = 1.08f;
        const double EmphasisRamp = 0.6;     // s to swell, and to settle back

        // the fairing turns see-through while the voice-over shows the satellite inside it
        const float FairingMinOpacity = 0.2f;
        const float FairingFadeTime = 1f;
        // Spacecraft on the orbit map, in the map's units (Earth's radius there is 2.2): a diagram, but small enough to read as craft beside a planet.
        const float MapRocketLength = 0.8f;
        const float MapStageLength = 0.5f;
        const float MapSatelliteSize = 0.6f;
        const float SpaceScale = 5f;         // orbit map enlarged around the learner's viewpoint, so everything stays beyond the UI
        const float BackdropRadius = 600f;  // Earth below the rocket during the flight in space

        static readonly Color Cyan = new Color(0.30f, 0.88f, 1f, 1f);
        static readonly Color White = new Color(0.94f, 0.98f, 1f, 1f);
        static readonly Color Green = new Color(0.25f, 0.92f, 0.50f, 1f);
        static readonly Color Amber = new Color(1f, 0.72f, 0.18f, 1f);
        static readonly Color Grey = new Color(0.40f, 0.46f, 0.52f, 1f);
        static readonly Color Gold = new Color(1f, 0.82f, 0.25f, 1f);

        static readonly string StatusFile = Path.Combine(Path.GetTempPath(), "GSLV_Timeline_Build.txt");
        static readonly string TimesFile = Path.Combine(Path.GetTempPath(), "GSLV_Timeline_Times.txt");

        // ---- state shared by the build steps
        static Scene scene;
        static StringBuilder report;
        static Dictionary<string, GameObject> O;
        static Dictionary<string, double> Times;
        static Transform rig, dollyHost, dolly, uiAnchor, effectsFolder, modelsFolder, spaceFolder;
        static Transform modelRoot, padHost, padLift, earthSystem, flightPath, fp1, fp2, fp3, gto, deployPoint, slot, satellite;
        static Transform spaceCamHost, spaceCam, flightSat, backdrop;

        // Timeline times of the flight's events, set while the storyboard is laid out.
        class Flight
        {
            public double lift, swap, l110Ign, s200, fair, l110Sep, c25Ign, map1, flight2, sepStart, map2;
            public Func<double, double> altKm;   // Timeline time -> the real altitude the flight stands for, in km
        }
        static Flight fl;
        static GameObject voSource, feedbackSource;
        static SpaceEnvironmentSwitcher switcher;
        static MissionFlow flow;
        static QuizPanel quiz;
        static PlayableDirector dir;
        static TimelineAsset tl;
        static AudioTrack voTrack;
        static List<AudioTrack> sfxTracks;            // sound-effect lanes, so overlapping effects never cross-fade
        static List<List<double[]>> sfxSpans;
        static Quaternion upright;            // rotation that stands the GSLV model up (taken from the user's instance)
        static float heightPerScale;          // model height in metres per 1.0 of local scale
        static Material hologramMat, fadeMat, flameMat, smokeMat, puffSmokeMat, cloudMat, skyMat;
        static Material[] sourceMats, glowMats;
        static readonly List<string> warnings = new List<string>();

        static MissionTimelineBuilder()
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
            Run(false);
        }

        // Applies the improved plume to the already-generated scene without rebuilding the mission. This is kept
        // separate because a plume-only visual update should not depend on source FBX files or replace other work.
        [MenuItem("GSLV/Timeline/Upgrade Engine Plumes")]
        static void UpgradeLaunchPlumes()
        {
            Scene target = SceneManager.GetSceneByPath(MainScenePath);
            bool openedHere = !target.IsValid() || !target.isLoaded;
            if (openedHere) target = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Additive);
            bool wasDirty = target.isDirty;
            try
            {
                EnsureFolder(MatFolder);
                EnsureFolder(TexFolder);
                var flame = FlameTexture();
                var material = LoadOrMakeMaterial("Engine Flame", "Mobile/Particles/Additive", m => m.SetTexture("_MainTex", flame));
                material.SetTexture("_MainTex", flame);
                if (material.HasProperty("_TintColor")) material.SetColor("_TintColor", Color.white);
                EditorUtility.SetDirty(material);

                var particles = target.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ParticleSystem>(true)).ToArray();
                Action<string, float, Color, Color, float, bool> apply = (name, size, hot, mid, strength, attached) =>
                {
                    // "Flame L110" is two flames, "Flame L110 A" and "B", one per Vikas engine
                    var found = particles.Where(p => p.name == name || p.name.StartsWith(name + " ")).ToArray();
                    if (found.Length == 0) throw new Exception("plume object not found in Scene 1: " + name);
                    foreach (var ps in found)
                    {
                        ConfigureFlame(ps, size, hot, mid, strength, attached, material);
                        EditorUtility.SetDirty(ps);
                        EditorUtility.SetDirty(ps.GetComponent<ParticleSystemRenderer>());
                    }
                };
                apply("Flame S200 R", 2.0f, new Color(1f, 1f, 0.88f), new Color(1f, 0.48f, 0.055f), 1f, false);
                apply("Flame S200 L", 2.0f, new Color(1f, 1f, 0.88f), new Color(1f, 0.48f, 0.055f), 1f, false);
                apply("Flame L110", 2.0f, new Color(1f, 0.94f, 0.76f), new Color(1f, 0.30f, 0.035f), 0.82f, true);
                apply("Flame C25", 1.7f, new Color(0.88f, 0.96f, 1f), new Color(0.28f, 0.46f, 1f), 0.72f, true);

                EditorSceneManager.MarkSceneDirty(target);
                AssetDatabase.SaveAssets();
                if (!wasDirty) EditorSceneManager.SaveScene(target);
                Debug.Log("[GSLV] Upgraded the four engine plumes in Scene 1" + (wasDirty ? "; scene was already dirty, press Ctrl+S to keep it." : "."));
            }
            catch (Exception e)
            {
                Debug.LogError("[GSLV] Engine plume upgrade failed: " + e);
            }
            finally
            {
                if (openedHere && target.IsValid() && target.isLoaded) EditorSceneManager.CloseScene(target, true);
            }
        }

        [MenuItem("GSLV/Timeline/Build Mission Timeline")]
        static void MenuBuild()
        {
            Run(true);
        }

        internal static void Run(bool ask)
        {
            report = new StringBuilder();
            warnings.Clear();
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) { Stop("Stop Play mode first."); return; }
                scene = EditorSceneManager.GetActiveScene();
                if (!scene.path.StartsWith("Assets/Assets A/")) { Stop("The open scene is not inside Assets/Assets A, so nothing was changed."); return; }
                string missing = MissingInputs();
                if (missing != null) { Stop(missing); return; }

                bool rebuild = FindRoot("TIMELINE") != null;
                if (rebuild && ask && !EditorUtility.DisplayDialog("Mission Timeline", "The mission Timeline was built before. Rebuild it? The generated UI, effects and Timeline are deleted and recreated.", "Rebuild", "Cancel"))
                    return;

                bool wasDirty = scene.isDirty;
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName(UndoName);
                int group = Undo.GetCurrentGroup();
                try
                {
                    Build(rebuild);
                }
                catch (Exception e)
                {
                    Undo.RevertAllDownToGroup(group);
                    Stop("Build failed and the scene changes were rolled back:\n" + e);
                    return;
                }
                Undo.CollapseUndoOperations(group);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!wasDirty) EditorSceneManager.SaveScene(scene);
                report.AppendLine();
                report.AppendLine(wasDirty ? "Scene NOT saved (it had unsaved changes before): press Ctrl+S." : "Scene saved.");
                Finish();
            }
            catch (Exception e)
            {
                Stop("Unexpected failure: " + e);
            }
        }

        static void Stop(string reason)
        {
            report.AppendLine("STOPPED: " + reason);
            Finish();
        }

        static void Finish()
        {
            foreach (var w in warnings) report.AppendLine("WARNING: " + w);
            File.WriteAllText(StatusFile, report.ToString());
            Debug.Log("[GSLV] Mission Timeline build: " + StatusFile);
        }

        static string MissingInputs()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(GslvModelPath) == null) return "GSLV model missing: " + GslvModelPath;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ProbePath) == null) return "Satellite model missing: " + ProbePath;
            if (AssetDatabase.FindAssets("t:AudioClip", new[] { VoFolder }).Length < 50) return "Voice-over clips missing in " + VoFolder;
            if (FindT("SPACE ENVIRONMENT") == null) return "SPACE ENVIRONMENT is not in the scene (run GSLV > Build Space Environment first).";
            if (!Directory.Exists(KitFolder) || Directory.GetFiles(KitFolder, "*.png").Length < 40) return "UI kit sprites missing in " + KitFolder + " (run its Source~/MakeUiKit.ps1).";
            // Checked before anything is deleted, so a renamed or missing part stops the build cleanly instead of half-way.
            var model = FindT("gslv rocket model com");
            if (model == null) return "The GSLV model 'gslv rocket model com' is not in the scene.";
            var missing = new[] { "Louncher.001", "Louncher.002", "Louncher.003", "Louncher.004", "Louncher.005", "Louncher.006", "Louncher.009", "J_2S_J_2_0", "Empty.001" }
                .Where(n => FindPart(model, n) == null).ToArray();
            if (missing.Length > 0) return "Parts missing from the GSLV model (renamed?): " + string.Join(", ", missing) + ". Nothing was changed.";
            return null;
        }

        // Other names the user has given a part in the scene's GSLV (the space copies keep the model's own names).
        static readonly Dictionary<string, string> PartAliases = new Dictionary<string, string> { { "J_2S_J_2_0", "Middle engine" } };

        static Transform FindPart(Transform root, string name)
        {
            var t = FindUnder(root, name);
            string alias;
            if (t == null && PartAliases.TryGetValue(name, out alias) && root != null)   // renamed by hand: ignore stray spaces and capitals
                t = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => string.Equals(x.name.Trim(), alias, StringComparison.OrdinalIgnoreCase));
            return t;
        }

        // =====================================================================================
        static void Build(bool rebuild)
        {
            O = new Dictionary<string, GameObject>();
            Times = new Dictionary<string, double>();
            EnsureFolder(AnimFolder);
            EnsureFolder(MatFolder);
            EnsureFolder(TexFolder);
            if (rebuild) Cleanup();

            Reorganize();
            PrepareAssets();
            BuildModels();
            BuildSpaceAdditions();
            BuildFlightBackdrop();
            BuildAudio();
            BuildDirector();
            PrepareUiKit();
            BuildUi();
            BuildHighlights();
            BuildEffects();
            RegisterSpaceObjects();
            BuildTimeline();
            WireFlow();
            SetInitialStates();
            SaveAssets();
        }

        static void Cleanup()
        {
            foreach (string n in new[] { "UI", "EFFECTS", "AUDIO", "TIMELINE" })
            {
                var f = FindRoot(n);
                if (f != null) Undo.DestroyObjectImmediate(f.gameObject);
            }
            foreach (string n in new[] { "Ascent Rocket", "Showcase Rocket", "GTO Glow", "Upper Stage (Deploy)", "Satellite Hologram", "Engine Flames", "Launch Smoke", "Space Camera Host", "Flight Backdrop", "Sky Atmosphere", "Cloud Layer" })
            {
                var t = FindT(n);
                if (t != null) Undo.DestroyObjectImmediate(t.gameObject);
            }
            AssetDatabase.DeleteAsset(TimelinePath);
            AssetDatabase.DeleteAsset(MatFolder + "/Hologram.mat");
            foreach (string g in AssetDatabase.FindAssets("t:AnimationClip", new[] { AnimFolder })) AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(g));
        }

        // ---------------------------------------------------------------- hierarchy folders
        static void Reorganize()
        {
            var cameraFolder = MakeFolder("CAMERA");
            var lights = MakeFolder("LIGHTS");
            var environment = MakeFolder("ENVIRONMENT");
            modelsFolder = MakeFolder("MODELS");
            spaceFolder = MakeFolder("SPACE");
            MakeFolder("UI");
            effectsFolder = MakeFolder("EFFECTS");
            MakeFolder("AUDIO");
            MakeFolder("TIMELINE");

            // camera: CAMERA (Animator) / Camera Dolly (moved by the Timeline) / QwrCameraRig
            rig = FindRig();
            if (rig == null) throw new Exception("QwrCameraRig not found in the scene.");
            dollyHost = cameraFolder;
            dolly = FindUnder(cameraFolder, "Camera Dolly");
            if (dolly == null) dolly = NewObject("Camera Dolly", cameraFolder).transform;
            Reparent(rig, dolly);
            if (dollyHost.GetComponent<Animator>() == null) Undo.AddComponent<Animator>(dollyHost.gameObject);

            var sun = FindT("Directional Light");
            if (sun != null) Reparent(sun, lights);
            var env = FindT("launching env");
            if (env != null) Reparent(env, environment);
            var space = FindT("SPACE ENVIRONMENT");
            Reparent(space, spaceFolder);
            switcher = space.GetComponent<SpaceEnvironmentSwitcher>();

            string[] order = { "CAMERA", "LIGHTS", "ENVIRONMENT", "MODELS", "SPACE", "UI", "EFFECTS", "AUDIO", "TIMELINE" };
            for (int i = 0; i < order.Length; i++) FindRoot(order[i]).SetSiblingIndex(i);
            report.AppendLine("Hierarchy folders: " + string.Join(", ", order));
        }

        static Transform MakeFolder(string name)
        {
            var existing = FindRoot(name);
            if (existing != null) return existing;
            var go = NewObject(name, null);
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return go.transform;
        }

        static Transform FindRoot(string name)
        {
            foreach (var r in scene.GetRootGameObjects()) if (r.name == name) return r.transform;
            return null;
        }

        static Transform FindT(string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    if (t.name == name) return t;
            return null;
        }

        static Transform FindUnder(Transform parent, string name)
        {
            return parent == null ? null : parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
        }

        static Transform FindRig()
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var c in root.GetComponentsInChildren<qwr.QwrCameraRig>(true))
                {
                    bool inside = false;
                    for (var p = c.transform.parent; p != null; p = p.parent) if (p.name == "launching env") inside = true;
                    if (!inside && c.gameObject.activeSelf) return c.transform;
                }
            return null;
        }

        static void Reparent(Transform t, Transform parent)
        {
            if (t.parent != parent) Undo.SetTransformParent(t, parent, UndoName);
        }

        static GameObject NewObject(string name, Transform parent, params Type[] components)
        {
            var go = new GameObject(name, components);
            Undo.RegisterCreatedObjectUndo(go, UndoName);
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        static GameObject Reg(string key, GameObject go)
        {
            O[key] = go;
            return go;
        }

        static GameObject G(string key)
        {
            GameObject go;
            if (!O.TryGetValue(key, out go) || go == null) throw new Exception("internal: missing object '" + key + "'");
            return go;
        }

        static void SetInactive(GameObject go)
        {
            Undo.RecordObject(go, UndoName);
            go.SetActive(false);
        }

        // ---------------------------------------------------------------- shared assets
        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static Material LoadOrMakeMaterial(string name, string shader, Action<Material> setup)
        {
            string path = MatFolder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            var sh = Shader.Find(shader);
            if (sh == null) throw new Exception("shader not found: " + shader);
            m = new Material(sh) { name = name };
            setup(m);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static Texture2D SoftDot()
        {
            string path = TexFolder + "/SoftDot.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32f, 32f)) / 32f;
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a * (3f - 2f * a);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // A long, soft-edged flame tongue. The baked turbulence and shock-cell bands make the exhaust read as
        // energetic without a costly distortion shader or extra particle layers (important for two-eye VR rendering).
        static Texture2D FlameTexture()
        {
            string path = TexFolder + "/Engine Flame Tongue.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;

            const int W = 128, H = 256;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float u = (x + 0.5f) / W;
                    float v = (y + 0.5f) / H;
                    float across = Mathf.Abs(u * 2f - 1f);

                    // Pinched at the nozzle and feathered at the end, with two scales of baked-in turbulence.
                    float envelope = Mathf.Sin(Mathf.PI * Mathf.Pow(v, 0.82f));
                    float width = Mathf.Lerp(0.17f, 0.74f, Mathf.Pow(envelope, 0.62f));
                    float coarse = Mathf.PerlinNoise(u * 4.5f + 11.3f, v * 8f + 2.7f);
                    float fine = Mathf.PerlinNoise(u * 13f + 37.1f, v * 22f + 19.4f);
                    width *= Mathf.Lerp(0.82f, 1.12f, coarse * 0.7f + fine * 0.3f);
                    float edge = Mathf.Clamp01((width - across) / 0.20f);
                    edge = edge * edge * (3f - 2f * edge);

                    float endFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(v * 9f))
                        * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - v) * 4f));
                    float core = Mathf.Exp(-across * across * 7.5f);
                    // Bright compression cells hint at the diamond pattern visible in high-speed rocket exhaust.
                    float cells = Mathf.Pow(Mathf.Max(0f, Mathf.Sin((v * 5.2f + 0.08f) * Mathf.PI)), 7f);
                    float brightness = Mathf.Clamp01(0.48f + core * 0.42f + cells * core * 0.28f);
                    float alpha = edge * endFade * Mathf.Lerp(0.68f, 1f, core);
                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, alpha));
                }

            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = true;       // avoids shimmer in the headset as the plume recedes
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Compressed;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static void PrepareAssets()
        {
            var dot = SoftDot();
            var flame = FlameTexture();
            var probeTex = AssetDatabase.LoadAssetAtPath<Texture2D>(ProbeTexPath);
            hologramMat = LoadOrMakeMaterial("Satellite Hologram", "Standard", m =>
            {
                m.SetFloat("_Mode", 2f);   // Fade
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = 3000;
                m.color = new Color(0.25f, 0.85f, 1f, 0.55f);
                m.SetFloat("_Glossiness", 0.2f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", new Color(0.1f, 0.65f, 0.9f));
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                if (probeTex != null) m.SetTexture("_MainTex", probeTex);
            });
            fadeMat = LoadOrMakeMaterial("Screen Fade", "OverlayUI", m => m.color = Color.black);
            flameMat = LoadOrMakeMaterial("Engine Flame", "Mobile/Particles/Additive", m => m.SetTexture("_MainTex", flame));
            // LoadOrMakeMaterial preserves existing assets, so refresh these properties on rebuild as well.
            flameMat.SetTexture("_MainTex", flame);
            if (flameMat.HasProperty("_TintColor")) flameMat.SetColor("_TintColor", Color.white);
            EditorUtility.SetDirty(flameMat);
            smokeMat = LoadOrMakeMaterial("Exhaust Smoke", "Mobile/Particles/Alpha Blended", m => m.SetTexture("_MainTex", dot));
            var smokeTex = PuffTexture("Smoke Puff", 14, 0.55f, 7);
            var cloudTex = PuffTexture("Cloud Puff", 18, 0.45f, 21);
            Action<Material, Texture2D> particles = (m, tex) => { m.SetTexture("_MainTex", tex); m.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f)); };
            puffSmokeMat = LoadOrMakeMaterial("Launch Smoke", "Legacy Shaders/Particles/Alpha Blended", m => particles(m, smokeTex));
            cloudMat = LoadOrMakeMaterial("Cloud Layer", "Legacy Shaders/Particles/Alpha Blended", m => particles(m, cloudTex));
            puffSmokeMat.SetColor("_TintColor", new Color(0.56f, 0.56f, 0.56f, 0.5f));   // a little brighter than neutral: launch clouds are white steam and smoke
            cloudMat.SetColor("_TintColor", new Color(0.56f, 0.56f, 0.56f, 0.5f));
            skyMat = LoadOrMakeMaterial("Sky Atmosphere", "GSLV/Sky Atmosphere",
                m => m.SetTexture("_StarTex", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Assets A/Space/Skybox/2k_stars_milky_way.jpg")));
        }

        // A billowy puff for smoke and clouds: soft overlapping blobs broken up by noise, a little brighter on top (lit from above).
        static Texture2D PuffTexture(string name, int blobs, float noise, int seed)
        {
            string path = TexFolder + "/" + name + ".png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;
            const int N = 256;
            var rnd = new System.Random(seed);
            var centres = new List<Vector3>();   // x, y and radius, in texture units
            for (int i = 0; i < blobs; i++)
            {
                float a = (float)(rnd.NextDouble() * Math.PI * 2.0), r = (float)(rnd.NextDouble() * 0.22);
                centres.Add(new Vector3(0.5f + Mathf.Cos(a) * r, 0.5f + Mathf.Sin(a) * r * 0.8f, 0.14f + (float)rnd.NextDouble() * 0.16f));
            }
            float ox = (float)rnd.NextDouble() * 50f, oy = (float)rnd.NextDouble() * 50f;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float u = (x + 0.5f) / N, v = (y + 0.5f) / N, d = 0f;
                    foreach (var c in centres)
                        d += Mathf.Exp(-((u - c.x) * (u - c.x) + (v - c.y) * (v - c.y)) / (c.z * c.z) * 1.6f);
                    float n = Mathf.PerlinNoise(ox + u * 6f, oy + v * 6f) * 0.65f + Mathf.PerlinNoise(ox + u * 14f, oy + v * 14f) * 0.35f;
                    float edge = Mathf.Clamp01(1f - Mathf.Pow(Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f, 4f));
                    float alpha = Mathf.Clamp01((d * (1f - noise + noise * n) - 0.15f) * 1.2f) * edge;
                    float shade = Mathf.Lerp(0.70f, 1f, v) * (0.85f + 0.15f * n);
                    tex.SetPixel(x, y, new Color(shade, shade, shade, alpha));
                }
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.alphaIsTransparency = true;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // Emissive copy of a material (emission map = its albedo), used by PartHighlight.
        static Material GlowCopy(Material source)
        {
            string path = MatFolder + "/" + source.name + " (Glow).mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var m = new Material(source) { name = source.name + " (Glow)" };
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.white);
            var main = source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
            if (main != null) m.SetTexture("_EmissionMap", main);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        // ---------------------------------------------------------------- models
        static void BuildModels()
        {
            // the user's GSLV (a prefab instance or unpacked); the space copies are called "GSLV Model"
            var candidates = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .Where(t => t.name == "gslv rocket model com").ToList();
            var userModel = candidates.FirstOrDefault(t => t.IsChildOf(modelsFolder)) ?? candidates.FirstOrDefault();
            if (userModel == null) throw new Exception("The GSLV model 'gslv rocket model com' is not in the scene.");
            modelRoot = userModel;

            padHost = FindUnder(modelsFolder, "GSLV Rocket (Pad)");
            if (padHost == null) padHost = NewObject("GSLV Rocket (Pad)", modelsFolder).transform;
            if (padHost.GetComponent<Animator>() == null) Undo.AddComponent<Animator>(padHost.gameObject);
            padLift = FindUnder(padHost, "Lift");
            if (padLift == null) padLift = NewObject("Lift", padHost).transform;
            Reparent(modelRoot, padLift);

            // Lift pivots at the base of the rocket, so the flight can pitch the rocket over about it (children keep their place).
            var rest = WorldBounds(modelRoot);
            Vector3 pivot = new Vector3(rest.center.x, rest.min.y, rest.center.z);
            if ((padLift.position - pivot).sqrMagnitude > 0.0001f)
            {
                var kids = padLift.Cast<Transform>().ToList();
                var keep = kids.Select(k => k.position).ToList();
                Undo.RecordObject(padLift, UndoName);
                padLift.position = pivot;
                for (int i = 0; i < kids.Count; i++)
                {
                    Undo.RecordObject(kids[i], UndoName);
                    kids[i].position = keep[i];
                }
            }

            // the satellite the user placed inside the fairing; the flight deploys it
            flightSat = FindUnder(modelRoot, "Satellite");
            if (flightSat == null) warnings.Add("no 'Satellite' inside the GSLV model, so the full-scale deployment only moves the upper stage");

            // The satellite and the C25 engine are very heavy meshes hidden inside the rocket until it is in space: the Timeline
            // switches them on only then. They are on while building, so their sizes and positions can be measured.
            var engine = FindPart(modelRoot, "J_2S_J_2_0");
            foreach (var heavy in new[] { flightSat, engine })
                if (heavy != null && !heavy.gameObject.activeSelf)
                {
                    Undo.RecordObject(heavy.gameObject, UndoName);
                    heavy.gameObject.SetActive(true);
                }
            if (flightSat != null) MergeParts(flightSat);
            if (flightSat != null) Reg("Satellite Model", flightSat.gameObject);
            if (engine != null) Reg("C25 Engine Model", engine.gameObject);

            upright = modelRoot.localRotation;
            var b = WorldBounds(modelRoot);
            heightPerScale = b.size.y / modelRoot.lossyScale.y;
            report.AppendLine(string.Format("GSLV pad model: height {0:0.0} m, {1:0.0} model units per 1.0 scale", b.size.y, heightPerScale));

            sourceMats = modelRoot.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
            glowMats = sourceMats.Select(GlowCopy).ToArray();

            // Satellite hologram beside the learner (Scene 1).
            var probe = AssetDatabase.LoadAssetAtPath<GameObject>(ProbePath);
            var holo = (GameObject)PrefabUtility.InstantiatePrefab(probe);
            Undo.RegisterCreatedObjectUndo(holo, UndoName);
            holo.name = "Satellite Hologram";
            holo.transform.SetParent(modelsFolder, false);
            foreach (Transform t in holo.GetComponentsInChildren<Transform>(true))
                if (t.name.IndexOf("fold", StringComparison.OrdinalIgnoreCase) >= 0) t.gameObject.SetActive(false);
            foreach (var r in holo.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                var mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = hologramMat;
                r.sharedMaterials = mats;
            }
            Fit(holo.transform, 2.0f);
            // park it where the learner sees it: left of the rocket, ahead and slightly below eye level
            float yaw = rig.eulerAngles.y;
            Vector3 fwd = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 pos = rig.position + Quaternion.Euler(0f, yaw - 18f, 0f) * Vector3.forward * 4.5f + Vector3.down * 0.5f;
            holo.transform.position = pos;
            holo.transform.rotation = Quaternion.Euler(0f, yaw + 180f, 0f);
            Undo.AddComponent<SlowSpin>(holo);
            var spin = holo.GetComponent<SlowSpin>();
            spin.speed = 25f;
            Reg("Hologram", holo);
        }

        // The user's satellite is 2,273 small meshes (panels, foil, bolts): a draw call each, for each eye, so the headset
        // stutters whenever it is in view. Its parts are drawn as one mesh instead, one submesh per material, in "Merged Parts"
        // under the satellite; the parts stay in the hierarchy, switched off. Nothing animates them on their own (the Timeline
        // moves the satellite as a whole), so it looks and moves the same.
        static void MergeParts(Transform root)
        {
            var old = root.Find(MergedName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var groups = root.Cast<Transform>().ToList();
            var parts = groups.SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)
                .Where(r => r.enabled && ShownWithin(r.transform, g) && r.GetComponent<MeshFilter>() != null && r.GetComponent<MeshFilter>().sharedMesh != null)).ToList();
            if (parts.Count < 2) return;

            var mats = new List<Material>();
            var tris = new List<List<int>>();
            var verts = new List<Vector3>(); var normals = new List<Vector3>(); var tangents = new List<Vector4>(); var uv = new List<Vector2>(); var colors = new List<Color>();
            bool anyColor = parts.Any(r => r.GetComponent<MeshFilter>().sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color));
            foreach (var r in parts)
            {
                var mesh = r.GetComponent<MeshFilter>().sharedMesh;
                var m = root.worldToLocalMatrix * r.transform.localToWorldMatrix;
                var nm = m.inverse.transpose;
                bool mirrored = m.determinant < 0f;   // a mirrored part: its triangles turn the other way round
                int start = verts.Count;
                var v = mesh.vertices; var n = mesh.normals; var t = mesh.tangents; var u = mesh.uv; var c = mesh.colors;
                for (int i = 0; i < v.Length; i++)
                {
                    verts.Add(m.MultiplyPoint3x4(v[i]));
                    normals.Add(n.Length == v.Length ? nm.MultiplyVector(n[i]).normalized : Vector3.up);
                    Vector3 tv = t.Length == v.Length ? m.MultiplyVector(t[i]).normalized : Vector3.right;
                    tangents.Add(new Vector4(tv.x, tv.y, tv.z, (t.Length == v.Length ? t[i].w : 1f) * (mirrored ? -1f : 1f)));
                    uv.Add(u.Length == v.Length ? u[i] : Vector2.zero);
                    if (anyColor) colors.Add(c.Length == v.Length ? c[i] : Color.white);
                }
                var shared = r.sharedMaterials;
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    var mat = s < shared.Length ? shared[s] : shared.LastOrDefault();
                    int k = mats.IndexOf(mat);
                    if (k < 0) { mats.Add(mat); tris.Add(new List<int>()); k = mats.Count - 1; }
                    var idx = mesh.GetTriangles(s);
                    for (int i = 0; i + 2 < idx.Length; i += 3)
                    {
                        tris[k].Add(start + idx[i]);
                        tris[k].Add(start + idx[mirrored ? i + 2 : i + 1]);
                        tris[k].Add(start + idx[mirrored ? i + 1 : i + 2]);
                    }
                }
            }
            var merged = new Mesh { name = root.name + " (merged parts)" };
            if (verts.Count > 65535) merged.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            merged.SetVertices(verts);
            merged.SetNormals(normals);
            merged.SetTangents(tangents);
            merged.SetUVs(0, uv);
            if (anyColor) merged.SetColors(colors);
            merged.subMeshCount = mats.Count;
            for (int k = 0; k < mats.Count; k++) merged.SetTriangles(tris[k], k, false);
            merged.RecalculateBounds();

            EnsureFolder(MeshFolder);
            string path = MeshFolder + "/" + root.name + " (merged parts).asset";
            var asset = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (asset == null) { AssetDatabase.CreateAsset(merged, path); asset = merged; }
            else { EditorUtility.CopySerialized(merged, asset); Object.DestroyImmediate(merged); }   // same asset, so its GUID stays

            var go = NewObject(MergedName, root, typeof(MeshFilter), typeof(MeshRenderer));
            go.layer = parts[0].gameObject.layer;
            go.GetComponent<MeshFilter>().sharedMesh = asset;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterials = mats.ToArray();
            mr.shadowCastingMode = parts[0].shadowCastingMode;
            mr.receiveShadows = parts[0].receiveShadows;
            mr.lightProbeUsage = parts[0].lightProbeUsage;
            mr.reflectionProbeUsage = parts[0].reflectionProbeUsage;
            foreach (var g in groups)
                if (g.gameObject.activeSelf && g.GetComponentsInChildren<MeshRenderer>(true).Any(parts.Contains)) SetInactive(g.gameObject);
            report.AppendLine(string.Format("{0}: {1} parts merged into one mesh ({2} materials, {3:N0} triangles)", root.name, parts.Count, mats.Count, tris.Sum(l => l.Count) / 3));
        }

        // t is switched on below root (root's own switch is not looked at)
        static bool ShownWithin(Transform t, Transform root)
        {
            for (; t != null && t != root; t = t.parent)
                if (!t.gameObject.activeSelf) return false;
            return true;
        }

        static void Fit(Transform t, float target)
        {
            var b = WorldBounds(t);
            float max = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            if (max > 0.0001f) t.localScale *= target / max;
        }

        static Bounds WorldBounds(Transform t)
        {
            var rs = t.GetComponentsInChildren<Renderer>(false);
            if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        // The mouths of a part's rocket nozzles, in world space: the rings of vertices along the bottom of its meshes, one
        // ring per nozzle (the L110 core has two Vikas engines side by side). The bounds of a part are no guide: a booster's
        // include its side strut, so their centre is off the nozzle, and the core's centre falls between its two nozzles.
        struct Nozzle
        {
            public Vector3 exit;   // centre of the mouth
            public float radius;
        }

        static List<Nozzle> NozzleExits(Transform part)
        {
            var pts = new List<Vector3>();
            foreach (var mf in part.GetComponentsInChildren<MeshFilter>(false))
            {
                if (mf.sharedMesh == null) continue;
                var m4 = mf.transform.localToWorldMatrix;
                try { pts.AddRange(mf.sharedMesh.vertices.Select(v => m4.MultiplyPoint3x4(v))); }
                catch (Exception) { }   // an unreadable mesh: fall back to the bounds below
            }
            if (pts.Count == 0)
            {
                var b = WorldBounds(part);
                return new List<Nozzle> { new Nozzle { exit = new Vector3(b.center.x, b.min.y, b.center.z), radius = Mathf.Min(b.extents.x, b.extents.z) } };
            }
            float minY = pts.Min(p => p.y);
            var lip = pts.Where(p => p.y <= minY + 0.05f).ToList();
            // neighbours on one ring are a few centimetres apart; two nozzles' rings are half a metre apart
            var group = Enumerable.Range(0, lip.Count).ToArray();
            Func<int, int> root = null;
            root = i => group[i] == i ? i : (group[i] = root(group[i]));
            for (int i = 0; i < lip.Count; i++)
                for (int j = i + 1; j < lip.Count; j++)
                {
                    var d = lip[i] - lip[j];
                    if (d.x * d.x + d.z * d.z < 0.3f * 0.3f) group[root(i)] = root(j);
                }
            var rings = Enumerable.Range(0, lip.Count).GroupBy(root).Select(g =>
            {
                var ring = g.Select(i => lip[i]).ToList();
                float x0 = ring.Min(p => p.x), x1 = ring.Max(p => p.x), z0 = ring.Min(p => p.z), z1 = ring.Max(p => p.z);
                return new Nozzle { exit = new Vector3((x0 + x1) * 0.5f, minY, (z0 + z1) * 0.5f), radius = Mathf.Max(x1 - x0, z1 - z0) * 0.5f };
            }).ToList();
            float biggest = rings.Max(r => r.radius);
            return rings.Where(r => r.radius >= biggest * 0.5f).OrderBy(r => r.exit.x).ThenBy(r => r.exit.z).ToList();   // stray vertices are not nozzles
        }

        // A GSLV model copy placed in space. longAxis: direction (in the parent's frame) the nose should point.
        static GameObject SpaceRocket(string name, Transform parent, Vector3 longAxis, float height, float spinDegrees)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(GslvModelPath);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            Undo.RegisterCreatedObjectUndo(go, UndoName);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            Vector3 modelLong = Quaternion.Inverse(upright) * Vector3.up;
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, longAxis) * upright * Quaternion.AngleAxis(spinDegrees, modelLong);
            go.transform.localScale = Vector3.one * (height / heightPerScale);
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
            return go;
        }

        static void BuildSpaceAdditions()
        {
            earthSystem = FindT("Earth System");
            flightPath = FindT("Flight Path");
            gto = FindT("Transfer Orbit (GTO)");
            if (earthSystem == null || flightPath == null || gto == null) throw new Exception("The space environment is incomplete (Earth System / Flight Path / Transfer Orbit).");
            fp1 = FindUnder(flightPath, "Flight Position 1");
            fp2 = FindUnder(flightPath, "Flight Position 2");
            fp3 = FindUnder(flightPath, "Flight Position 3");
            deployPoint = FindUnder(gto, "Deploy Point");
            slot = FindUnder(deployPoint, "Upper Stage Slot");
            satellite = FindUnder(deployPoint, "Communication Satellite");
            if (fp1 == null || fp2 == null || fp3 == null || deployPoint == null || slot == null || satellite == null)
                throw new Exception("Flight positions or the deploy point are missing from the space environment.");
            ScaleSpaceSet();

            // The map's Earth and clouds move from the old generated "Earth Sphere" mesh to Unity's own sphere (once), turned and
            // doubled so the same Earth shows.
            Quaternion layout;
            var unitySphere = GslvSpace.EditorTools.SpaceEnvironmentBuilder.UnitySphere(out layout);
            foreach (var mf in earthSystem.GetComponentsInChildren<MeshFilter>(true))
                if (mf.sharedMesh != null && mf.sharedMesh.name == "Earth Sphere")
                {
                    Undo.RecordObject(mf, UndoName);
                    Undo.RecordObject(mf.transform, UndoName);
                    mf.sharedMesh = unitySphere;
                    mf.transform.localRotation *= Quaternion.Inverse(layout);
                    mf.transform.localScale *= 2f;
                    var spin = mf.GetComponent<SlowSpin>();
                    if (spin != null)
                    {
                        Undo.RecordObject(spin, UndoName);
                        spin.localAxis = layout * spin.localAxis;
                    }
                    report.AppendLine("Unity's sphere now on " + mf.name);
                }

            // The SLV-3 stand-in is replaced by the real GSLV model.
            var old = FindUnder(fp1, "Small Rocket");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            // The map's objects are switched off between builds (the Timeline shows them); on while building, so sizes can be measured.
            var mapParts = new List<Transform> { earthSystem, flightPath, gto, satellite, FindT("Final Operational Orbit"), FindT("Transfer Arrow") };
            foreach (var fp in new[] { fp1, fp2, fp3 }) mapParts.Add(FindUnder(fp, "Marker"));
            foreach (var m in mapParts)
                if (m != null && !m.gameObject.activeSelf)
                {
                    Undo.RecordObject(m.gameObject, UndoName);
                    m.gameObject.SetActive(true);
                }

            // The map's satellite (sized by the space builder) shrunk to diagram size; safe to run again.
            float mapUnit = earthSystem.lossyScale.x;
            var satB = WorldBounds(satellite);
            float satUnits = Mathf.Max(satB.size.x, Mathf.Max(satB.size.y, satB.size.z)) / mapUnit;
            if (satUnits > 0.0001f && Mathf.Abs(satUnits - MapSatelliteSize) > 0.01f)
            {
                Undo.RecordObject(satellite, UndoName);
                satellite.localScale *= MapSatelliteSize / satUnits;
            }

            // Final showcase copy (Scene 6), assembled, beside Earth at the second flight position.
            var show = NewObject("Showcase Rocket", earthSystem);
            show.transform.localPosition = fp2.localPosition;
            show.transform.localRotation = fp2.localRotation;
            var showModel = SpaceRocket("GSLV Model", show.transform, Vector3.up, MapRocketLength, 35f);
            Reg("ShowcaseModel", showModel);

            // Upper stage (C25 only) right behind the satellite at the deploy point (Scene 5 map).
            var stage = SpaceRocket("Upper Stage (Deploy)", slot, Vector3.forward, 6.2f, 0f);
            foreach (var p in stage.GetComponentsInChildren<Transform>(true))
                if (new[] { "Louncher.001", "Louncher.002", "Louncher.003", "Louncher.004", "Louncher.005", "Louncher.006" }.Contains(p.name)) p.gameObject.SetActive(false);
            Bounds cb = WorldBounds(stage.transform);
            float longest = Mathf.Max(cb.size.x, Mathf.Max(cb.size.y, cb.size.z));
            if (longest > 0.0001f) stage.transform.localScale *= MapStageLength * mapUnit / longest;
            cb = WorldBounds(stage.transform);
            satB = WorldBounds(satellite);
            Vector3 along = deployPoint.forward;   // the satellite is released this way
            Func<Bounds, float> halfAlong = bb => Mathf.Abs(along.x) * bb.extents.x + Mathf.Abs(along.y) * bb.extents.y + Mathf.Abs(along.z) * bb.extents.z;
            stage.transform.position += satB.center - along * (halfAlong(satB) + halfAlong(cb)) - cb.center;
            Reg("DeployStage", stage);

            if (gto.parent.GetComponent<Animator>() == null) Undo.AddComponent<Animator>(gto.parent.gameObject);

            // A thicker copy of the transfer orbit that the Timeline switches on to make the path "glow".
            var gtoLine = gto.GetComponent<LineRenderer>();
            var glow = NewObject("GTO Glow", gto.parent, typeof(LineRenderer));
            glow.transform.localPosition = gto.localPosition;
            glow.transform.localRotation = gto.localRotation;
            glow.transform.localScale = gto.localScale;
            var gl = glow.GetComponent<LineRenderer>();
            gl.useWorldSpace = gtoLine.useWorldSpace;
            gl.loop = gtoLine.loop;
            gl.positionCount = gtoLine.positionCount;
            var pts = new Vector3[gtoLine.positionCount];
            gtoLine.GetPositions(pts);
            gl.SetPositions(pts);
            gl.sharedMaterial = gtoLine.sharedMaterial;
            gl.widthMultiplier = gtoLine.widthMultiplier * 3.2f;
            gl.colorGradient = gtoLine.colorGradient;
            gl.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Reg("GTO Glow", glow);

            // The rocket flies on into space, so the switch to space must not hide it.
            var list = (switcher.launchpadObjects ?? new GameObject[0]).ToList();
            list.Remove(padHost.gameObject);
            Undo.RecordObject(switcher, UndoName);
            switcher.launchpadObjects = list.Where(o => o != null).ToArray();
            switcher.faceRigForward = true;

            // Space camera: Animator host (identity, in the space set's frame) / anchor the learner's eyes follow while in space.
            if (switcher.spaceContent == null) throw new Exception("The space switcher has no Space Content.");
            spaceCamHost = NewObject("Space Camera Host", switcher.spaceContent.transform, typeof(Animator)).transform;
            spaceCam = NewObject("Space Camera Anchor", spaceCamHost).transform;
            var mc = dolly.GetComponent<MissionCamera>();
            if (mc == null) mc = Undo.AddComponent<MissionCamera>(dolly.gameObject);
            Undo.RecordObject(mc, UndoName);
            mc.rig = rig;
            mc.space = switcher;
            mc.spaceAnchor = spaceCam;
        }

        // Enlarges the space set around the learner's viewpoint (so the view from there is unchanged) to give the camera
        // room to fly beside the rocket while everything stays farther away than the UI. Safe to run again.
        static void ScaleSpaceSet()
        {
            float factor = SpaceScale / earthSystem.localScale.x;
            if (Mathf.Abs(factor - 1f) < 0.001f) return;
            Undo.RecordObject(earthSystem, UndoName);
            earthSystem.localPosition *= factor;
            earthSystem.localScale = Vector3.one * SpaceScale;
            foreach (var lr in earthSystem.GetComponentsInChildren<LineRenderer>(true))
            {
                Undo.RecordObject(lr, UndoName);
                lr.widthMultiplier *= factor;   // line widths are in world units and do not follow the transform's scale
            }
            report.AppendLine(string.Format("Space set enlarged x{0:0.#} around the learner's viewpoint (Earth now {1:0} m away)", SpaceScale, earthSystem.localPosition.magnitude));
        }

        // Earth under the rocket while it flies in space. It follows the learner's eyes (SkyBackdrop), so it reads as infinitely far
        // away, and draws behind everything (after the sky, without depth). Sriharikota starts underneath; the ground drifts backwards.
        static void BuildFlightBackdrop()
        {
            Quaternion layout;
            var mesh = GslvSpace.EditorTools.SpaceEnvironmentBuilder.UnitySphere(out layout);   // Unity's sphere, radius 0.5
            var earthMat = LoadOrMakeMaterial("Flight Earth", "Standard", m =>
            {
                m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Assets A/Space/Textures/Earth_Diff.jpg"));
                var nm = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Assets A/Space/Textures/Earth_NM.jpg");
                if (nm != null) { m.SetTexture("_BumpMap", nm); m.EnableKeyword("_NORMALMAP"); }
                m.SetFloat("_Glossiness", 0.15f);
                m.SetFloat("_Mode", 2f);   // Fade: no depth writes, so the rocket always draws over it
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            });
            earthMat.renderQueue = 2502;   // right after the sky (2501), before clouds, flames and UI
            var earthCloudMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Assets A/Space/Materials/Clouds.mat");

            var root = NewObject("Flight Backdrop", spaceFolder, typeof(Animator), typeof(SkyBackdrop));
            backdrop = root.transform;
            var sky = root.GetComponent<SkyBackdrop>();
            sky.radius = BackdropRadius;
            sky.height = 30f;
            root.transform.position = rig.position + Vector3.down * (BackdropRadius + sky.height);

            Vector3 fwd = Vector3.ProjectOnPlane(rig.forward, Vector3.up).normalized;
            Vector3 downrange = Vector3.Cross(Vector3.up, fwd).normalized;     // the learner's right
            Quaternion turn = BackdropTurn(downrange);
            Vector3 spinAxis = Vector3.Cross(downrange, Vector3.up);          // turns the ground under the rocket backwards
            Func<string, float, Material, float, Transform> sphere = (name, r, mat, degPerSec) =>
            {
                var go = NewObject(name, root.transform, typeof(MeshFilter), typeof(MeshRenderer), typeof(SlowSpin));
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
                go.transform.localRotation = turn * Quaternion.Inverse(layout);
                go.transform.localScale = Vector3.one * (r * 2f);
                var spin = go.GetComponent<SlowSpin>();
                spin.localAxis = (layout * (Quaternion.Inverse(turn) * spinAxis)).normalized;
                spin.speed = degPerSec;
                return go.transform;
            };
            sphere("Earth", BackdropRadius, earthMat, 0.6f);
            if (earthCloudMat != null) sphere("Clouds", BackdropRadius * 1.012f, earthCloudMat, 0.7f);

            // The sky for the climb: from the pad's blue to black, with the limb glow on Earth's horizon (SkyAtmosphere).
            var dome = NewObject("Sky Atmosphere", spaceFolder, typeof(MeshFilter), typeof(MeshRenderer), typeof(Animator), typeof(SkyAtmosphere));
            dome.GetComponent<MeshFilter>().sharedMesh = mesh;
            var dr = dome.GetComponent<MeshRenderer>();
            dr.sharedMaterial = skyMat;
            dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dr.receiveShadows = false;
            dome.transform.position = rig.position;
            dome.transform.localScale = Vector3.one * 1900f;  // radius 950 m, inside the eyes' 1000 m far clip
            var atm = dome.GetComponent<SkyAtmosphere>();
            atm.earth = sky;
            atm.padSun = switcher.launchpadSun;
            atm.spaceSun = switcher.spaceSun;
            atm.opacity = 0f;
            Reg("SkyDome", dome);
        }

        // Turns the Earth sphere so Sriharikota is straight up (under the learner) and east points downrange.
        static Quaternion BackdropTurn(Vector3 downrange)
        {
            const float lat = 13.72f * Mathf.Deg2Rad, lon = 80.23f * Mathf.Deg2Rad;   // Satish Dhawan Space Centre
            var site = new Vector3(Mathf.Cos(lat) * Mathf.Sin(lon), Mathf.Sin(lat), -Mathf.Cos(lat) * Mathf.Cos(lon));   // as the space builder maps it
            var east = new Vector3(Mathf.Cos(lon), 0f, Mathf.Sin(lon));
            return Quaternion.LookRotation(downrange, Vector3.up) * Quaternion.Inverse(Quaternion.LookRotation(east, site));
        }

        // ---------------------------------------------------------------- audio, director
        static void BuildAudio()
        {
            var audio = FindRoot("AUDIO");
            voSource = NewObject("VO Source", audio, typeof(AudioSource));
            feedbackSource = NewObject("Feedback Source", audio, typeof(AudioSource));
            var click = NewObject("UI Click Source", audio, typeof(AudioSource));
            foreach (var go in new[] { voSource, feedbackSource, click })
            {
                var a = go.GetComponent<AudioSource>();
                a.playOnAwake = false;
                a.loop = false;
                a.spatialBlend = 0f;
            }
            // every button's click sound (the buttons are listed in WireFlow, once the UI exists)
            clickSound = Undo.AddComponent<UiClickSound>(click);
            clickSound.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(SfxFolder + "/" + ClickSound);
            clickSound.volume = ClickVolume;
            if (clickSound.clip == null) warnings.Add("click sound missing: " + SfxFolder + "/" + ClickSound);
        }

        static UiClickSound clickSound;

        static void BuildDirector()
        {
            var host = NewObject("Mission Director", FindRoot("TIMELINE"), typeof(PlayableDirector));
            dir = host.GetComponent<PlayableDirector>();
            dir.playOnAwake = true;
            dir.extrapolationMode = DirectorWrapMode.Hold;
            dir.timeUpdateMode = DirectorUpdateMode.GameTime;
            flow = Undo.AddComponent<MissionFlow>(host);
            flow.director = dir;
            // headset render settings (MSAA, terrain distances, eye near plane), applied before QWR makes its eye buffers
            NewObject("Headset Render Settings", FindRoot("TIMELINE"), typeof(HeadsetRenderSettings));
        }

        // ---------------------------------------------------------------- UI
        static void BuildUi()
        {
            var ui = FindRoot("UI");
            uiAnchor = NewObject("UI Anchor", ui).transform;
            uiAnchor.SetPositionAndRotation(rig.position, Quaternion.Euler(0f, rig.eulerAngles.y, 0f));
            var follow = Undo.AddComponent<FollowDolly>(uiAnchor.gameObject);
            follow.dolly = dolly;

            // ----- captions (the storyboard's ON-SCREEN lines)
            Caption("Cap GSLV", "GSLV — GEOSYNCHRONOUS SATELLITE LAUNCH VEHICLE", 0f, -2.4f, 54f);
            Caption("Cap Objective", "MISSION OBJECTIVE — EXPLORE GSLV AND LAUNCH A SATELLITE", 0f, -2.4f, 50f);
            Caption("Cap Explore", "EXPLORE THE GSLV", 0f, -1.35f, 56f, 760f, null, "Icon_Book");   // low, below the rocket, a clear gap above the Explore button
            Caption("Cap S200", "S200 — SOLID ROCKET BOOSTERS", 0f, -2.4f, 54f);
            Caption("Cap L110", "L110 — LIQUID STAGE", 0f, -2.4f, 54f);
            Caption("Cap C25", "C25 — CRYOGENIC UPPER STAGE", 0f, -2.4f, 54f);
            Caption("Cap Hydrogen", "LIQUID HYDROGEN — FUEL", SideBearing + 1f, 0.95f, 40f, 820f, Cyan, "Icon_Drop");
            Caption("Cap Oxygen", "LIQUID OXYGEN — OXIDIZER", SideBearing + 1f, 0.05f, 40f, 820f, Amber, "Icon_Drop");
            Caption("Cap All", "S200  |  L110  |  C25", 0f, -2.4f, 56f);
            Caption("Cap Discovered", "ROCKET SYSTEMS DISCOVERED!", 0f, -2.4f, 64f, 1300f, Green);
            CountCaption("Cap Count5", "5", 0f, 0.7f);   // the countdown, centred in front of the learner
            CountCaption("Cap Count4", "4", 0f, 0.7f);
            CountCaption("Cap Count3", "3", 0f, 0.7f);
            CountCaption("Cap Count2", "2", 0f, 0.7f);
            CountCaption("Cap Count1", "1", 0f, 0.7f);
            Caption("Cap Liftoff", "LIFTOFF!", 0f, 0.7f, 120f, 900f, Green);
            Caption("Cap LiftoffOk", "GSLV LIFTOFF SUCCESSFUL!", 0f, -2.4f, 64f, 1400f, Green);
            Caption("Cap Separation", "STAGE SEPARATION", 0f, -2.4f, 56f);
            Caption("Cap Fairing", "PAYLOAD FAIRING RELEASED", 0f, -2.4f, 56f);
            Caption("Cap Cryo", "CRYOGENIC UPPER STAGE", 0f, -2.4f, 56f);
            Caption("Cap GTO", "GEOSYNCHRONOUS TRANSFER ORBIT — GTO", 0f, -2.4f, 54f, 1500f, null, "Icon_Orbit");
            Caption("Cap DeployReady", "SATELLITE DEPLOYMENT — READY", SideBearing, 1.55f, 50f, 1100f, Green);   // one line, a clear gap above the Deploy button
            Caption("Cap Deployed", "SATELLITE DEPLOYED INTO GTO!", 0f, -2.4f, 56f, 1400f, Green);
            Caption("Cap Success", "GSLV MISSION SUCCESSFUL!", 0f, -2.4f, 60f, 1400f, Green);
            Caption("Cap Complete", "MISSION GSLV — COMPLETE!\n<size=55%><color=#9fe8ff>POWERFUL PROPULSION. ONE MISSION. A JOURNEY TO SPACE.</color></size>", 0f, -2.7f, 76f, 1500f, Green);

            // ----- gate buttons
            ButtonCanvas("Explore", "EXPLORE", 0f, -2.4f, new Vector2(620f, 170f), "Blue", "Icon_Book");   // on the bottom caption line
            ButtonCanvas("Deploy", "DEPLOY SATELLITE", SideBearing, 0.3f, new Vector2(880f, 170f), "Orange", "Icon_Satellite");

            // ----- mission readiness panel (Scene 3), with its LAUNCH button
            BuildReadinessPanel();

            // ----- quiz and achievements (Scene 6), then the end menu
            BuildQuizPanel();
            BuildAchievements();
            BuildEndMenu();

            // ----- the atmosphere's layers and a live altitude / speed readout, on the left during the climb
            BuildAtmosphereReadouts();
        }

        static void BuildAtmosphereReadouts()
        {
            float bearing = -(SideBearing + 4f);
            string[,] layers =
            {
                { "TROPOSPHERE", "0 - 12 km  |  clouds and weather" },
                { "STRATOSPHERE", "12 - 50 km  |  the ozone layer" },
                { "MESOSPHERE", "50 - 85 km  |  meteors burn up here" },
                { "THERMOSPHERE", "85 - 600 km  |  almost no air; space begins at 100 km" },
            };
            for (int i = 0; i < layers.GetLength(0); i++)
            {
                string key = "Cap Layer " + (i + 1);
                var c = CanvasObject(key, bearing, UiDistance, 1.35f, new Vector2(900f, 190f));
                Img("Background", c.transform, "Panel", new Vector2(900f, 190f), Vector2.zero);
                Img("Icon", c.transform, "Icon_Layers", new Vector2(64f, 64f), new Vector2(-370f, 38f), Cyan);
                Text(c.transform, layers[i, 0], 54f, Cyan, new Vector2(860f, 80f), new Vector2(0f, 38f)).characterSpacing = 4f;
                Img("Line", c.transform, "Divider", new Vector2(800f, 12f), new Vector2(0f, -4f));
                Text(c.transform, layers[i, 1], 30f, White, new Vector2(860f, 60f), new Vector2(0f, -42f), TextAlignmentOptions.Center, FontStyles.Normal);
                Reg(key, c);
            }
            var tele = CanvasObject("Telemetry", bearing, UiDistance, -0.05f, new Vector2(620f, 170f));   // a clear gap below the layer panel
            Img("Background", tele.transform, "Panel", new Vector2(620f, 170f), Vector2.zero);
            Img("Icon", tele.transform, "Icon_Gauge", new Vector2(86f, 86f), new Vector2(-235f, 0f), Cyan);
            var readout = Text(tele.transform, "", 40f, White, new Vector2(440f, 150f), new Vector2(50f, 0f), TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
            var ft = Undo.AddComponent<FlightTelemetry>(tele);
            ft.director = dir;
            ft.text = readout;
            Reg("Telemetry", tele);
        }

        static GameObject CanvasObject(string name, float bearing, float dist, float y, Vector2 size)
        {
            var go = NewObject(name, uiAnchor, typeof(RectTransform), typeof(Canvas));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one * Px;
            float rad = bearing * Mathf.Deg2Rad;
            rt.localPosition = new Vector3(Mathf.Sin(rad) * dist, y, Mathf.Cos(rad) * dist);
            rt.localRotation = Quaternion.Euler(0f, bearing, 0f);
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            return go;
        }

        // ---- UI kit (Assets A/UI/GSLV UI Kit): the panels, buttons, rows, popups and icons of the Mission GSLV UI sheets
        static void PrepareUiKit()
        {
            foreach (string file in Directory.GetFiles(KitFolder, "*.png"))
            {
                string path = KitFolder + "/" + Path.GetFileName(file);
                if (AssetImporter.GetAtPath(path) == null) AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;
                Vector4 border = SliceBorder(Path.GetFileNameWithoutExtension(path));
                if (imp.textureType == TextureImporterType.Sprite && imp.spriteBorder == border && imp.mipmapEnabled && imp.alphaIsTransparency
                    && imp.textureCompression == TextureImporterCompression.Uncompressed) continue;
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.spriteBorder = border;
                imp.mipmapEnabled = true;            // world-space UI seen from metres away
                imp.alphaIsTransparency = true;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.filterMode = FilterMode.Trilinear;
                imp.textureCompression = TextureImporterCompression.Uncompressed;   // glows and gradients band when compressed
                imp.SaveAndReimport();
            }
        }

        // 9-slice borders (left, bottom, right, top) of the kit's stretchable sprites.
        static Vector4 SliceBorder(string sprite)
        {
            if (HasGlowMargin(sprite)) return new Vector4(48f, 48f, 48f, 48f);
            if (sprite.StartsWith("Bar_")) return new Vector4(16f, 14f, 16f, 14f);
            return Vector4.zero;
        }

        static bool HasGlowMargin(string sprite)
        {
            return sprite.StartsWith("Panel") || sprite.StartsWith("Button_") || sprite.StartsWith("Option") || sprite.StartsWith("Popup_") || sprite.StartsWith("Row_");
        }

        static Sprite Kit(string sprite)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(KitFolder + "/" + sprite + ".png");
            if (s == null && !warnings.Contains("UI kit sprite missing: " + sprite)) warnings.Add("UI kit sprite missing: " + sprite);
            return s;
        }

        // A UI kit image; 'size' is the visible size (a 9-slice skin's rect also takes in its glow margin).
        static Image Img(string name, Transform parent, string sprite, Vector2 size, Vector2 pos, Color? color = null)
        {
            var img = NewObject(name, parent, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            Skin(img, sprite, size, pos);
            img.color = color ?? Color.white;
            img.raycastTarget = false;
            return img;
        }

        static void Skin(Image img, string sprite, Vector2 size, Vector2 pos)
        {
            img.sprite = Kit(sprite);
            img.type = SliceBorder(sprite) != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            img.rectTransform.sizeDelta = HasGlowMargin(sprite) ? size + Vector2.one * (2f * SkinGlow) : size;
            img.rectTransform.anchoredPosition = pos;
        }

        // Panel header from the sheets: icon, spaced title, fading underline.
        static TextMeshProUGUI Header(Transform parent, string icon, string title, float width, float y, float fontSize)
        {
            float left = -width * 0.5f + 45f, iconSize = fontSize * 1.2f;
            Img("Header Icon", parent, icon, new Vector2(iconSize, iconSize), new Vector2(left + iconSize * 0.5f, y), Cyan);
            float textLeft = left + iconSize + 22f, textWidth = width * 0.5f - 45f - textLeft;
            var t = Text(parent, title, fontSize, Cyan, new Vector2(textWidth, fontSize * 1.6f), new Vector2(textLeft + textWidth * 0.5f, y), TextAlignmentOptions.MidlineLeft);
            t.characterSpacing = 6f;
            OneLine(t);
            Img("Header Line", parent, "Divider", new Vector2(width - 90f, 12f), new Vector2(0f, y - fontSize * 0.95f));
            return t;
        }

        static TextMeshProUGUI Text(Transform parent, string text, float size, Color color, Vector2 rect, Vector2 pos, TextAlignmentOptions align = TextAlignmentOptions.Center, FontStyles style = FontStyles.Bold)
        {
            var go = NewObject("Text", parent, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = rect;
            rt.anchoredPosition = pos;
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = TMP_Settings.defaultFontAsset;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.enableAutoSizing = false;
            t.enableWordWrapping = true;
            t.raycastTarget = false;
            return t;
        }

        // Glass panel caption; green captions are the sheets' success popup with its check badge. 'icon' (tinted like the text) sits left.
        static GameObject Caption(string key, string text, float bearing, float y, float fontSize, float maxWidth = 1500f, Color? color = null, string icon = null)
        {
            bool success = color == Green;
            if (success) icon = "Step_Done";
            string skin = success ? "Popup_Success" : "Panel";
            float iconSize = icon == null ? 0f : fontSize * 1.1f, iconRoom = icon == null ? 0f : iconSize + 24f;
            var go = CanvasObject(key, bearing, UiDistance, y, Vector2.one);
            var bg = Img("Background", go.transform, skin, Vector2.one, Vector2.zero);
            Image mark = null;
            if (success) mark = Img("Badge", go.transform, icon, Vector2.one * (iconSize * 2f), Vector2.zero);   // the badge sprite's disc is half its size
            else if (icon != null) mark = Img("Icon", go.transform, icon, Vector2.one * iconSize, Vector2.zero, color ?? Cyan);
            var t = Text(go.transform, text, fontSize, color ?? White, Vector2.one, Vector2.zero);
            // the panel fits the text as TMP lays it out, wrapping only past maxWidth
            float textW = Mathf.Min(t.GetPreferredValues(text).x + 20f, maxWidth - 90f);
            float textH = t.GetPreferredValues(text, textW, 0f).y;
            var size = new Vector2(textW + 90f + iconRoom, textH + 60f);
            ((RectTransform)go.transform).sizeDelta = size;
            Skin(bg, skin, size, Vector2.zero);
            if (mark != null) mark.rectTransform.anchoredPosition = new Vector2(-size.x * 0.5f + 45f + iconSize * 0.5f, 0f);
            t.rectTransform.sizeDelta = new Vector2(textW, textH + 10f);
            t.rectTransform.anchoredPosition = new Vector2(iconRoom * 0.5f, 0f);
            return Reg(key, go);
        }

        // Countdown number inside the sheets' glowing ring.
        static GameObject CountCaption(string key, string digit, float bearing, float y)
        {
            var go = CanvasObject(key, bearing, UiDistance, y, new Vector2(420f, 420f));
            Img("Shade", go.transform, "Disc_Soft", new Vector2(440f, 440f), Vector2.zero, new Color(0.02f, 0.07f, 0.16f, 0.95f));
            Img("Glow", go.transform, "Disc_Soft", new Vector2(380f, 380f), Vector2.zero, new Color(Amber.r, Amber.g, Amber.b, 0.25f));
            Img("Ring", go.transform, "Ring_Glow", new Vector2(400f, 400f), Vector2.zero, Amber);
            Text(go.transform, digit, 220f, Amber, new Vector2(300f, 300f), new Vector2(0f, 6f));
            return Reg(key, go);
        }

        // Button with the kit's skins: hover lights it up, disabled (gate not open yet) greys it out.
        static Button MakeButton(string name, Transform parent, Vector2 pos, Vector2 size, string skin, string hover, string disabled)
        {
            var go = NewObject(name, parent, typeof(RectTransform), typeof(Image), typeof(Button), typeof(BoxCollider));
            var img = go.GetComponent<Image>();
            Skin(img, skin, size, pos);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.SpriteSwap;
            var states = btn.spriteState;
            states.highlightedSprite = Kit(hover);
            states.pressedSprite = Kit(hover);
            states.disabledSprite = disabled != null ? Kit(disabled) : null;
            btn.spriteState = states;
            var box = go.GetComponent<BoxCollider>();
            box.size = new Vector3(size.x, size.y, 30f);
            return btn;
        }

        // The sheets' main action button: icon tile, title, chevron. color: Blue, Green or Orange.
        static Button ActionButton(string name, Transform parent, Vector2 pos, Vector2 size, string label, float fontSize, string color, string icon)
        {
            var btn = MakeButton(name, parent, pos, size, "Button_" + color, "Button_" + color + "_Hover", "Button_Disabled");
            float tile = size.y - 46f, left = -size.x * 0.5f + 24f + tile * 0.5f;
            Img("Icon Tile", btn.transform, "IconTile_" + color, Vector2.one * (tile * 128f / 108f), new Vector2(left, 0f));   // tile sprite: 108 px body in 128
            Img("Icon", btn.transform, icon, Vector2.one * (tile * 0.62f), new Vector2(left, 0f));
            float chevron = fontSize * 0.75f, right = size.x * 0.5f - 28f - chevron * 0.5f;
            Img("Chevron", btn.transform, "Icon_Chevron", Vector2.one * chevron, new Vector2(right, 0f), new Color(1f, 1f, 1f, 0.9f));
            float textLeft = left + tile * 0.5f + 30f, textRight = right - chevron * 0.5f - 16f;
            var t = Text(btn.transform, label, fontSize, White, new Vector2(textRight - textLeft, size.y - 20f), new Vector2((textLeft + textRight) * 0.5f, 0f), TextAlignmentOptions.MidlineLeft);
            t.characterSpacing = 4f;
            OneLine(t);
            return btn;
        }

        // Keeps a label on one line, shrinking it (down to 60%) when it would not fit.
        static void OneLine(TMP_Text t)
        {
            t.enableWordWrapping = false;
            t.enableAutoSizing = true;
            t.fontSizeMax = t.fontSize;
            t.fontSizeMin = t.fontSize * 0.6f;
        }

        static void ButtonCanvas(string id, string label, float bearing, float y, Vector2 size, string color, string icon)
        {
            size *= ButtonScale;
            var canvas = CanvasObject(id + " Button", bearing, UiDistance - 0.4f, y, size + new Vector2(40f, 40f));
            var btn = ActionButton(id, canvas.transform, Vector2.zero, size, label, 62f * ButtonScale, color, icon);
            Reg(id + " Button", canvas);
            Reg("Btn " + id, btn.gameObject);
        }

        static void BuildReadinessPanel()
        {
            var panel = CanvasObject("Readiness Panel", 0f, UiDistance - 0.4f, 0.7f, new Vector2(720f, 740f));   // centred in front of the learner
            Img("Background", panel.transform, "Panel", new Vector2(720f, 740f), Vector2.zero);
            Header(panel.transform, "Icon_Checklist", "MISSION READINESS", 720f, 300f, 46f);
            string[] rows = { "SATELLITE SECURED", "PROPULSION SYSTEMS", "FLIGHT SYSTEMS" };
            for (int i = 0; i < rows.Length; i++)
            {
                float y = 170f - i * 120f;
                Img("Row " + (i + 1) + " Bar", panel.transform, "Option", new Vector2(650f, 100f), new Vector2(0f, y));
                Img("Row " + (i + 1) + " Box", panel.transform, "Checkbox_Off", new Vector2(104f, 104f), new Vector2(-270f, y));
                // ticked check box, green row and READY: switched on by the Timeline; the label stays on top
                var ready = NewObject("Row " + (i + 1) + " Ready", panel.transform, typeof(RectTransform));
                Img("Ready Bar", ready.transform, "Row_Ready", new Vector2(650f, 100f), new Vector2(0f, y));
                Img("Ready Box", ready.transform, "Checkbox_On", new Vector2(104f, 104f), new Vector2(-270f, y));
                Text(ready.transform, "READY", 32f, Green, new Vector2(150f, 90f), new Vector2(225f, y), TextAlignmentOptions.MidlineRight).characterSpacing = 4f;
                SetInactive(ready);
                Reg("Ready " + (i + 1), ready);
                OneLine(Text(panel.transform, rows[i], 36f, White, new Vector2(390f, 90f), new Vector2(-30f, y), TextAlignmentOptions.MidlineLeft));
            }
            var launch = ActionButton("Launch", panel.transform, new Vector2(0f, -285f), new Vector2(620f, 150f) * ButtonScale, "LAUNCH", 70f * ButtonScale, "Green", "Icon_Rocket");
            Reg("Readiness Panel", panel);
            Reg("Btn Launch", launch.gameObject);
        }

        static void BuildQuizPanel()
        {
            var panel = CanvasObject("Quiz Panel", 0f, UiDistance - 0.8f, 0.3f, new Vector2(1240f, 860f));
            Img("Background", panel.transform, "Panel", new Vector2(1240f, 860f), Vector2.zero);
            var counter = Header(panel.transform, "Icon_Quiz", "QUESTION 1 OF 5", 1240f, 372f, 36f);
            var question = Text(panel.transform, "", 54f, White, new Vector2(1120f, 200f), new Vector2(0f, 245f));
            Text(panel.transform, "CLICK THE CORRECT ANSWER.", 32f, new Color(0.6f, 0.85f, 1f, 1f), new Vector2(900f, 50f), new Vector2(0f, -345f), TextAlignmentOptions.Center, FontStyles.Normal);

            quiz = Undo.AddComponent<QuizPanel>(panel);
            quiz.counterText = counter;
            quiz.questionText = question;
            quiz.optionButtons = new Button[3];
            quiz.optionTexts = new TMP_Text[3];
            quiz.optionImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var b = MakeButton("Option " + (char)('A' + i), panel.transform, new Vector2(0f, 70f - i * 150f), new Vector2(1100f, 126f), "Option", "Option_Hover", null);
                quiz.optionButtons[i] = b;
                quiz.optionImages[i] = b.GetComponent<Image>();
                quiz.optionTexts[i] = Text(b.transform, "", 46f, White, new Vector2(1030f, 110f), Vector2.zero, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
            }
            quiz.normalSprite = Kit("Option");
            quiz.correctSprite = Kit("Option_Correct");
            quiz.wrongSprite = Kit("Option_Wrong");
            quiz.questions = new[]
            {
                Q("WHAT DOES GSLV STAND FOR?", 0, "Geosynchronous Satellite Launch Vehicle", "General Space Launch Vehicle", "Global Satellite Launch Vehicle"),
                Q("WHAT IS THE MAIN PURPOSE OF GSLV?", 1, "Carry passengers across Earth", "Launch satellites into space", "Explore the ocean floor"),
                Q("WHY DO COMPLETED ROCKET STAGES SEPARATE?", 0, "To reduce the rocket's mass", "To change the rocket's colour", "To make the rocket stop moving"),
                Q("WHICH FUEL IS USED IN GSLV'S CRYOGENIC UPPER STAGE?", 1, "Petrol", "Liquid hydrogen", "Coal"),
                Q("WHAT HAPPENS WHEN THE SATELLITE IS DEPLOYED?", 0, "It separates from the rocket", "It returns to the launchpad", "It lands on Earth")
            };
            Reg("Quiz Panel", panel);
        }

        static QuizQuestion Q(string text, int correct, params string[] options)
        {
            return new QuizQuestion { question = text, correctIndex = correct, options = options };
        }

        static void BuildAchievements()
        {
            var panel = CanvasObject("Achievements", 0f, UiDistance, 2.5f, new Vector2(1500f, 330f));
            string[] names = { "GSLV DISCOVERED", "ROCKET SYSTEMS EXPLORED", "SATELLITE DEPLOYED" };
            string[] icons = { "Icon_Book", "Icon_Rocket", "Icon_Satellite" };
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 480f;
                Img("Badge " + (i + 1) + " Shade", panel.transform, "Disc_Soft", new Vector2(250f, 250f), new Vector2(x, 50f), new Color(0.02f, 0.07f, 0.16f, 0.9f));
                Img("Badge " + (i + 1), panel.transform, "Ring_Glow", new Vector2(226f, 226f), new Vector2(x, 50f), Grey);
                Img("Badge " + (i + 1) + " Icon", panel.transform, icons[i], new Vector2(80f, 80f), new Vector2(x, 50f), new Color(Grey.r, Grey.g, Grey.b, 0.7f));
                Text(panel.transform, names[i], 34f, new Color(0.75f, 0.85f, 0.95f, 1f), new Vector2(440f, 100f), new Vector2(x, -95f));
                var lit = NewObject("Badge " + (i + 1) + " Lit", panel.transform, typeof(RectTransform));
                Img("Glow", lit.transform, "Disc_Soft", new Vector2(280f, 280f), new Vector2(x, 50f), new Color(Gold.r, Gold.g, Gold.b, 0.4f));
                Img("Ring", lit.transform, "Ring_Glow", new Vector2(226f, 226f), new Vector2(x, 50f), Gold);
                Img("Icon", lit.transform, icons[i], new Vector2(80f, 80f), new Vector2(x, 50f));
                Img("Check", lit.transform, "Step_Done", new Vector2(110f, 110f), new Vector2(x + 62f, 112f));
                SetInactive(lit);
                Reg("Badge Lit " + (i + 1), lit);
            }
            Reg("Achievements", panel);
        }

        // Shown once the mission is complete: RESTART and EXIT, centred in front of the learner.
        static void BuildEndMenu()
        {
            var size = new Vector2(1120f, 480f);
            var panel = CanvasObject("End Menu", 0f, UiDistance - 0.4f, 0.3f, size);
            Img("Background", panel.transform, "Panel", size, Vector2.zero);
            Header(panel.transform, "Icon_Rocket", "MISSION COMPLETE", size.x, 165f, 46f);
            var buttonSize = new Vector2(520f, 150f) * ButtonScale;
            float x = buttonSize.x * 0.5f + 30f;
            var menu = Undo.AddComponent<MissionEndMenu>(panel);
            menu.restartButton = ActionButton("Restart", panel.transform, new Vector2(-x, -45f), buttonSize, "RESTART", 62f * ButtonScale, "Blue", "Icon_Rocket");
            menu.exitButton = ActionButton("Exit", panel.transform, new Vector2(x, -45f), buttonSize, "EXIT", 62f * ButtonScale, "Orange", "Icon_Cross");
            Reg("End Menu", panel);
        }

        // ---------------------------------------------------------------- highlights
        static GameObject Highlight(string key, Transform root, Color color, params string[] partNames)
        {
            var go = NewObject(key, FindUnder(effectsFolder, "Highlights") ?? NewObject("Highlights", effectsFolder).transform, typeof(PartHighlight));
            var h = go.GetComponent<PartHighlight>();
            var rs = new List<Renderer>();
            foreach (string n in partNames)
            {
                var part = FindPart(root, n);
                if (part == null) { warnings.Add("highlight '" + key + "': part '" + n + "' not found"); continue; }
                rs.AddRange(part.GetComponentsInChildren<Renderer>(true));
            }
            h.renderers = rs.Distinct().ToArray();
            h.sourceMaterials = sourceMats;
            h.glowMaterials = glowMats;
            h.color = color;
            return Reg(key, go);
        }

        static void BuildHighlights()
        {
            var cy = new Color(0.15f, 0.85f, 1f);
            var all = new[] { "Louncher.001", "Louncher.002", "Louncher.003", "Louncher.004", "Louncher.005", "Louncher.006", "Louncher.009", "J_2S_J_2_0" };
            var s200 = new[] { "Louncher.004", "Louncher.005", "Louncher.006" };
            var l110 = new[] { "Louncher.001" };
            var c25 = new[] { "Louncher.009", "J_2S_J_2_0" };
            var fairing = new[] { "Louncher.002", "Louncher.003" };
            var s200l110c25 = s200.Concat(l110).Concat(c25).ToArray();

            Highlight("HL Pad Whole", modelRoot, cy, all);
            Highlight("HL Pad S200", modelRoot, cy, s200);
            Highlight("HL Pad L110", modelRoot, cy, l110);
            Highlight("HL Pad C25", modelRoot, cy, c25);
            Highlight("HL Pad Three Stages", modelRoot, cy, s200l110c25);
            Highlight("HL Pad Fairing", modelRoot, cy, fairing);
            Highlight("HL Showcase Whole", G("ShowcaseModel").transform, cy, all);
            Highlight("HL Deploy Stage", G("DeployStage").transform, cy, c25);

            // the satellite (Probe) uses its own material
            var sat = NewObject("HL Deploy Satellite", FindUnder(effectsFolder, "Highlights"), typeof(PartHighlight));
            var h = sat.GetComponent<PartHighlight>();
            h.renderers = satellite.GetComponentsInChildren<Renderer>(true);
            h.sourceMaterials = h.renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
            h.glowMaterials = h.sourceMaterials.Select(GlowCopy).ToArray();
            h.color = new Color(0.4f, 1f, 0.7f);
            Reg("HL Deploy Satellite", sat);

            // the user's satellite inside the rocket, also with its own materials
            if (flightSat != null)
            {
                var fs = NewObject("HL Flight Satellite", FindUnder(effectsFolder, "Highlights"), typeof(PartHighlight));
                var fh = fs.GetComponent<PartHighlight>();
                fh.renderers = flightSat.GetComponentsInChildren<Renderer>(true).Where(r => ShownWithin(r.transform, flightSat)).ToArray();   // the merged mesh, not the parts
                fh.sourceMaterials = fh.renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
                fh.glowMaterials = fh.sourceMaterials.Select(GlowCopy).ToArray();
                fh.color = new Color(0.4f, 1f, 0.7f);
                Reg("HL Flight Satellite", fs);
            }
        }

        // The fairing's see-through copies of its materials, and the component that fades it by Timeline time
        // (the satellite inside shows between 'start' and 'end'). The rocket's own materials are never changed.
        static void FairingReveal(double start, double end)
        {
            var rs = new[] { "Louncher.002", "Louncher.003" }.Select(n => FindPart(modelRoot, n)).Where(p => p != null)
                .SelectMany(p => p.GetComponentsInChildren<Renderer>(true)).Distinct().ToArray();
            if (rs.Length == 0) { warnings.Add("fairing not found: it stays opaque while the satellite is introduced"); return; }
            var ff = NewObject("Fairing Fade", effectsFolder, typeof(FairingFade)).GetComponent<FairingFade>();
            ff.director = dir;
            ff.renderers = rs;
            ff.sourceMaterials = rs.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
            ff.fadeMaterials = ff.sourceMaterials.Select(FadeCopy).ToArray();
            ff.start = start;
            ff.end = end;
            ff.fadeTime = FairingFadeTime;
            ff.minOpacity = FairingMinOpacity;
            foreach (var m in ff.sourceMaterials.Where(m => m.shader == null || m.shader.name != "Standard"))
                warnings.Add("fairing material '" + m.name + "' does not use the Standard shader, so it may not fade");
        }

        // A copy of a Standard material in its Fade mode (what the material inspector sets), kept as an asset so Android builds keep the variant.
        static Material FadeCopy(Material source)
        {
            string path = MatFolder + "/" + source.name + " (Fade).mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(source) { name = source.name + " (Fade)" };
                AssetDatabase.CreateAsset(m, path);
            }
            else
            {
                m.shader = source.shader;
                m.CopyPropertiesFromMaterial(source);
            }
            m.SetFloat("_Mode", 2f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(m);
            return m;
        }

        // ---------------------------------------------------------------- effects (flames, smoke)
        static void BuildEffects()
        {
            Vector3 fwdH = Vector3.ProjectOnPlane(rig.forward, Vector3.up).normalized;
            Vector3 downrange = Vector3.Cross(Vector3.up, fwdH).normalized;   // the learner's right: the flight heads this way
            var pb = WorldBounds(modelRoot);
            Vector3 padBase = new Vector3(pb.center.x, pb.min.y, pb.center.z);

            // One low-cost stretched-particle plume per engine, coloured like the real exhausts: the S200 solids
            // bright yellow-white, the L110's Vikas engines orange, and the C25 hydrogen-oxygen plume pale blue.
            // The texture supplies turbulent edges and shock cells, so realism does not require stacked renderers.
            var flames = NewObject("Engine Flames", padLift);
            var nozzles = new[] { new[] { "Flame S200 R", "Louncher.005" }, new[] { "Flame S200 L", "Louncher.006" }, new[] { "Flame L110", "Louncher.001" }, new[] { "Flame C25", "J_2S_J_2_0" } };
            var nozzleAt = new Dictionary<string, Vector3>();
            foreach (var nz in nozzles)
            {
                var part = FindPart(modelRoot, nz[1]);
                if (part == null || part.GetComponentsInChildren<Renderer>().Length == 0) { warnings.Add("flame anchor '" + nz[1] + "' not found"); continue; }
                var exits = NozzleExits(part);   // the part's own mesh, or the meshes under it (the C25 engine is a group)
                var at = exits[0].exit;
                nozzleAt[nz[0]] = at;
                // the L110 and C25 burn in space, where the rocket is fast: their flames move with the nozzle (local space),
                // so they stay a flame on the engine instead of a long streak left behind
                if (nz[0] == "Flame L110")
                {
                    // two Vikas engines side by side: one flame from each, switched on and off together through the parent
                    var pair = NewObject(nz[0], flames.transform);
                    pair.transform.position = exits.Aggregate(Vector3.zero, (s, x) => s + x.exit) / exits.Count;
                    for (int i = 0; i < exits.Count; i++)
                        MakeFlame(nz[0] + " " + (char)('A' + i), pair.transform, exits[i].exit, 2.0f, new Color(1f, 0.94f, 0.76f), new Color(1f, 0.30f, 0.035f), 0.82f, true);
                    Reg(nz[0], pair);
                }
                else if (nz[0] == "Flame C25") Reg(nz[0], MakeFlame(nz[0], flames.transform, at, 1.7f, new Color(0.88f, 0.96f, 1f), new Color(0.28f, 0.46f, 1f), 0.72f, true));
                else Reg(nz[0], MakeFlame(nz[0], flames.transform, at, 2.0f, new Color(1f, 1f, 0.88f), new Color(1f, 0.48f, 0.055f), 1f));
            }

            // Thick white smoke trails from the solid boosters while in the lower atmosphere (world space, so they stay behind).
            foreach (string side in new[] { "R", "L" })
            {
                Vector3 at;
                if (!nozzleAt.TryGetValue("Flame S200 " + side, out at)) continue;
                var trail = NewParticles("Trail S200 " + side, flames.transform, puffSmokeMat, 260);
                trail.transform.position = at + Vector3.down * 2.5f;
                trail.transform.rotation = Quaternion.Euler(90f, 0f, 0f);   // cone pointing down
                var m = trail.main;
                m.loop = true;
                m.startLifetime = new ParticleSystem.MinMaxCurve(9f, 13f);
                m.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
                m.startSize = new ParticleSystem.MinMaxCurve(3f, 5f);
                m.startRotation = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
                m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.86f, 0.86f, 0.88f, 0.9f), new Color(1f, 1f, 1f, 0.95f));
                m.gravityModifier = -0.01f;
                var e = trail.emission;
                e.rateOverTime = 10f;
                e.rateOverDistance = 0.6f;
                var s = trail.shape;
                s.shapeType = ParticleSystemShapeType.Cone;
                s.angle = 8f;
                s.radius = 0.8f;
                Grow(trail, 1f, 4.5f);
                Fade(trail, 0.85f, new Color(1f, 0.97f, 0.92f), new Color(0.8f, 0.8f, 0.82f));
                Reg("Trail S200 " + side, trail.gameObject);
            }

            // The engines' fire lights up the pad, the smoke and the rocket.
            if (nozzleAt.ContainsKey("Flame S200 R") && nozzleAt.ContainsKey("Flame S200 L"))
            {
                var glow = NewObject("Engine Light", flames.transform, typeof(Light), typeof(LightFlicker));
                glow.transform.position = (nozzleAt["Flame S200 R"] + nozzleAt["Flame S200 L"]) * 0.5f + Vector3.down * 3f;
                var lamp = glow.GetComponent<Light>();
                lamp.type = LightType.Point;
                lamp.color = new Color(1f, 0.62f, 0.3f);
                lamp.range = 70f;
                lamp.intensity = 2.2f;
                lamp.shadows = LightShadows.None;
                lamp.renderMode = LightRenderMode.ForceVertex;   // per-pixel it would draw everything near the pad a second time
                glow.GetComponent<LightFlicker>().baseIntensity = 2.2f;
                Reg("Engine Light", glow);
            }

            // Launch exhaust: a dense cloud billowing out over the pad, and two plumes blasting sideways out of the flame trench.
            // They emit only while the rocket is near the ground (non-looping), then linger, spread and drift up.
            var smoke = NewObject("Launch Smoke", padHost);
            smoke.transform.position = padBase + Vector3.up * 1f;
            var cloud = NewParticles("Exhaust Cloud", smoke.transform, puffSmokeMat, 280);
            cloud.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);   // circle in the ground plane: puffs blast outward along it
            {
                var m = cloud.main;
                m.duration = 10f;
                m.loop = false;
                m.startLifetime = new ParticleSystem.MinMaxCurve(16f, 24f);
                m.startSpeed = new ParticleSystem.MinMaxCurve(14f, 28f);
                m.startSize = new ParticleSystem.MinMaxCurve(9f, 16f);
                m.startRotation = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
                m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.84f, 0.84f, 0.86f, 0.95f), new Color(1f, 1f, 1f, 1f));
                m.gravityModifier = -0.015f;
                var e = cloud.emission;
                e.rateOverTime = 20f;
                e.SetBursts(new[] { new ParticleSystem.Burst(0f, 36) });
                var s = cloud.shape;
                s.shapeType = ParticleSystemShapeType.Circle;
                s.radius = 6f;
                var lv = cloud.limitVelocityOverLifetime;
                lv.enabled = true;
                lv.limit = 2.5f;
                lv.dampen = 0.12f;
                Grow(cloud, 0.45f, 3.4f);
                Fade(cloud, 0.9f, new Color(1f, 0.97f, 0.93f), new Color(0.88f, 0.88f, 0.9f));
            }
            foreach (int sign in new[] { 1, -1 })
            {
                var plume = NewParticles(sign > 0 ? "Trench Plume Downrange" : "Trench Plume Uprange", smoke.transform, puffSmokeMat, 160);
                plume.transform.position = padBase + downrange * (sign * 7f) + Vector3.up * 1.5f;
                plume.transform.rotation = Quaternion.LookRotation(downrange * sign + Vector3.up * 0.18f);
                var m = plume.main;
                m.duration = 9f;
                m.loop = false;
                m.startLifetime = new ParticleSystem.MinMaxCurve(10f, 15f);
                m.startSpeed = new ParticleSystem.MinMaxCurve(32f, 48f);
                m.startSize = new ParticleSystem.MinMaxCurve(6f, 11f);
                m.startRotation = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
                m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.86f, 0.86f, 0.88f, 0.95f), new Color(1f, 1f, 1f, 1f));
                m.gravityModifier = -0.02f;
                var e = plume.emission;
                e.rateOverTime = 16f;
                var s = plume.shape;
                s.shapeType = ParticleSystemShapeType.Cone;
                s.angle = 16f;
                s.radius = 2.5f;
                var lv = plume.limitVelocityOverLifetime;
                lv.enabled = true;
                lv.limit = 3f;
                lv.dampen = 0.08f;
                Grow(plume, 0.5f, 4f);
                Fade(plume, 0.9f, new Color(1f, 0.96f, 0.9f), new Color(0.88f, 0.88f, 0.9f));
            }
            Reg("Smoke", smoke);

            // A layer of scattered clouds about 200 m up that the rocket and camera climb through on the way out of the troposphere.
            var clouds = NewParticles("Cloud Layer", effectsFolder, cloudMat, 46);
            clouds.transform.position = padBase + Vector3.up * 200f;
            {
                var m = clouds.main;
                m.duration = 1f;
                m.loop = false;
                m.startLifetime = 100000f;
                m.startSpeed = 0f;
                m.startSize = new ParticleSystem.MinMaxCurve(90f, 220f);
                m.startRotation = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
                m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.94f, 0.95f, 0.97f, 0.85f), new Color(1f, 1f, 1f, 1f));
                var e = clouds.emission;
                e.rateOverTime = 0f;
                e.SetBursts(new[] { new ParticleSystem.Burst(0f, 46) });
                var s = clouds.shape;
                s.shapeType = ParticleSystemShapeType.Box;
                s.scale = new Vector3(1700f, 60f, 1700f);
            }
            clouds.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.Billboard;   // puffs facing the camera: never seen edge-on as flat sheets
            Undo.AddComponent<Animator>(clouds.gameObject);
            Undo.AddComponent<MaterialFader>(clouds.gameObject).opacity = 0f;
            Reg("Clouds", clouds.gameObject);

            var fade = NewObject("Screen Fade", effectsFolder, typeof(ScreenFade), typeof(Animator));
            var sf = fade.GetComponent<ScreenFade>();
            sf.material = fadeMat;
            Reg("Fade", fade);
        }

        static ParticleSystem NewParticles(string name, Transform parent, Material mat, int maxParticles)
        {
            var go = NewObject(name, parent, typeof(ParticleSystem));
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.sortMode = ParticleSystemSortMode.Distance;
            return ps;
        }

        // Particles grow from 'from' to 'to' times their start size, fastest at first, so they billow out.
        static void Grow(ParticleSystem ps, float from, float to)
        {
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, from, 0f, (to - from) * 3f),
                new Keyframe(0.35f, from + (to - from) * 0.7f), new Keyframe(1f, to, (to - from) * 0.4f, 0f)));
        }

        // Smoke fades in quickly, holds, and fades out; warm-tinted near the engines, cooling to grey.
        static void Fade(ParticleSystem ps, float peak, Color warm, Color cool)
        {
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(warm, 0f), new GradientColorKey(cool, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(peak, 0.06f), new GradientAlphaKey(peak * 0.75f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
        }

        // attached: particles move with the engine (local space) instead of staying where they were emitted.
        // The maximum and emission rate are deliberately below the old effect: about 20-28 particles are alive per
        // engine, but stretched textured tongues fill the plume more convincingly than many overlapping soft dots.
        static GameObject MakeFlame(string name, Transform parent, Vector3 worldPos, float size, Color hot, Color mid, float strength, bool attached = false)
        {
            var go = NewObject(name, parent, typeof(ParticleSystem));
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // emit straight down
            var ps = go.GetComponent<ParticleSystem>();
            ConfigureFlame(ps, size, hot, mid, strength, attached, flameMat);
            return go;
        }

        static void ConfigureFlame(ParticleSystem ps, float size, Color hot, Color mid, float strength, bool attached, Material material)
        {
            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.38f, 0.56f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(17f, 24f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.48f, size * 0.72f);
            main.startColor = new Color(hot.r, hot.g, hot.b, strength);
            main.simulationSpace = attached ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
            main.maxParticles = 64;
            var em = ps.emission;
            em.rateOverTime = 48f;
            // The quad is drawn ahead of each particle (negative stretch, below), and its bright part starts about a sixth
            // of the way along it, so the particles start that far up inside the bell and the fire begins at the nozzle mouth.
            float birthLength = size * 0.6f * 0.9f * 3.2f + 20.5f * 0.16f;
            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Cone;
            sh.angle = 3.2f;
            sh.radius = size * 0.13f;
            sh.radiusThickness = 0.75f;
            sh.position = new Vector3(0f, 0f, -Mathf.Min(0.16f * birthLength, 1f));   // the cone emits along +z, which points down
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(hot, 0.12f), new GradientColorKey(mid, 0.52f), new GradientColorKey(mid * 0.42f, 1f) },
                new[] { new GradientAlphaKey(strength, 0f), new GradientAlphaKey(strength, 0.28f), new GradientAlphaKey(0.72f * strength, 0.68f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.9f), new Keyframe(0.2f, 1.1f), new Keyframe(0.62f, 0.85f), new Keyframe(1f, 0.15f)));   // full width at the mouth

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            // negative: the stretched quad reaches ahead of the particle, down and away from the engine; positive scales
            // trail it back up, which drew the fire up the side of the booster instead of out of its nozzle
            renderer.lengthScale = -3.2f;
            renderer.velocityScale = -0.16f;
            renderer.cameraVelocityScale = 0f;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortMode = ParticleSystemSortMode.OldestInFront;
        }

        // =====================================================================================
        // Timeline
        // =====================================================================================
        class Lane
        {
            public string group;
            public string name;
            public List<ControlTrack> tracks = new List<ControlTrack>();
            public List<List<double[]>> spans = new List<List<double[]>>();
        }

        static Dictionary<string, TrackAsset> groups;
        static Dictionary<string, Lane> lanes;
        static double t;

        static TrackAsset Group(string name)
        {
            TrackAsset g;
            if (!groups.TryGetValue(name, out g))
            {
                g = tl.CreateTrack<GroupTrack>(null, name);
                groups[name] = g;
            }
            return g;
        }

        static string LaneGroup(string lane)
        {
            switch (lane)
            {
                case "Highlights": return "Rocket";
                case "Space": return "Space";
                case "Effects": return "Effects";
                default: return "UI";
            }
        }

        static void Show(string laneName, string key, double a, double b, bool leaveActive = false, bool particles = false)
        {
            var go = G(key);
            if (b <= a) b = a + 0.1;
            Lane lane;
            if (!lanes.TryGetValue(laneName, out lane))
            {
                lane = new Lane { name = laneName, group = LaneGroup(laneName) };
                lanes[laneName] = lane;
            }
            int idx = -1;
            for (int i = 0; i < lane.tracks.Count; i++)
                if (!lane.spans[i].Any(s => a < s[1] - 1e-6 && b > s[0] + 1e-6)) { idx = i; break; }
            if (idx < 0)
            {
                var tr = tl.CreateTrack<ControlTrack>(Group(lane.group), lane.tracks.Count == 0 ? laneName : laneName + " " + (lane.tracks.Count + 1));
                lane.tracks.Add(tr);
                lane.spans.Add(new List<double[]>());
                idx = lane.tracks.Count - 1;
            }
            var clip = lane.tracks[idx].CreateClip<ControlPlayableAsset>();
            clip.start = a;
            clip.duration = b - a;
            clip.displayName = key;
            var cp = (ControlPlayableAsset)clip.asset;
            var id = new PropertyName(Guid.NewGuid().ToString("N"));
            cp.sourceGameObject.exposedName = id;
            dir.SetReferenceValue(id, go);
            cp.updateParticle = particles;
            cp.updateDirector = false;
            cp.updateITimeControl = false;
            cp.searchHierarchy = false;
            cp.postPlayback = leaveActive ? ActivationControlPlayable.PostPlaybackState.Active : ActivationControlPlayable.PostPlaybackState.Revert;
            lane.spans[idx].Add(new[] { a, b });
        }

        static void Mark(double time, MissionCommand command, string argument, string name)
        {
            var m = tl.markerTrack.CreateMarker<MissionMarker>(time);
            m.command = command;
            m.argument = argument;
            m.name = name;
        }

        // Places a voice-over clip at the cursor and moves the cursor past it. Returns {start, end}.
        static double[] Say(string clipName, double gapAfter = 0.35)
        {
            var ac = AssetDatabase.LoadAssetAtPath<AudioClip>(VoFolder + "/" + clipName + ".mp3");
            if (ac == null) throw new Exception("voice-over clip missing: " + clipName);
            var clip = voTrack.CreateClip(ac);
            clip.start = t;
            clip.displayName = clipName;
            double a = t, b = t + ac.length;
            t = b + Math.Max(gapAfter, 1.0);   // at least a second of quiet after every voice-over, so none is cut off by the next
            voTimes[clipName] = new[] { a, b };
            return new[] { a, b };
        }

        static Dictionary<string, double[]> voTimes;   // voice-over clip name -> {start, end}, filled by Say()

        static double[] V(string clipName)
        {
            double[] r;
            if (!voTimes.TryGetValue(clipName, out r)) throw new Exception("internal: voice-over not placed yet: " + clipName);
            return r;
        }

        static AnimationTrack AnimTrack(string group, string name, Animator host)
        {
            var tr = tl.CreateTrack<AnimationTrack>(Group(group), name);
            dir.SetGenericBinding(tr, host);
            return tr;
        }

        static double timelineEnd;

        // Places an animation clip on the track and keeps its final pose until the end of the Timeline.
        static void Play(AnimationTrack track, AnimationClip clip, double start)
        {
            HoldUntil(clip, timelineEnd - start);
            var c = track.CreateClip(clip);
            c.start = start;
            c.displayName = clip.name;
        }

        static void HoldUntil(AnimationClip clip, double duration)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve.length == 0 || curve[curve.length - 1].time >= duration) continue;
                curve.AddKey(new Keyframe((float)duration, curve[curve.length - 1].value, 0f, 0f));
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }
        }

        static AnimationClip NewClip(string name)
        {
            string path = AnimFolder + "/" + name + ".anim";
            AssetDatabase.DeleteAsset(path);
            var clip = new AnimationClip { name = name, frameRate = 30f };
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        static void Curve(AnimationClip clip, string path, Type type, string property, params Keyframe[] keys)
        {
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, type, property), new AnimationCurve(keys));
        }

        static Keyframe K(double time, float value)
        {
            return new Keyframe((float)time, value, 0f, 0f);   // flat tangents = ease in and out
        }

        static void PositionCurves(AnimationClip clip, string path, params KeyValuePair<double, Vector3>[] pts)
        {
            string[] axes = { "x", "y", "z" };
            for (int a = 0; a < 3; a++)
                Curve(clip, path, typeof(Transform), "m_LocalPosition." + axes[a], pts.Select(p => K(p.Key, p.Value[a])).ToArray());
        }

        static KeyValuePair<double, Vector3> P(double time, Vector3 v)
        {
            return new KeyValuePair<double, Vector3>(time, v);
        }

        static void ScaleCurves(AnimationClip clip, string path, params KeyValuePair<double, Vector3>[] pts)
        {
            string[] axes = { "x", "y", "z" };
            for (int a = 0; a < 3; a++)
                Curve(clip, path, typeof(Transform), "m_LocalScale." + axes[a], pts.Select(p => K(p.Key, p.Value[a])).ToArray());
        }

        // A camera position path. Key() is a stop (flat tangents, so every move eases in and out);
        // Sample() adds a stretch of continuous motion whose tangents follow the movement.
        class CamPath
        {
            readonly List<double> times = new List<double>();
            readonly List<Vector3> values = new List<Vector3>();
            readonly List<Vector3> slopes = new List<Vector3>();

            public void Key(double time, Vector3 v)
            {
                Add(time, v, Vector3.zero);
            }

            public void Sample(double t0, double t1, double step, Func<double, Vector3> f)
            {
                int n = Mathf.Max(1, Mathf.CeilToInt((float)((t1 - t0) / step)));
                const double e = 0.01;
                for (int i = 0; i <= n; i++)
                {
                    double x = t0 + (t1 - t0) * i / n;
                    Add(x, f(x), (f(x + e) - f(x - e)) / (float)(2 * e));
                }
            }

            void Add(double time, Vector3 v, Vector3 slope)
            {
                int last = times.Count - 1;
                if (last >= 0 && time < times[last] + 0.02)
                {
                    if (time < times[last] - 0.02) warnings.Add(string.Format("camera key at {0:0.0}s is before the previous one ({1:0.0}s); moved", time, times[last]));
                    time = times[last] + 0.02;
                }
                times.Add(time);
                values.Add(v);
                slopes.Add(slope);
            }

            public void Write(AnimationClip clip, string path)
            {
                string[] axes = { "x", "y", "z" };
                for (int a = 0; a < 3; a++)
                {
                    var keys = new Keyframe[times.Count];
                    for (int i = 0; i < keys.Length; i++)
                        keys[i] = new Keyframe((float)times[i], values[i][a], slopes[i][a], slopes[i][a]);
                    Curve(clip, path, typeof(Transform), "m_LocalPosition." + axes[a], keys);
                }
            }
        }

        static Bounds PartBounds(Transform root, params string[] names)
        {
            var rs = names.Select(n => FindPart(root, n)).Where(p => p != null).Select(p => p.GetComponent<Renderer>()).Where(r => r != null).ToList();
            if (rs.Count < names.Length) warnings.Add("camera: some of " + string.Join(", ", names) + " not found");
            if (rs.Count == 0) return WorldBounds(root);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static void BuildTimeline()
        {
            tl = ScriptableObject.CreateInstance<TimelineAsset>();
            tl.name = "GSLV Mission";
            AssetDatabase.CreateAsset(tl, TimelinePath);
            dir.playableAsset = tl;
            groups = new Dictionary<string, TrackAsset>();
            lanes = new Dictionary<string, Lane>();
            voTimes = new Dictionary<string, double[]>();
            tl.CreateMarkerTrack();

            voTrack = tl.CreateTrack<AudioTrack>(Group("Audio"), "VO");
            dir.SetGenericBinding(voTrack, voSource.GetComponent<AudioSource>());
            sfxTracks = new List<AudioTrack>();
            sfxSpans = new List<List<double[]>>();

            // animation tracks (hosts hold an Animator, the animated object is a child, so offsets never apply)
            var camTrack = AnimTrack("Camera and Fade", "Camera Dolly", dollyHost.GetComponent<Animator>());
            var spaceCamTrack = AnimTrack("Camera and Fade", "Space Camera", spaceCamHost.GetComponent<Animator>());
            var fadeTrack = AnimTrack("Camera and Fade", "Screen Fade", G("Fade").GetComponent<Animator>());
            var flightTrack = AnimTrack("Rocket", "Rocket Flight", padHost.GetComponent<Animator>());
            var deployTrack = AnimTrack("Rocket", "Satellite Deploy (Map)", gto.parent.GetComponent<Animator>());
            var skyTrack = AnimTrack("Space", "Flight Backdrop", backdrop.GetComponent<Animator>());
            var atmosphereTrack = AnimTrack("Space", "Sky Atmosphere", G("SkyDome").GetComponent<Animator>());
            var cloudTrack = AnimTrack("Effects", "Cloud Layer", G("Clouds").GetComponent<Animator>());

            t = 1.5;
            string UI = "UI", CAP = "Captions", HL = "Highlights", SP = "Space", FX = "Effects";

            // ================= SCENE 1 - A satellite needs a rocket
            Mark(0, MissionCommand.Scene, "Scene 1", "SCENE 1 - A SATELLITE NEEDS A ROCKET!");
            double s1 = t;
            Times["s1_start"] = s1;
            var a1 = Say("S1_01_HowSatellitesReachSpace");
            var a2 = Say("S1_02_SatelliteServices");
            var a3 = Say("S1_03_SatelliteNeedsRocket");
            Show(SP, "Hologram", a1[1], a3[1] + 0.2);                 // hologram appears after the first question, spins, then vanishes
            Times["s1_hologram"] = a2[0] + 2.0;
            var a4 = Say("S1_04_MeetGSLV");
            var a5 = Say("S1_05_DevelopedByISRO");
            var a6 = Say("S1_06_MissionIntro");
            Show(HL, "HL Pad Whole", a3[1] + 0.3, a4[1] + 1.2);     // the GSLV illuminates
            Times["s1_rocket_lit"] = a4[0] + 1.0;
            Show(CAP, "Cap GSLV", a4[0], a5[1]);
            Times["s1_caption"] = a4[0] + 1.0;
            Show(CAP, "Cap Objective", a6[0], a6[1] + 1.0);
            Times["s1_objective"] = a6[0] + 1.5;

            // ================= SCENE 2 - Discover the GSLV
            Mark(t, MissionCommand.Scene, "Scene 2", "SCENE 2 - DISCOVER THE GSLV!");
            double s2 = t;
            var b1 = Say("S2_01_LookAtRocket");
            Show(HL, "HL Pad Whole", b1[0], b1[1] + 0.8);
            var b2 = Say("S2_02_ThreePropulsionStages");
            var b3 = Say("S2_03_LetsDiscover");
            var b4 = Say("S2_04_ClickExplorePrompt", 0.1);
            double gateExplore = t;
            Mark(gateExplore, MissionCommand.Gate, "Explore", "WAIT: learner clicks EXPLORE");
            Show(UI, "Explore Button", gateExplore - 0.05, gateExplore + 0.8);   // only while it can be clicked; gone before the S200 caption
            Show(CAP, "Cap Explore", s2, gateExplore + 0.8);
            Times["s2_explore"] = gateExplore - 0.5;
            t = gateExplore + 0.9;

            emphases.Clear();
            var c1 = Say("S2_05_S200SolidBoosters");
            Show(HL, "HL Pad S200", c1[0], c1[1] + 0.2);
            Show(CAP, "Cap S200", c1[0], c1[1] + 0.2);
            Emphasize(c1[0] + 0.3, c1[1], "Louncher.004", "Louncher.005");   // each booster swells about its own centre
            Emphasize(c1[0] + 0.3, c1[1], "Louncher.006");
            Times["s2_s200"] = c1[0] + 6.0;
            var c2 = Say("S2_06_L110LiquidStage");
            Show(HL, "HL Pad L110", c2[0], c2[1] + 0.2);
            Show(CAP, "Cap L110", c2[0], c2[1] + 0.2);
            Emphasize(c2[0] + 0.3, c2[1], "Louncher.001");
            Times["s2_l110"] = c2[0] + 4.0;
            var c3 = Say("S2_07_C25CryogenicStage", 0.2);
            Emphasize(c3[0] + 0.3, c3[1], "Louncher.009", "J_2S_J_2_0");
            var c4 = Say("S2_08_HydrogenOxygenFuel");
            Show(HL, "HL Pad C25", c3[0], c4[1] + 0.2);
            Show(CAP, "Cap C25", c3[0], c3[1] + 0.1);
            Show(CAP, "Cap Hydrogen", c4[0] + 0.3, c4[1]);
            Show(CAP, "Cap Oxygen", c4[0] + 1.6, c4[1]);
            Times["s2_labels"] = c4[0] + 3.5;
            var c5 = Say("S2_09_AllStagesTogether", 0.2);
            Show(HL, "HL Pad Three Stages", c5[0], c5[1] + 3.0);
            Show(CAP, "Cap All", c5[0], c5[1]);
            Show(CAP, "Cap Discovered", c5[1], c5[1] + 3.0);
            Times["s2_all"] = c5[0] + 5.0;
            t = c5[1] + 3.2;

            // ================= SCENE 3 - Ready for liftoff
            Mark(t, MissionCommand.Scene, "Scene 3", "SCENE 3 - READY FOR LIFTOFF!");
            double s3 = t;
            var d1 = Say("S3_01_SatelliteInFairing");
            var d2 = Say("S3_02_FairingProtects");
            // the fairing glows while the camera rises to it, turns see-through on the satellite inside, and closes up again
            double fairingIn = d1[0] + 2.6, fairingOut = d2[1] - 0.5;
            Show(HL, "HL Pad Fairing", d1[0] + 0.3, fairingIn - 0.1);
            FairingReveal(fairingIn, fairingOut);
            if (O.ContainsKey("Satellite Model")) Show(SP, "Satellite Model", fairingIn - 0.1, fairingOut + FairingFadeTime + 0.2);
            Times["s3_satellite"] = fairingIn + 2.0;
            var d3 = Say("S3_03_FinalChecks");
            for (int i = 1; i <= 3; i++) readyAt[i - 1] = d3[0] + 0.3 + (i - 1) * 0.9;
            var d4 = Say("S3_04_AllSystemsReady");
            var d5 = Say("S3_05_ClickLaunchPrompt", 0.1);
            double gateLaunch = t;
            Mark(gateLaunch, MissionCommand.Gate, "Launch", "WAIT: learner clicks LAUNCH");
            double panelEnd = gateLaunch + 0.8;
            Show(UI, "Readiness Panel", d3[0] - 0.3, panelEnd);   // centred, so it comes up after the satellite has been shown
            for (int i = 1; i <= 3; i++) Show("Readiness Lights", "Ready " + i, readyAt[i - 1], panelEnd);
            for (int i = 0; i < 3; i++) Sfx(TickSound, readyAt[i], 0, TickVolume);   // a tick as each check turns green
            Times["s3_panel"] = d3[0] + 1.0;
            Times["s3_checks"] = d3[0] + 2.5;
            Times["s3_launch"] = gateLaunch - 0.5;

            // countdown, liftoff and the flight: the scene's GSLV flies the whole way, the chase camera following it
            // launch sequence and countdown voice-overs (S3_07/S3_08); the numbers follow the spoken words, the engines light on "Ignition"
            double cd = panelEnd + 0.2, ign, lift;
            var countVo = AssetDatabase.LoadAssetAtPath<AudioClip>(VoFolder + "/S3_08_Countdown.mp3");
            if (countVo != null && AssetDatabase.LoadAssetAtPath<AudioClip>(VoFolder + "/S3_07_LaunchSequence.mp3") != null)
            {
                t = cd;
                Say("S3_07_LaunchSequence", 0.3);
                var k = Say("S3_08_Countdown");
                var words = WordOnsets(countVo, 6);                   // five, four, three, two, one, ignition
                for (int i = 0; i < 5; i++) Show(CAP, "Cap Count" + (5 - i), k[0] + words[i], k[0] + words[i + 1]);
                for (int i = 0; i < 5; i++) Sfx(AlarmSound, k[0] + words[i], k[0] + words[i + 1] - 0.05, AlarmVolume);   // a beep on each number, cut before the next
                cd = k[0] + words[0];
                ign = k[0] + words[5];
                lift = ign + 1.4;                                     // thrust builds, then the rocket leaves the pad
            }
            else
            {
                warnings.Add("S3_07_LaunchSequence / S3_08_Countdown not in VoiceOver yet: silent 3-2-1 countdown");
                Show(CAP, "Cap Count3", cd, cd + 1.0);
                Show(CAP, "Cap Count2", cd + 1.0, cd + 2.0);
                Show(CAP, "Cap Count1", cd + 2.0, cd + 3.0);
                for (int i = 0; i < 3; i++) Sfx(AlarmSound, cd + i, cd + i + 0.95, AlarmVolume);
                lift = cd + 3.0;
                ign = lift - 0.3;
            }
            fl = new Flight { lift = lift };
            Show(CAP, "Cap Liftoff", lift, lift + 2.2);
            Times["s3_count"] = cd + 1.4;
            Times["s3_lift"] = lift + 3.5;
            Times["s3_lift_late"] = lift + 7.0;
            double duck = lift + LaunchLoudHold;                                         // the launch roar starts fading down here
            var e1 = Place("S3_06_Liftoff", duck + LaunchDuckFade + LaunchVoGap);       // the voice-over once the roar is low
            Show(CAP, "Cap LiftoffOk", e1[1] + 1.0, e1[1] + 4.0);
            Times["s3_climb"] = e1[1] + 2.5;

            // ================= SCENE 4 - Journey through the sky
            double s4 = Math.Max(lift + 10.6, e1[1] + 1.0);
            Mark(s4, MissionCommand.Scene, "Scene 4", "SCENE 4 - JOURNEY THROUGH THE SKY!");
            t = s4;
            var f1 = Say("S4_01_ClimbingHigher");
            var f2 = Say("S4_02_AccelerateAgainstGravity");
            fl.swap = f2[0] + 1.2;                                 // through the clouds into space (white-out)
            fl.l110Ign = f2[0] + 2.6;                              // the L110 core lights in flight
            Mark(fl.swap, MissionCommand.ShowSpace, null, "SWAP: launch pad -> space (screen is white)");
            var f3 = Say("S4_03_StageSeparation");
            fl.s200 = f3[0] + 1.8;
            var f4 = Say("S4_04_ReducesMass");
            var f5 = Say("S4_05_ThinnerAtmosphere", 0.6);
            var f6 = Say("S4_06_FairingReleased");
            fl.fair = f6[0] + 1.2;
            var f7 = Say("S4_07_CryogenicUpperStage");
            fl.l110Sep = f7[0] + 0.2;
            fl.c25Ign = fl.l110Sep + 1.0;
            var f8 = Say("S4_08_HeadingToGTO");
            fl.map1 = f8[0] - 0.2;                                 // orbit map: Earth, the flight path and the transfer orbit
            var f9 = Say("S4_09_TransferOrbitPurpose", 0.4);
            double s4End = f9[1] + 1.0;
            Show(FX, "Flame S200 R", ign, fl.s200 + 0.05);
            Show(FX, "Flame S200 L", ign, fl.s200 + 0.05);
            Show(FX, "Flame L110", fl.l110Ign, fl.l110Sep + 0.05);
            if (O.ContainsKey("Flame C25")) Show(FX, "Flame C25", fl.c25Ign, fl.map1);
            Show(FX, "Smoke", ign, fl.swap);
            Show(SP, "Backdrop", fl.swap, fl.map1);
            Show(CAP, "Cap Separation", fl.s200, f4[1] + 0.5);
            Show(HL, "HL Pad Fairing", f5[1] - 1.4, fl.fair + 0.3);
            Show(CAP, "Cap Fairing", fl.fair + 0.1, f6[1] + 1.8);
            Show(HL, "HL Pad C25", fl.c25Ign, f7[1] + 0.5);
            Show(CAP, "Cap Cryo", f7[0], f7[1]);
            Show(SP, "Flight Path", fl.map1, s4End);
            Show(SP, "FP Marker 1", fl.map1, s4End);
            Show(SP, "FP Marker 2", fl.map1, s4End);
            Show(SP, "FP Marker 3", fl.map1, s4End);
            Show(SP, "GTO", f8[0] + 0.3, f8[0] + 0.5, true);     // stretched to the end of the Timeline below
            Show(CAP, "Cap GTO", f8[0], f9[0] + 2.5);
            Show(SP, "GTO Glow", f8[0] + 1.2, f8[1] + 0.5);
            Times["s4_start"] = fl.swap + 2.5;
            Times["s4_track"] = f2[1];
            Times["s4_sep"] = fl.s200 + 3.0;
            Times["s4_fairing"] = fl.fair + 2.5;
            Times["s4_upper"] = fl.l110Sep + 3.5;
            Times["s4_gto"] = f8[0] + 3.0;
            t = s4End + 0.3;

            // ================= SCENE 5 - Deploy the satellite (back beside the upper stage, full scale)
            Mark(t, MissionCommand.Scene, "Scene 5", "SCENE 5 - DEPLOY THE SATELLITE!");
            double s5 = t;
            fl.flight2 = s5 - 0.1;
            Show(SP, "Map", fl.map1, fl.flight2);
            Show(SP, "MapCam", fl.map1, fl.flight2);
            var g1 = Say("S5_01_FinalPartOfMission");
            var g2 = Say("S5_02_ReachedTransferOrbit");
            var g3 = Say("S5_03_MustSeparate", 0.2);
            double deployBtn = g3[1] - 0.6;
            var g4 = Say("S5_04_ClickDeployPrompt", 0.1);
            double gateDeploy = t;
            Mark(gateDeploy, MissionCommand.Gate, "Deploy", "WAIT: learner clicks DEPLOY SATELLITE");
            Show(CAP, "Cap DeployReady", s5, gateDeploy + 1.0);
            Show(HL, "HL Pad C25", g2[0], g2[1] + 0.5);
            bool hasSat = O.ContainsKey("HL Flight Satellite");
            if (hasSat) Show(HL, "HL Flight Satellite", g2[0], g2[1] + 0.5);
            Show(UI, "Deploy Button", deployBtn, gateDeploy + 1.0);
            Times["s5_start"] = s5 + 1.5;
            Times["s5_deploy"] = gateDeploy - 0.5;
            t = gateDeploy + 0.4;
            double sepStart = t;
            fl.sepStart = sepStart;
            var h1 = Say("S5_05_SeparationSuccessful", 5.0);   // time to watch the satellite drift away before the map
            if (hasSat) Show(HL, "HL Flight Satellite", h1[0], h1[1] + 1.5);
            Show(CAP, "Cap Deployed", h1[0], h1[1] + 2.0);
            var h2 = Say("S5_06_ReachFinalOrbit");
            fl.map2 = h2[0] - 0.2;                                 // orbit map again: the final orbit, then the quiz
            var h3 = Say("S5_07_MissionComplete", 0.5);
            double h3e = h3[1] + 1.8;
            Show(SP, "Backdrop", fl.flight2, fl.map2);
            Show(SP, "Map", fl.map2, fl.map2 + 0.5, true);         // stretched to the end below
            Show(SP, "MapCam", fl.map2, fl.map2 + 0.5, true);
            Show(SP, "Satellite", fl.map2, fl.map2 + 0.5, true);   // stretched below
            Show(HL, "HL Deploy Satellite", fl.map2 + 0.6, h3e);
            Show(SP, "Final Orbit", h2[0], h3e);
            Show(SP, "Transfer Arrow", h2[0], h3e);
            Show(CAP, "Cap Success", h3[0], h3[1] + 1.5);
            Times["s5_after"] = sepStart + 2.5;
            Times["s5_drift"] = h1[1] + 3.5;
            Times["s5_orbit"] = h2[0] + 4.0;
            Times["s5_success"] = h3[0] + 1.5;
            t = h3[1] + 2.2;

            // ---- the climb in numbers: Timeline seconds after lift-off -> mission time -> altitude and speed (illustrative GSLV
            // figures). The tropopause (12 km) comes just as the rocket climbs through the clouds into space view.
            var missionTime = new[] { new[] { 0.0, 0 }, new[] { fl.swap - lift, 75 }, new[] { fl.s200 - lift, 128 }, new[] { fl.fair - lift, 215 },
                new[] { fl.l110Sep - lift, 306 }, new[] { fl.map1 - lift, 600 }, new[] { fl.flight2 - lift, 960 }, new[] { sepStart - lift, 975 }, new[] { sepStart - lift + 60, 1035 } };
            var climb = new[] { new[] { 0.0, 0, 0 }, new[] { 60.0, 5, 0.4 }, new[] { 75.0, 12, 0.7 }, new[] { 108.0, 40, 1.2 }, new[] { 127.0, 58, 1.9 },
                new[] { 195.0, 115, 3.2 }, new[] { 306.0, 180, 5.6 }, new[] { 500.0, 175, 8.0 }, new[] { 969.0, 170, 10.2 }, new[] { 1100.0, 180, 10.2 } };
            Func<double[][], double, int, double> lookup = (tb, x, col) =>
            {
                if (x <= tb[0][0]) return tb[0][col];
                for (int i = 1; i < tb.Length; i++)
                    if (x <= tb[i][0]) return tb[i - 1][col] + (tb[i][col] - tb[i - 1][col]) * (x - tb[i - 1][0]) / (tb[i][0] - tb[i - 1][0]);
                return tb[tb.Length - 1][col];
            };
            fl.altKm = tt => lookup(climb, lookup(missionTime, tt - lift, 1), 1);
            Func<double, double> speedKms = tt => lookup(climb, lookup(missionTime, tt - lift, 1), 2);
            Func<double, double> reaches = km => { for (double x = lift; x < fl.map1; x += 0.05) if (fl.altKm(x) >= km) return x; return fl.map1; };
            double t12 = reaches(12), t50 = reaches(50), t85 = reaches(85);
            Show(CAP, "Cap Layer 1", lift + 2.5, t12 - 0.3);
            Show(CAP, "Cap Layer 2", t12 + 0.8, t50);
            Show(CAP, "Cap Layer 3", t50, t85);
            Show(CAP, "Cap Layer 4", t85, fl.map1 - 0.3);
            Show(UI, "Telemetry", lift + 0.5, fl.map1);
            Show(UI, "Telemetry", fl.flight2, fl.map2);
            var tele = G("Telemetry").GetComponent<FlightTelemetry>();
            Undo.RecordObject(tele, UndoName);
            tele.liftTime = lift;
            tele.table = Enumerable.Range(0, (int)((fl.map2 - lift) / 0.5) + 2)
                .Select(i => { double x = lift + i * 0.5; return new Vector3((float)(x - lift), (float)fl.altKm(x), (float)speedKms(x)); }).ToArray();
            Times["s3_tropo"] = lift + 6.0;
            Times["s4_strato"] = (t12 + t50) * 0.5;
            Times["s4_thermo"] = t85 + 1.5;

            // the sky, the clouds the rocket climbs through, the boosters' smoke trails and the engines' glow
            // the heavy satellite and C25 engine meshes only exist from the white flash into space until the orbit map takes over
            foreach (string k in new[] { "Satellite Model", "C25 Engine Model" })
                if (O.ContainsKey(k)) Show(SP, k, fl.swap - 0.1, fl.map2);
            Show(SP, "SkyDome", lift - 0.5, fl.map1);
            Show(SP, "SkyDome", fl.flight2, fl.map2);
            Show(FX, "Clouds", lift - 1.0, fl.swap + 9.0);
            foreach (string k in new[] { "Trail S200 R", "Trail S200 L" })
                if (O.ContainsKey(k)) Show(FX, k, lift - 0.1, fl.swap);
            if (O.ContainsKey("Engine Light")) Show(FX, "Engine Light", ign, fl.swap);

            // ================= SCENE 6 - Mission complete (quiz)
            Mark(t, MissionCommand.Scene, "Scene 6", "SCENE 6 - MISSION GSLV COMPLETE!");
            double s6 = t;
            var i1 = Say("S6_01_Congratulations");
            var i2 = Say("S6_02_QuizIntro");
            double quizStart = i2[0];
            Mark(quizStart, MissionCommand.Quiz, "1", "QUIZ: show question 1");
            string[,] qv = { { "S6_03_Q1_Question", "S6_04_Q1_Correct" }, { "S6_05_Q2_Question", "S6_06_Q2_Correct" }, { "S6_07_Q3_Question", "S6_08_Q3_Correct" },
                             { "S6_09_Q4_Question", "S6_10_Q4_Correct" }, { "S6_11_Q5_Question", "S6_12_Q5_Correct" } };
            double quizEnd = 0;
            for (int q = 0; q < 5; q++)
            {
                var ask = Say(qv[q, 0], 0.1);
                Mark(ask[0], MissionCommand.Quiz, (q + 1).ToString(), "QUIZ: show question " + (q + 1));
                double gate = t;
                Mark(gate, MissionCommand.Gate, "Quiz", "WAIT: learner answers question " + (q + 1));
                t = gate + 0.6;
                var ok = Say(qv[q, 1], 0.5);
                quizEnd = ok[1] + 0.4;
                Times["s6_q" + (q + 1)] = ask[1] - 0.3;
            }
            Show(UI, "Quiz Panel", quizStart, quizEnd);
            t = quizEnd + 0.2;
            Show(SP, "ShowcaseModel", t, t + 1, true);   // widened below
            double fin = t;
            var j1 = Say("S6_14_OutstandingWork");
            var j2 = Say("S6_15_StagesWorkTogether");
            var j3 = Say("S6_16_JourneyRecap");
            var j4 = Say("S6_17_MissionAccomplished", 1.0);
            Show(UI, "Achievements", fin, j4[1] + 8.0);
            Show("Badges", "Badge Lit 1", fin + 0.8, j4[1] + 8.0);
            Show("Badges", "Badge Lit 2", fin + 1.6, j4[1] + 8.0);
            Show("Badges", "Badge Lit 3", j4[0], j4[1] + 8.0);
            Show(HL, "HL Showcase Whole", j2[0], j2[1] + 0.3);
            Show(HL, "HL Deploy Satellite", j3[0], j3[1] + 0.3);
            Show(SP, "GTO Glow", j3[0], j3[1] + 0.3);
            Show(CAP, "Cap Complete", j4[1] + 0.2, j4[1] + 8.0);
            Times["s6_final"] = fin + 2.5;
            Times["s6_recap"] = j3[0] + 2.0;
            Times["s6_end"] = j4[1] + 3.0;
            double end = j4[1] + 8.0;
            // RESTART / EXIT once everything else has finished: switched on by MissionFlow, so no clip end can hide it again
            Mark(end, MissionCommand.EndMenu, null, "END: show the end menu (RESTART / EXIT)");

            // ---------------- sound: one-shot effects on the Timeline, music and ambience beds that keep playing while a gate holds it
            // engine roar from ignition: loud through lift-off, then cross-faded to the same sound (in step) at a low bed under
            // the lift-off voice-over, fading out after the white-out
            double roar = ign - 0.1;
            Sfx("Rocket launch.mp3", roar, duck + LaunchDuckFade, LaunchLoudVolume, 0.3, LaunchDuckFade);
            Sfx("Rocket launch.mp3", duck, fl.swap + 3.0, LaunchBedVolume, LaunchDuckFade, 3.0, duck - roar);
            Sfx("rocket launched.mp3", lift, duck + LaunchDuckFade, 0.9f, 0, LaunchDuckFade);
            foreach (double sep in new[] { fl.s200, fl.fair, fl.l110Sep, fl.sepStart })
                Sfx("Engine detach .mp3", sep - 0.1, 0, 0.75f);
            Bed("Music Pads", "idoberg-ambient-pads-loop.mp3", 0.0, cd, 0.16f, 3f, 3f);              // scenes 1-3, fading under the launch call
            Bed("Space Ambience", "Space sound .mp3", fl.swap - 0.5, s6 + 1.5, 0.3f, 3f, 3f);       // the flight in space and the deployment
            Bed("Space Music", "space-music-ambient-22325.mp3", s6 - 1.0, end, 0.2f, 4f, 5f);       // quiz and finale

            // stretch the clips that had to be created before their end time was known
            // (to half a second past the end menu's marker, so the Timeline ends with the menu up over the finished scene)
            ExtendClip("GTO", end + 0.5);
            ExtendClip("Satellite", end + 0.5);
            ExtendClip("ShowcaseModel", end + 0.5);
            ExtendClip("Map", end + 0.5);
            ExtendClip("MapCam", end + 0.5);

            // ---------------- animation clips
            timelineEnd = end;
            BuildAnimations(camTrack, spaceCamTrack, fadeTrack, flightTrack, deployTrack, skyTrack, atmosphereTrack, cloudTrack, gateExplore);

            report.AppendLine(string.Format("Timeline: {0:0.0} s of content ({1:0.0} min), 5 gates, {2} tracks", end, end / 60.0, tl.outputTrackCount));
            report.AppendLine("Gate times: Explore " + gateExplore.ToString("0.0") + ", Launch " + gateLaunch.ToString("0.0") + ", Deploy " + gateDeploy.ToString("0.0"));
            Times["end"] = end + 0.5;   // the auto-test's last shot shows the end menu
            File.WriteAllLines(TimesFile, Times.Select(kv => kv.Key + "=" + kv.Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).ToArray());
        }

        static double[] readyAt = new double[3];

        // A voice-over that does not advance the main cursor (used while another sequence runs underneath).
        static double[] Place(string clipName, double start)
        {
            double keep = t;
            t = start;
            var r = Say(clipName);
            t = keep;
            return r;
        }

        // Start times (s) of 'count' spoken words in a clip: the longest pauses between its loud parts split it into words.
        static double[] WordOnsets(AudioClip clip, int count)
        {
            var even = Enumerable.Range(0, count).Select(i => (double)clip.length * i / count).ToArray();
            if (clip.loadState != AudioDataLoadState.Loaded) clip.LoadAudioData();
            var data = new float[clip.samples * clip.channels];
            if (!clip.GetData(data, 0)) { warnings.Add("countdown: could not read " + clip.name + ", numbers timed evenly"); return even; }
            const double Win = 0.02;
            int per = Math.Max(1, (int)(clip.frequency * Win)) * clip.channels;
            var rms = new double[data.Length / per];
            for (int w = 0; w < rms.Length; w++)
            {
                double s = 0;
                for (int i = w * per; i < (w + 1) * per; i++) s += data[i] * data[i];
                rms[w] = Math.Sqrt(s / per);
            }
            double floor = rms.Max() * 0.08;
            int first = Array.FindIndex(rms, v => v > floor), last = Array.FindLastIndex(rms, v => v > floor);
            var gaps = new List<int[]>();   // {first quiet window, number of quiet windows}
            for (int w = first; first >= 0 && w <= last;)
            {
                if (rms[w] > floor) { w++; continue; }
                int a = w;
                while (w <= last && rms[w] <= floor) w++;
                gaps.Add(new[] { a, w - a });
            }
            if (gaps.Count < count - 1) { warnings.Add("countdown: only " + (gaps.Count + 1) + " words heard in " + clip.name + ", numbers timed evenly"); return even; }
            var onsets = new List<double> { first * Win };
            onsets.AddRange(gaps.OrderByDescending(g => g[1]).Take(count - 1).OrderBy(g => g[0]).Select(g => (g[0] + g[1]) * Win));
            report.AppendLine("Countdown words at " + string.Join(", ", onsets.Select(o => o.ToString("0.00")).ToArray()) + " s into " + clip.name);
            return onsets.ToArray();
        }

        // A sound effect from Assets A/SFX on the first SFX track free at that time (overlapping clips on one track would cross-fade).
        // clipIn: seconds into the sound where the clip starts.
        static void Sfx(string file, double start, double end, float volume, double easeIn = 0, double easeOut = 0, double clipIn = 0)
        {
            var ac = AssetDatabase.LoadAssetAtPath<AudioClip>(SfxFolder + "/" + file);
            if (ac == null) { warnings.Add("sound effect missing: " + SfxFolder + "/" + file); return; }
            if (end <= start || end > start + ac.length - clipIn) end = start + ac.length - clipIn;
            int idx = sfxSpans.FindIndex(spans => !spans.Any(s => start < s[1] && end > s[0]));
            if (idx < 0)
            {
                string suffix = sfxTracks.Count == 0 ? "" : " " + (sfxTracks.Count + 1);
                var src = NewObject("SFX Source" + suffix, FindRoot("AUDIO"), typeof(AudioSource)).GetComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                var tr = tl.CreateTrack<AudioTrack>(Group("Audio"), "SFX" + suffix);
                dir.SetGenericBinding(tr, src);
                sfxTracks.Add(tr);
                sfxSpans.Add(new List<double[]>());
                idx = sfxTracks.Count - 1;
            }
            var clip = sfxTracks[idx].CreateClip(ac);
            clip.start = start;
            clip.clipIn = clipIn;
            clip.duration = end - start;
            clip.displayName = Path.GetFileNameWithoutExtension(file).Trim();
            clip.easeInDuration = easeIn;
            clip.easeOutDuration = easeOut;
            var so = new SerializedObject(clip.asset);
            so.FindProperty("m_ClipProperties.volume").floatValue = volume;
            so.ApplyModifiedPropertiesWithoutUndo();
            sfxSpans[idx].Add(new[] { start, end });
        }

        // A looping music or ambience bed under AUDIO, streamed from disk; AudioBed plays it while the Timeline is between start and end.
        static void Bed(string name, string file, double start, double end, float volume, float fadeIn, float fadeOut)
        {
            string path = SfxFolder + "/" + file;
            var imp = AssetImporter.GetAtPath(path) as AudioImporter;
            if (imp == null) { warnings.Add("music missing: " + path); return; }
            var settings = imp.defaultSampleSettings;
            if (settings.loadType != AudioClipLoadType.Streaming)
            {
                settings.loadType = AudioClipLoadType.Streaming;   // long loops: no decompressed copy in memory on the headset
                imp.defaultSampleSettings = settings;
                imp.SaveAndReimport();
            }
            var go = NewObject(name, FindRoot("AUDIO"), typeof(AudioSource));
            var src = go.GetComponent<AudioSource>();
            src.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            src.volume = 0f;
            var bed = Undo.AddComponent<AudioBed>(go);
            bed.director = dir;
            bed.start = start;
            bed.end = end;
            bed.volume = volume;
            bed.fadeIn = fadeIn;
            bed.fadeOut = fadeOut;
        }

        static void ExtendClip(string key, double end)
        {
            foreach (var lane in lanes.Values)
                foreach (var track in lane.tracks)
                    foreach (var clip in track.GetClips())
                    {
                        var cp = clip.asset as ControlPlayableAsset;
                        if (cp == null || clip.displayName != key) continue;
                        if (cp.postPlayback == ActivationControlPlayable.PostPlaybackState.Active || clip.duration < 1.5)
                            clip.duration = Math.Max(clip.duration, end - clip.start);
                    }
        }

        static void BuildAnimations(AnimationTrack camTrack, AnimationTrack spaceCamTrack, AnimationTrack fadeTrack, AnimationTrack flightTrack,
            AnimationTrack deployTrack, AnimationTrack skyTrack, AnimationTrack atmosphereTrack, AnimationTrack cloudTrack, double gateExplore)
        {
            double lift = fl.lift, tEnd = timelineEnd;

            // ---- the flight, integrated here and keyed onto Lift (pivot at the rocket's base): a slow lift-off that keeps building speed,
            // a gravity turn towards downrange (the learner's right, so it is seen side-on), and a coast once the C25 has cut off
            // (during the orbit map). Speeds are not realistic; nothing near the rocket in space shows them except its exhaust.
            Vector3 fwd = Vector3.ProjectOnPlane(rig.forward, Vector3.up).normalized;
            Vector3 downrange = Vector3.Cross(Vector3.up, fwd).normalized;
            Vector3 pitchAxis = Vector3.Cross(Vector3.up, downrange).normalized;   // turns 'up' towards downrange
            double tauCut = fl.map1 - lift;
            Func<double, double> pitchDeg = tau => 72.0 * Math.Pow(Mathf.Clamp01((float)((tau - 6.0) / 60.0)), 1.35);
            Func<double, double> speed = tau => tau <= 0 || tau > tauCut ? 0 : 4.0 + 0.55 * Math.Pow(tau, 1.25);
            const double dtSim = 1.0 / 60.0;
            int nSim = (int)Math.Ceiling((tEnd - lift) / dtSim) + 2;
            var travel = new Vector3[nSim];
            for (int i = 1; i < nSim; i++)
            {
                double tau = (i - 0.5) * dtSim, th = pitchDeg(tau) * Mathf.Deg2Rad;
                travel[i] = travel[i - 1] + (Vector3.up * (float)Math.Cos(th) + downrange * (float)Math.Sin(th)) * (float)(speed(tau) * dtSim);
            }
            Func<double, Vector3> disp = tt =>
            {
                double x = (tt - lift) / dtSim;
                if (x <= 0) return Vector3.zero;
                int i = Math.Min((int)x, nSim - 2);
                return Vector3.Lerp(travel[i], travel[i + 1], (float)(x - i));
            };
            Func<double, Quaternion> attitude = tt => Quaternion.AngleAxis((float)pitchDeg(tt - lift), pitchAxis);
            Vector3 liftRest = padLift.localPosition;   // GSLV Rocket (Pad) sits at the origin, so this is also the pivot's world position

            var flightClip = NewClip("Rocket Flight");
            var lp = new CamPath();
            lp.Key(0, liftRest);
            lp.Key(lift, liftRest);
            lp.Sample(lift, tEnd, 0.25, tt => liftRest + disp(tt));
            lp.Write(flightClip, "Lift");
            SampleRotation(flightClip, "Lift", lift, tEnd, 0.25, Quaternion.identity, attitude);

            // stages leave the rocket (model units: X sideways, Y depth, Z along the rocket; 1 unit = model scale in metres)
            Func<string, Transform> part = n => FindPart(modelRoot, n);
            Jettison(flightClip, new[] { part("Louncher.004"), part("Louncher.005") }, fl.s200, 7.0, new Vector3(-34f, 0f, -6f), 220f, Vector3.up, -35f);
            Jettison(flightClip, new[] { part("Louncher.006") }, fl.s200, 7.0, new Vector3(34f, 0f, -6f), 220f, Vector3.up, 35f);
            Jettison(flightClip, new[] { part("Louncher.002") }, fl.fair, 6.5, new Vector3(0f, 30f, 6f), 160f, Vector3.right, -40f);
            Jettison(flightClip, new[] { part("Louncher.003") }, fl.fair, 6.5, new Vector3(0f, -30f, 6f), 160f, Vector3.right, 40f);
            Jettison(flightClip, new[] { part("Louncher.001") }, fl.l110Sep, 7.0, new Vector3(3f, 0f, 0f), 260f, Vector3.up, 18f);

            // satellite deployment: the satellite is pushed gently forward along the rocket's axis and slowly turns; the C25 eases back
            double sep = fl.sepStart;
            Func<double, float> satPush = tt =>
            {
                float u = Mathf.Clamp01((float)((tt - sep) / 6.0));
                return 30f * (1f - (1f - u) * (1f - u)) + 0.8f * Mathf.Max(0f, (float)(tt - sep - 6.0));
            };
            if (flightSat != null)
            {
                string sp0 = AnimationUtility.CalculateTransformPath(flightSat, padHost);
                Vector3 s0p = flightSat.localPosition;
                Quaternion s0q = flightSat.localRotation;
                var satPath = new CamPath();
                satPath.Key(0, s0p);
                satPath.Key(sep, s0p);
                satPath.Sample(sep, sep + 14.0, 0.2, tt => s0p + Vector3.forward * satPush(tt));
                satPath.Write(flightClip, sp0);
                SampleRotation(flightClip, sp0, sep, sep + 14.0, 0.2, s0q, tt => Quaternion.AngleAxis(3f * (float)(tt - sep), Vector3.forward) * s0q);
            }
            var upper = part("Empty.001");
            if (upper != null)
            {
                string up0 = AnimationUtility.CalculateTransformPath(upper, padHost);
                Vector3 u0p = upper.localPosition;
                var upperPath = new CamPath();
                upperPath.Key(0, u0p);
                upperPath.Key(sep, u0p);
                upperPath.Sample(sep, sep + 10.0, 0.2, tt => u0p + Vector3.back * (35f * Mathf.Clamp01((float)((tt - sep) / 10.0))));
                upperPath.Write(flightClip, up0);
            }
            ApplyEmphasis(flightClip);   // the stages swelling while Scene 2 introduces them
            Play(flightTrack, flightClip, 0);
            report.AppendLine(string.Format("Flight: {0:0} m up when it enters space, {1:0} m travelled by C25 cut-off; S200 off {2:0.0} s, fairing {3:0.0} s, L110 off {4:0.0} s after lift-off",
                disp(fl.swap).y, disp(fl.map1).magnitude, fl.s200 - lift, fl.fair - lift, fl.l110Sep - lift));

            // Curves that follow the real altitude the flight stands for (sampled, with tangents along the motion).
            Func<Func<double, float>, double, double, Keyframe[]> sampled = (f, t0, t1) =>
            {
                var keys = new List<Keyframe>();
                for (double x = t0; x <= t1 + 1e-6; x += 0.5)
                {
                    float slope = (float)((f(x + 0.05) - f(x - 0.05)) / 0.1);
                    keys.Add(new Keyframe((float)x, f(x), slope, slope));
                }
                return keys.ToArray();
            };
            Func<double, float> km = tt => (float)fl.altKm(tt);

            // Earth below curves away more as the rocket climbs (its distance below the eyes grows with altitude)
            var skyClip = NewClip("Flight Backdrop");
            Func<double, float> below = tt => 20f + 0.55f * km(tt);
            var hk = new List<Keyframe> { K(0, below(fl.swap)), K(fl.swap, below(fl.swap)) };
            hk.AddRange(sampled(below, fl.swap + 0.5, fl.map1));
            hk.Add(K(fl.flight2, below(fl.flight2)));
            hk.Add(K(tEnd, below(fl.flight2)));
            Curve(skyClip, "", typeof(SkyBackdrop), "height", hk.ToArray());
            Play(skyTrack, skyClip, 0);

            // the sky: fades in over the pad's own sky after lift-off, then darkens with altitude from blue to black
            var atmClip = NewClip("Sky Atmosphere");
            var ak = new List<Keyframe> { K(0, 0f), K(lift, 0f) };
            ak.AddRange(sampled(km, lift + 0.5, fl.map1));
            ak.Add(K(fl.flight2, km(fl.flight2)));
            ak.Add(K(tEnd, km(fl.flight2)));
            Curve(atmClip, "", typeof(SkyAtmosphere), "altitudeKm", ak.ToArray());
            Curve(atmClip, "", typeof(SkyAtmosphere), "opacity", K(0, 0f), K(lift, 0f), K(fl.swap - 1.0, 1f), K(tEnd, 1f));
            Play(atmosphereTrack, atmClip, 0);

            // the cloud layer drifts into view after lift-off and thins out once the rocket is above it
            var cloudClip = NewClip("Cloud Layer");
            Curve(cloudClip, "", typeof(MaterialFader), "opacity", K(0, 0f), K(lift - 1.0, 0f), K(lift + 3.0, 1f), K(fl.swap + 1.0, 1f), K(fl.swap + 4.5, 0f), K(tEnd, 0f));
            Play(cloudTrack, cloudClip, 0);

            // ---- learner camera on the launch pad (dolly offsets; the learner keeps facing the rocket, only the position moves).
            // Still while the hologram is beside the learner, a slow reveal, a crane past each stage after EXPLORE,
            // up to the fairing, down to a low launch angle, then it rides up with the rocket.
            var rb = WorldBounds(modelRoot);
            float h = rb.size.y;
            Vector3 axis = new Vector3(rb.center.x, 0f, rb.center.z);
            Vector3 rigRest = rig.position - dolly.position;
            Vector3 toLearner = rigRest - axis;
            toLearner.y = 0f;
            toLearner.Normalize();
            Func<float, float, Vector3> E = (dist, y) => axis + toLearner * dist + Vector3.up * y - rigRest;   // eye at 'dist' from the rocket, height 'y'
            float yS200 = PartBounds(modelRoot, "Louncher.004", "Louncher.005", "Louncher.006").center.y;
            float yL110 = PartBounds(modelRoot, "Louncher.001").center.y + 1f;
            float yC25 = PartBounds(modelRoot, "Louncher.009").center.y;
            float yFairing = PartBounds(modelRoot, "Louncher.002", "Louncher.003").center.y;
            float yLaunch = rb.min.y + 0.22f * h, dLaunch = 2.05f * h;
            double c0 = gateExplore + 0.2;

            var pad = new CamPath();
            pad.Key(0, Vector3.zero);
            pad.Key(V("S1_03_SatelliteNeedsRocket")[1] + 0.3, Vector3.zero);
            pad.Key(V("S1_06_MissionIntro")[1], E(2.2f * h, rigRest.y + 1.5f));                // slow reveal: "Meet GSLV"
            pad.Key(c0, E(2.2f * h, rigRest.y + 1.5f));                                          // waits for EXPLORE
            pad.Key(c0 + 6.5, E(1.1f * h, yS200));                                               // S200 boosters
            pad.Key(V("S2_06_L110LiquidStage")[0], E(1.1f * h, yS200));
            pad.Key(V("S2_06_L110LiquidStage")[0] + 3.5, E(1.05f * h, yL110));                   // L110 core
            pad.Key(V("S2_07_C25CryogenicStage")[0], E(1.05f * h, yL110));
            pad.Key(V("S2_07_C25CryogenicStage")[0] + 3.5, E(1.05f * h, yC25));                  // C25, held for the fuel labels
            pad.Key(V("S2_09_AllStagesTogether")[0], E(1.05f * h, yC25));
            pad.Key(V("S2_09_AllStagesTogether")[0] + 4.5, E(1.85f * h, rb.center.y));          // all three stages together
            pad.Key(V("S3_01_SatelliteInFairing")[0], E(1.85f * h, rb.center.y));
            pad.Key(V("S3_01_SatelliteInFairing")[0] + 3.5, E(1.4f * h, yFairing));              // up to the fairing
            pad.Key(V("S3_03_FinalChecks")[0], E(1.4f * h, yFairing));
            pad.Key(V("S3_03_FinalChecks")[0] + 4.5, E(dLaunch, yLaunch));                       // low launch angle, held through LAUNCH and the countdown
            pad.Key(lift, E(dLaunch, yLaunch));

            // From lift-off a chase camera (like a launch broadcast): it looks at a point on the stack that moves up as stages fall away,
            // stays on the learner's side of the rocket, swings slowly round it, rises above it, and follows with a little lag, so the
            // camera trails the rocket as it accelerates. After deployment it closes in on the satellite and keeps circling.
            Vector3 pivotRest = liftRest;
            Vector3 satRel = (flightSat != null ? WorldBounds(flightSat).center : pivotRest + Vector3.up * (0.84f * h)) - pivotRest;
            float modelScale = modelRoot.lossyScale.y;
            Func<double, double, double, float> sm = (a, b, x) => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp((float)a, (float)b, (float)x));
            Func<double, Vector3> stack = tt =>
            {
                float f = 0.45f + 0.11f * sm(fl.s200, fl.s200 + 3, tt) + 0.10f * sm(fl.fair, fl.fair + 3, tt) + 0.12f * sm(fl.l110Sep, fl.l110Sep + 3, tt);
                return pivotRest + disp(tt) + attitude(tt) * (Vector3.up * (f * h));
            };
            Func<double, Vector3> satNow = tt => pivotRest + disp(tt) + attitude(tt) * (satRel + Vector3.up * (satPush(tt) * modelScale));
            Func<double, Vector3> focus = tt => Vector3.Lerp(stack(tt), satNow(tt), sm(sep, sep + 3, tt));
            // The flight's shot list: time, azimuth (degrees, + towards downrange), elevation (degrees, + camera above the rocket),
            // distance (rocket heights). The camera eases from one shot to the next; it only moves, it never turns the learner's view.
            // Azimuths stay at or below zero so the rocket stays right of centre, clear of the layer readouts on the left.
            var shots = new[]
            {
                new[] { lift - 2,            0.0,  -6.0, dLaunch / h },
                new[] { lift + 4,           -8.0, -10.0, 2.25 },   // stays low while the rocket rises out of its own smoke
                new[] { lift + 10,         -24.0,  -5.0, 2.0 },    // swings round as it clears the tower and starts to pitch over
                new[] { fl.swap - 1.5,     -12.0,   2.0, 1.9 },    // falls in behind, chasing it up into the clouds
                new[] { fl.swap + 4,       -20.0,   6.0, 1.8 },    // above the clouds, side-on against the darkening sky
                new[] { fl.s200 - 1,       -32.0,   0.0, 1.5 },    // close and level as the boosters let go
                new[] { fl.s200 + 5,       -12.0,  16.0, 1.7 },    // rises to watch them tumble back towards Earth
                new[] { fl.fair - 2,        -4.0,  10.0, 1.15 },   // in close on the nose for the fairing
                new[] { fl.l110Sep + 0.5,  -18.0,   8.0, 1.2 },    // side-on as the core stage drops away
                new[] { fl.l110Sep + 6,     -4.0,  14.0, 1.0 },    // the C25 burning, Earth's limb behind
                new[] { fl.flight2,        -10.0,  14.0, 0.6 },    // beside the upper stage and the satellite (Scene 5)
            }.OrderBy(s => s[0]).ToArray();
            Func<double, int, float> shot = (tt, k) =>
            {
                if (tt <= shots[0][0]) return (float)shots[0][k];
                for (int i = 1; i < shots.Length; i++)
                    if (tt <= shots[i][0]) return Mathf.Lerp((float)shots[i - 1][k], (float)shots[i][k], sm(shots[i - 1][0], shots[i][0], tt));
                return (float)shots[shots.Length - 1][k];
            };
            Func<double, Vector3> chase = tt =>
            {
                float dist = Mathf.Lerp(shot(tt, 3) * h, 0.42f * h, sm(sep, sep + 6, tt));
                float az = shot(tt, 1) + Mathf.Min(10f, 1.5f * Mathf.Max(0f, (float)(tt - sep)));
                float el = shot(tt, 2) - 10f * sm(sep, sep + 4, tt);   // after deployment the satellite rides above the captions
                float a = az * Mathf.Deg2Rad, e = el * Mathf.Deg2Rad;
                Vector3 side = (-fwd * Mathf.Cos(a) + downrange * Mathf.Sin(a)) * (dist * Mathf.Cos(e));
                return focus(tt) + side + Vector3.up * (dist * Mathf.Sin(e));
            };
            // From the launch view the camera eases into the chase as the rocket leaves the pad and follows it up through the clouds;
            // the white flash then hands over to space without a cut, the chase carrying on.
            Vector3 launchEye = E(dLaunch, yLaunch) + rigRest;
            const double camRate = 6.0, lead = 0.7;   // lag rate; share of the lag cancelled by aiming ahead of the moving rocket
            int nCam = (int)Math.Ceiling((tEnd - lift) / dtSim) + 2;
            var eyeAt = new Vector3[nCam];
            Vector3 eyePos = launchEye;
            for (int i = 0; i < nCam; i++)
            {
                double tt = lift + i * dtSim;
                Vector3 vel = (focus(tt + dtSim) - focus(tt)) / (float)dtSim;
                Vector3 want = chase(tt) + vel * (float)(lead / camRate);
                eyePos += (want - eyePos) * (float)(1.0 - Math.Exp(-dtSim * camRate));
                eyeAt[i] = eyePos;
            }
            pad.Sample(lift, tEnd, 0.25, tt =>
            {
                double x = (tt - lift) / dtSim;
                int i = Mathf.Clamp((int)x, 0, nCam - 2);
                return Vector3.Lerp(eyeAt[i], eyeAt[i + 1], (float)(x - i)) - rigRest;
            });
            var cam = NewClip("Camera Dolly Moves");
            pad.Write(cam, "Camera Dolly");
            Play(camTrack, cam, 0);
            Func<double, float> gap = tt => ((tt < lift ? launchEye : eyeAt[Mathf.Clamp((int)((tt - lift) / dtSim), 0, nCam - 1)]) - focus(tt)).magnitude;
            report.AppendLine(string.Format("Camera (pad): reveal to {0:0} m, stage crane at {1:0} m (S200 {2:0.0} / L110 {3:0.0} / C25 {4:0.0} / fairing {5:0.0} m high), launch view {6:0} m",
                2.2f * h, 1.1f * h, yS200, yL110, yC25, yFairing, dLaunch));
            report.AppendLine(string.Format("Camera (chase): {0:0} m from the rocket at lift-off+10 s, {1:0} m at S200 separation, {2:0} m at L110 separation, {3:0} m before deployment, {4:0} m from the satellite after it",
                gap(lift + 10), gap(fl.s200), gap(fl.l110Sep), gap(sep - 1), gap(sep + 8)));

            // ---- camera on the orbit map, keyed in the space set's frame (origin = where the learner's eyes were when space was shown,
            // +Z = the rig's forward): the authored wide view, then a slow push towards Earth for the finale.
            Vector3 earthC = switcher.transform.InverseTransformPoint(earthSystem.position);
            Vector3 wide = Vector3.zero, finale = Vector3.Lerp(wide, earthC, 0.18f);
            var sp = new CamPath();
            sp.Key(0, wide);
            sp.Key(V("S6_14_OutstandingWork")[0], wide);
            sp.Key(V("S6_17_MissionAccomplished")[1], finale);
            var spaceClip = NewClip("Space Camera Moves");
            sp.Write(spaceClip, "Space Camera Anchor");
            Play(spaceCamTrack, spaceClip, 0);

            // ---- screen fade: in at the start; white through the clouds into space; quick black cuts to and from the orbit map
            var fade = NewClip("Screen Fade");
            var alpha = new List<Keyframe> { K(0, 1f), K(1.2, 0f), K(fl.swap - 0.6, 0f), K(fl.swap - 0.1, 1f), K(fl.swap + 0.15, 1f), K(fl.swap + 1.1, 0f) };
            foreach (double c in new[] { fl.map1, fl.flight2, fl.map2 })
                alpha.AddRange(new[] { K(c - 0.7, 0f), K(c - 0.1, 1f), K(c + 0.15, 1f), K(c + 0.85, 0f) });
            Curve(fade, "", typeof(ScreenFade), "alpha", alpha.OrderBy(k => k.time).ToArray());
            foreach (string ch in new[] { "r", "g", "b" })   // white while inside the cloud, black for every other cut
                Curve(fade, "", typeof(ScreenFade), "color." + ch, K(0, 0f), K(fl.swap - 0.65, 0f), K(fl.swap - 0.6, 1f), K(fl.swap + 1.15, 1f), K(fl.swap + 1.2, 0f));
            Play(fadeTrack, fade, 0);

            // ---- the map's satellite leaves its upper stage at the same moment (off-screen), so the map shows it already released
            var dep = NewClip("Satellite Deploy");
            Vector3 s0 = satellite.localPosition;
            PositionCurves(dep, AnimationUtility.CalculateTransformPath(satellite, gto.parent), P(0, s0), P(2.8, s0 + new Vector3(0f, 0f, 1.1f)));
            var stageT = FindUnder(slot, "Upper Stage (Deploy)");
            Vector3 u0 = stageT.localPosition;
            PositionCurves(dep, AnimationUtility.CalculateTransformPath(stageT, gto.parent), P(0, u0), P(2.8, u0 + new Vector3(0f, 0f, -0.35f)));
            Play(deployTrack, dep, fl.sepStart);

            // ---- back on the map it coasts up the transfer orbit to the apogee and then circles on the final orbit (OrbitMotion)
            var gtoPath = gto.GetComponent<OrbitPath>();
            var finalT = FindT("Final Operational Orbit");
            var finalPath = finalT != null ? finalT.GetComponent<OrbitPath>() : null;
            if (gtoPath == null || finalPath == null) warnings.Add("orbit paths not found: the map's satellite stays where it was released");
            else
            {
                var motion = satellite.GetComponent<OrbitMotion>();   // kept on the space set between builds
                if (motion == null) motion = Undo.AddComponent<OrbitMotion>(satellite.gameObject);
                Undo.RecordObject(motion, UndoName);
                motion.director = dir;
                motion.transfer = gtoPath;
                motion.finalOrbit = finalPath;
                motion.startTime = fl.map2 + 0.9;                     // once the cut to the map has faded in
                motion.transferSeconds = (float)Math.Max(6.0, V("S5_06_ReachFinalOrbit")[1] - motion.startTime);
                motion.orbitPeriod = 40f;
            }
        }

        // A stage leaving the rocket: pushed out sideways, falling behind along the rocket's axis ever faster, tumbling about the
        // first part's pivot, and shrunk away once it is far behind. Offsets are in the model's units (X sideways, Y depth, Z along).
        static void Jettison(AnimationClip clip, Transform[] parts, double t0, double dur, Vector3 push, float fallBehind, Vector3 tumbleAxis, float tumbleDeg)
        {
            parts = parts.Where(p => p != null).ToArray();
            if (parts.Length == 0) { warnings.Add("jettison at " + t0.ToString("0.0") + " s: parts not found"); return; }
            Vector3 c0 = parts[0].localPosition;
            Func<double, float> u = tt => Mathf.Clamp01((float)((tt - t0) / dur));
            Func<double, Quaternion> spin = tt => Quaternion.AngleAxis(tumbleDeg * u(tt), tumbleAxis);
            Func<double, Vector3> move = tt => push * (1f - (1f - u(tt)) * (1f - u(tt))) + Vector3.back * (fallBehind * u(tt) * u(tt));
            foreach (var part in parts)
            {
                string path = AnimationUtility.CalculateTransformPath(part, padHost);
                Vector3 p0 = part.localPosition, s0 = part.localScale;
                Quaternion q0 = part.localRotation;
                var pp = new CamPath();
                pp.Key(0, p0);
                pp.Key(t0, p0);
                pp.Sample(t0, t0 + dur, 0.2, tt => c0 + move(tt) + spin(tt) * (p0 - c0));
                pp.Write(clip, path);
                SampleRotation(clip, path, t0, t0 + dur, 0.2, q0, tt => spin(tt) * q0);
                ScaleCurves(clip, path, P(0, s0), P(t0 + dur * 0.8, s0), P(t0 + dur, Vector3.zero));
            }
        }

        // Parts that swell a little while the voice-over introduces them (Timeline times a..b). Keyed into the flight clip
        // by ApplyEmphasis once its other curves are written; the jettisons come long after, so the rest pose is kept.
        class Emphasis
        {
            public double a, b;
            public string[] names;
        }

        static readonly List<Emphasis> emphases = new List<Emphasis>();

        static void Emphasize(double a, double b, params string[] names)
        {
            b = Math.Max(b, a + 2 * EmphasisRamp + 0.2);   // room to swell, hold a moment and settle
            emphases.Add(new Emphasis { a = a, b = b, names = names });
        }

        // Each group scales about the centre of its meshes: every part keeps its own parent, so the centre is taken in that
        // parent's space (position c + k (p - c), scale k s); a part inside another part of the group follows its parent.
        static void ApplyEmphasis(AnimationClip clip)
        {
            foreach (var e in emphases)
            {
                var parts = e.names.Select(n => FindPart(modelRoot, n)).Where(p => p != null).ToArray();
                parts = parts.Where(p => !parts.Any(q => q != p && p.IsChildOf(q))).ToArray();
                var withMesh = parts.Where(p => p.GetComponentsInChildren<Renderer>().Length > 0).ToArray();
                if (withMesh.Length == 0) { warnings.Add("emphasis: " + string.Join(", ", e.names) + " not found"); continue; }
                var bounds = WorldBounds(withMesh[0]);
                foreach (var p in withMesh) bounds.Encapsulate(WorldBounds(p));
                foreach (var p in parts)
                {
                    string path = AnimationUtility.CalculateTransformPath(p, padHost);
                    Vector3 c = p.parent.InverseTransformPoint(bounds.center), p0 = p.localPosition, s0 = p.localScale;
                    SwellKeys(clip, path, "m_LocalPosition", p0, c + EmphasisScale * (p0 - c), e.a, e.b);
                    SwellKeys(clip, path, "m_LocalScale", s0, s0 * EmphasisScale, e.a, e.b);
                }
            }
        }

        // Adds rest -> peak -> rest keys (eased) between a and b to a Transform curve, which holds 'rest' there.
        static void SwellKeys(AnimationClip clip, string path, string property, Vector3 rest, Vector3 peak, double a, double b)
        {
            for (int i = 0; i < 3; i++)
            {
                var binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), property + "." + "xyz"[i]);
                var curve = AnimationUtility.GetEditorCurve(clip, binding) ?? new AnimationCurve(K(0, rest[i]));
                foreach (var k in new[] { K(a, rest[i]), K(a + EmphasisRamp, peak[i]), K(b - EmphasisRamp, peak[i]), K(b, rest[i]) })
                    curve.AddKey(k);
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }
        }

        // Rotation keys sampled from f between t0 and t1 (rest before), as quaternion components with tangents that follow the motion.
        static void SampleRotation(AnimationClip clip, string path, double t0, double t1, double step, Quaternion rest, Func<double, Quaternion> f)
        {
            var times = new List<double> { 0, t0 };
            var values = new List<Quaternion> { rest, rest };
            int n = Mathf.Max(1, Mathf.CeilToInt((float)((t1 - t0) / step)));
            for (int i = 1; i <= n; i++)
            {
                double x = t0 + (t1 - t0) * i / n;
                var q = f(x);
                if (Quaternion.Dot(values[values.Count - 1], q) < 0f) q = new Quaternion(-q.x, -q.y, -q.z, -q.w);   // stay in one hemisphere
                times.Add(x);
                values.Add(q);
            }
            for (int c = 0; c < 4; c++)
            {
                var keys = new Keyframe[times.Count];
                for (int i = 0; i < keys.Length; i++)
                {
                    float slope = i >= 2 && i < keys.Length - 1 ? (float)((values[i + 1][c] - values[i - 1][c]) / (times[i + 1] - times[i - 1])) : 0f;
                    keys[i] = new Keyframe((float)times[i], values[i][c], slope, slope);
                }
                Curve(clip, path, typeof(Transform), "m_LocalRotation." + "xyzw"[c], keys);
            }
        }

        // =====================================================================================
        static void WireFlow()
        {
            flow.space = switcher;
            flow.quiz = quiz;
            flow.endMenu = G("End Menu");
            flow.feedbackSource = feedbackSource.GetComponent<AudioSource>();
            flow.wrongAnswerClip = AssetDatabase.LoadAssetAtPath<AudioClip>(VoFolder + "/S6_13_WrongAnswer_TryAgain.mp3");
            flow.gateButtons = new[]
            {
                new MissionFlow.GateButton { id = "Explore", button = G("Btn Explore").GetComponent<Button>() },
                new MissionFlow.GateButton { id = "Launch", button = G("Btn Launch").GetComponent<Button>() },
                new MissionFlow.GateButton { id = "Deploy", button = G("Btn Deploy").GetComponent<Button>() }
            };
            foreach (var g in flow.gateButtons)
                UnityEventTools.AddStringPersistentListener(g.button.onClick, flow.ButtonClicked, g.id);
            for (int i = 0; i < 3; i++)
                UnityEventTools.AddIntPersistentListener(quiz.optionButtons[i].onClick, flow.QuizOption, i);
            EditorUtility.SetDirty(flow);

            var endMenu = G("End Menu").GetComponent<MissionEndMenu>();
            clickSound.buttons = flow.gateButtons.Select(g => g.button).Concat(quiz.optionButtons)
                .Concat(new[] { endMenu.restartButton, endMenu.exitButton }).Where(b => b != null).ToArray();
            EditorUtility.SetDirty(clickSound);
        }

        // The parts of the space environment that the Timeline switches on and off.
        static void RegisterSpaceObjects()
        {
            var finalOrbit = FindT("Final Operational Orbit");
            var arrow = FindT("Transfer Arrow");
            Reg("Map", earthSystem.gameObject);          // the whole orbit map, shown only for the orbit explanations and the quiz
            Reg("MapCam", spaceCamHost.gameObject);      // while on, the learner's camera follows the map's anchor
            Reg("Backdrop", backdrop.gameObject);        // Earth below the rocket during the flight in space
            Reg("Flight Path", flightPath.gameObject);
            Reg("GTO", gto.gameObject);
            if (finalOrbit != null) Reg("Final Orbit", finalOrbit.gameObject);
            if (arrow != null) Reg("Transfer Arrow", arrow.gameObject);
            Reg("Satellite", satellite.gameObject);
            var fps = new[] { fp1, fp2, fp3 };
            for (int i = 0; i < 3; i++)
            {
                var marker = FindUnder(fps[i], "Marker");
                if (marker != null) Reg("FP Marker " + (i + 1), marker.gameObject);
            }
        }

        // Everything the Timeline switches on starts off. Animator hosts, the fade and the gate buttons stay on.
        static void SetInitialStates()
        {
            string[] stayOn = { "Fade", "Btn Explore", "Btn Launch", "Btn Deploy" };
            foreach (var kv in O)
                if (!stayOn.Contains(kv.Key)) SetInactive(kv.Value);
        }

        static void SaveAssets()
        {
            EditorUtility.SetDirty(tl);
            var paths = new List<string> { TimelinePath };
            paths.AddRange(AssetDatabase.FindAssets("t:Material t:AnimationClip t:Texture2D", new[] { Folder }).Select(AssetDatabase.GUIDToAssetPath));
            AssetDatabase.ForceReserializeAssets(paths.Distinct());
            report.AppendLine("Assets saved: " + paths.Distinct().Count());
        }
    }
}
