using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GslvSpace.EditorTools
{
    /// <summary>
    /// Menu GSLV > Build Space Environment. Builds the storyboard's space setting for Scenes 4-6
    /// into the open scene: Earth with clouds and an atmosphere glow, the transfer (GTO) and final
    /// orbits, the ascent path from Sriharikota, a transfer arrow, a satellite, a small rocket,
    /// a star sky and a sun, plus a SpaceEnvironmentSwitcher that swaps it with the launch pad.
    /// The set is parked above the launch pad for editing; ShowSpace() brings it to the learner.
    /// Everything it writes stays inside Assets/Assets A. Generated assets are created once and
    /// reused on later runs, so tuning made to them is kept (delete one to regenerate it).
    /// </summary>
    public static class SpaceEnvironmentBuilder
    {
        const string Folder = "Assets/Assets A/Space";
        const string RootName = "SPACE ENVIRONMENT";
        const string UndoName = "Build Space Environment";
        const string Title = "Space Environment";
        const string LaunchpadPrefabFolder = "Assets/Assets A/FBX/Launch pad/";
        const string ImportMarker = "gslv-space-import";

        const string EarthAlbedoPath = Folder + "/Textures/Earth_Diff.jpg";
        const string EarthNormalPath = Folder + "/Textures/Earth_NM.jpg";
        const string CloudsPath = Folder + "/Textures/Cloudsphere1.png";
        const string StarsPath = Folder + "/Skybox/2k_stars_milky_way.jpg";
        const string ProbeModelPath = Folder + "/Models/Probe.FBX";
        const string ProbeTexturePath = Folder + "/Models/Probe.png";
        const string RocketModelPath = "Assets/Assets A/FBX/Launch pad/rocket env/SLV_3_Textured.fbx";

        // Layout in metres, relative to the learner's eye looking along +Z.
        const float ParkHeight = 5000f;                  // editing position above the launch pad
        const float EarthRadius = 2.2f;
        const float EarthDistance = 11f;
        const float PerigeeRadius = EarthRadius * 1.15f;
        const float ApogeeRadius = EarthRadius * 3.2f;   // also the radius of the final circular orbit
        const float OrbitTilt = 40f;                     // orbit plane leans towards the learner
        const float OrbitRoll = 25f;                     // and rises to the right, so the ascent climbs on screen
        const float PerigeeBearing = 35f;                // periapsis sits to the right of Earth
        const float LaunchLatitude = 13.72f;             // Satish Dhawan Space Centre, Sriharikota
        const float LaunchLongitude = 80.23f;
        const float ViewLatitude = 30f;                  // latitude turned to face the learner
        const float DeployAnomaly = 55f;                 // just after injection at perigee, in open sky
        const float PathWidth = 0.09f;
        const float RocketHeight = 0.55f;
        const float SatelliteSize = 1.6f;

        static readonly Color TransferColor = new Color(0.30f, 0.85f, 1.00f);
        static readonly Color FinalColor = new Color(0.45f, 1.00f, 0.55f);
        static readonly Color AscentStart = new Color(1.00f, 0.55f, 0.15f);
        static readonly Color AscentEnd = new Color(1.00f, 0.90f, 0.35f);
        static readonly Color SpaceAmbient = new Color(0.07f, 0.08f, 0.11f);

        static Quaternion sphereLayout;   // see UnitySphere()

        class Art
        {
            public Mesh sphere;
            public Material earth, clouds, halo, line, marker, satellite, skybox;
            public Cubemap stars;
        }

        [MenuItem("GSLV/Build Space Environment")]
        static void Build()
        {
            Run(true);
        }

        /// <summary>Builds or rebuilds the space set. askBeforeRebuild=false is used by the one-time trigger.</summary>
        internal static void Run(bool askBeforeRebuild)
        {
            Debug.Log("[GSLV] Build Space Environment started.");
            WriteStatus("started");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Stop("Stop Play mode first.");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.path.StartsWith("Assets/Assets A/"))
            {
                Stop("Open your scene from Assets/Assets A first (for example Assets A/Scene/Scene 1).\n\nActive scene: " +
                    (string.IsNullOrEmpty(scene.path) ? "unsaved scene" : scene.path));
                return;
            }

            string missing = MissingArt();
            if (missing.Length > 0)
            {
                Stop("These files are missing:\n" + missing);
                return;
            }

            SpaceEnvironmentSwitcher existing = FindExisting(scene);
            if (existing != null && askBeforeRebuild && !EditorUtility.DisplayDialog(Title,
                    "Rebuild the space set?\n\nEverything inside " + existing.name + " is recreated, so links from other objects to things " +
                    "inside it (anchors, satellite, orbits) must be made again. The switcher component and its settings are kept.",
                    "Rebuild", "Cancel"))
            {
                Debug.Log("[GSLV] Rebuild cancelled.");
                return;
            }

            bool wasDirty = scene.isDirty;
            try
            {
                EditorUtility.DisplayProgressBar(Title, "Preparing textures", 0.1f);
                var report = new StringBuilder();
                ConfigureImports(report);
                EditorUtility.DisplayProgressBar(Title, "Preparing materials", 0.4f);
                Art art = CreateArt();

                EditorUtility.DisplayProgressBar(Title, "Building the scene", 0.6f);
                Undo.IncrementCurrentGroup();
                int group = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName(UndoName);
                SpaceEnvironmentSwitcher switcher;
                try
                {
                    switcher = BuildEnvironment(scene, art, existing, report);
                }
                catch
                {
                    Undo.RevertAllDownToGroup(group);
                    throw;
                }
                Undo.CollapseUndoOperations(group);

                EditorUtility.DisplayProgressBar(Title, "Rendering previews", 0.9f);
                string[] previews = TryRenderPreviews(switcher, report);

                EditorSceneManager.MarkSceneDirty(scene);
                string saveNote;
                if (wasDirty)
                    saveNote = "The scene already had other unsaved changes, so it was not saved automatically. Save it (Ctrl+S) when ready.";
                else if (EditorSceneManager.SaveScene(scene))
                    saveNote = "Scene saved.";
                else
                    saveNote = "Saving the scene failed. Save it manually (Ctrl+S).";

                Selection.activeGameObject = switcher.gameObject;
                if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();

                report.Insert(0, "[GSLV] Space environment " + (existing != null ? "rebuilt" : "built") + " in " + scene.path + ". " + saveNote + "\n");
                if (previews.Length > 0) report.AppendLine("Preview images: " + string.Join(" | ", previews));
                report.AppendLine("Call SpaceEnvironmentSwitcher.ShowSpace() at the launch-to-space transition and ShowLaunchpad() to return.");
                Debug.Log(report.ToString(), switcher);
                WriteStatus("done\n" + report);
            }
            catch (Exception e)
            {
                WriteStatus("failed\n" + e);
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static void Stop(string reason)
        {
            Debug.LogWarning("[GSLV] Build stopped: " + reason);
            WriteStatus("stopped\n" + reason);
            EditorUtility.DisplayDialog(Title, reason, "OK");
        }

        // Status file in the system temp folder, readable even when several editors share Editor.log.
        static void WriteStatus(string text)
        {
            try
            {
                File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "GSLV_SpaceBuild_Status.txt"),
                    DateTime.Now.ToString("HH:mm:ss") + " " + text);
            }
            catch (Exception)
            {
                // Status reporting is best effort only.
            }
        }

        static string MissingArt()
        {
            var sb = new StringBuilder();
            foreach (string path in new[] { EarthAlbedoPath, EarthNormalPath, CloudsPath, StarsPath, ProbeModelPath, ProbeTexturePath })
                if (AssetDatabase.LoadAssetAtPath<Object>(path) == null) sb.AppendLine(path);
            if (Shader.Find("Custom/Glow") == null) sb.AppendLine("Shader 'Custom/Glow' (Assets A/Shaders/Glow_Rim.shader)");
            return sb.ToString();
        }

        static SpaceEnvironmentSwitcher FindExisting(Scene scene)
        {
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                var sw = go.GetComponentInChildren<SpaceEnvironmentSwitcher>(true);
                if (sw != null) return sw;
            }
            return null;
        }

        // ---------------------------------------------------------------- imports

        // Applied once per texture (marked in the importer's userData), so later tuning is kept.
        static void ConfigureImports(StringBuilder report)
        {
            ConfigureTexture(EarthAlbedoPath, ti =>
            {
                ti.textureType = TextureImporterType.Default;
                ti.textureShape = TextureImporterShape.Texture2D;
                ti.sRGBTexture = true;
                ti.wrapModeU = TextureWrapMode.Repeat;
                ti.wrapModeV = TextureWrapMode.Clamp;
                ti.maxTextureSize = 4096;
            }, 2048, TextureImporterFormat.ETC2_RGB4, report);

            ConfigureTexture(EarthNormalPath, ti =>
            {
                ti.textureType = TextureImporterType.NormalMap;
                ti.textureShape = TextureImporterShape.Texture2D;
                ti.wrapModeU = TextureWrapMode.Repeat;
                ti.wrapModeV = TextureWrapMode.Clamp;
                ti.maxTextureSize = 2048;
            }, 1024, TextureImporterFormat.ETC2_RGB4, report);

            ConfigureTexture(CloudsPath, ti =>
            {
                ti.textureType = TextureImporterType.Default;
                ti.textureShape = TextureImporterShape.Texture2D;
                ti.sRGBTexture = true;
                ti.alphaSource = TextureImporterAlphaSource.FromInput;
                ti.alphaIsTransparency = true;
                ti.wrapModeU = TextureWrapMode.Repeat;
                ti.wrapModeV = TextureWrapMode.Clamp;
                ti.maxTextureSize = 4096;
            }, 2048, TextureImporterFormat.ETC2_RGBA8, report);

            // Stars suffer from block compression, and the cube is small, so keep it uncompressed on device.
            ConfigureTexture(StarsPath, ti =>
            {
                ti.textureShape = TextureImporterShape.TextureCube;
                ti.maxTextureSize = 2048;
            }, 2048, TextureImporterFormat.RGB24, report);

            ConfigureTexture(ProbeTexturePath, ti =>
            {
                ti.textureType = TextureImporterType.Default;
                ti.maxTextureSize = 2048;
            }, 1024, TextureImporterFormat.ETC2_RGB4, report);
        }

        static void ConfigureTexture(string path, Action<TextureImporter> setup, int androidMax, TextureImporterFormat androidFormat, StringBuilder report)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null || ti.userData == ImportMarker) return;
            setup(ti);
            ti.mipmapEnabled = true;
            ti.textureCompression = TextureImporterCompression.Compressed;
            TextureImporterPlatformSettings android = ti.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = androidMax;
            android.format = androidFormat;
            ti.SetPlatformTextureSettings(android);
            ti.userData = ImportMarker;
            ti.SaveAndReimport();
            report.AppendLine("Import settings applied: " + path);
        }

        // ---------------------------------------------------------------- generated art

        static Art CreateArt()
        {
            EnsureFolder(Folder + "/Materials");
            EnsureFolder(Folder + "/Textures");
            EnsureFolder(Folder + "/Skybox");

            Texture2D pathGlow = LoadOrMakeTexture(Folder + "/Textures/PathGlow.png", 8, 64, PathGlowPixel);
            Texture2D haloTex = LoadOrMakeTexture(Folder + "/Textures/AtmosphereGlow.png", 256, 256, HaloPixel);
            Texture2D dotTex = LoadOrMakeTexture(Folder + "/Textures/MarkerDot.png", 64, 64, DotPixel);

            var art = new Art
            {
                sphere = UnitySphere(out sphereLayout),
                stars = AssetDatabase.LoadAssetAtPath<Cubemap>(StarsPath)
            };
            art.earth = LoadOrMakeMaterial(Folder + "/Materials/Earth.mat", "Custom/Glow", m =>
            {
                m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(EarthAlbedoPath));
                m.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(EarthNormalPath));
                m.SetColor("_Color", Color.white);
                m.SetColor("_RimColor", new Color(0.22f, 0.50f, 1.00f));
                m.SetFloat("_RimPower", 3f);
                m.SetFloat("_Glossiness", 0.18f);
                m.SetFloat("_Metallic", 0f);
            });
            art.clouds = LoadOrMakeMaterial(Folder + "/Materials/Clouds.mat", "Legacy Shaders/Transparent/Diffuse", m =>
            {
                m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(CloudsPath));
                m.SetColor("_Color", new Color(1f, 1f, 1f, 0.85f));
                m.renderQueue = (int)RenderQueue.Transparent - 1; // under the glowing paths and labels
            });
            art.halo = LoadOrMakeMaterial(Folder + "/Materials/Atmosphere Glow.mat", "Mobile/Particles/Additive", m => m.SetTexture("_MainTex", haloTex));
            art.line = LoadOrMakeMaterial(Folder + "/Materials/Path Glow.mat", "Mobile/Particles/Additive", m => m.SetTexture("_MainTex", pathGlow));
            art.marker = LoadOrMakeMaterial(Folder + "/Materials/Path Marker.mat", "Mobile/Particles/Additive", m => m.SetTexture("_MainTex", dotTex));
            art.satellite = LoadOrMakeMaterial(Folder + "/Materials/Satellite.mat", "Standard", m =>
            {
                m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(ProbeTexturePath));
                m.SetFloat("_Glossiness", 0.45f);
                m.SetFloat("_Metallic", 0.1f);
            });
            art.skybox = LoadOrMakeMaterial(Folder + "/Skybox/Space Skybox.mat", "Skybox/Cubemap", m =>
            {
                m.SetTexture("_Tex", art.stars);
                m.SetColor("_Tint", new Color(0.5f, 0.5f, 0.5f, 0.5f));
                m.SetFloat("_Exposure", 1.3f);
                m.SetFloat("_Rotation", 0f);
            });
            return art;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(path.LastIndexOf('/') + 1));
        }

        static Material LoadOrMakeMaterial(string path, string shaderName, Action<Material> setup)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            Shader shader = Shader.Find(shaderName);
            if (shader == null) throw new InvalidOperationException("[GSLV] Shader not found: " + shaderName);
            var mat = new Material(shader);
            setup(mat);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static Texture2D LoadOrMakeTexture(string path, int width, int height, Func<float, float, Color> pixel)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = pixel((x + 0.5f) / width, (y + 0.5f) / height);
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Default;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static float Smooth(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        // Soft glowing tube across the line's width (V).
        static Color PathGlowPixel(float u, float v)
        {
            float d = (v - 0.5f) * 2f;
            float a = 0.85f * Mathf.Exp(-(d / 0.22f) * (d / 0.22f)) + 0.35f * Mathf.Exp(-(d / 0.6f) * (d / 0.6f));
            return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
        }

        // Blue ring that peaks at Earth's silhouette (80% of the quad's half-size) and fades outwards.
        static Color HaloPixel(float u, float v)
        {
            float r = new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f;
            const float peak = 0.8f;
            float i = r < peak ? Mathf.Exp(-((peak - r) / 0.02f) * ((peak - r) / 0.02f)) : Mathf.Exp(-(r - peak) / 0.06f);
            i *= 1f - Smooth(0.9f, 1f, r);
            return new Color(0.35f, 0.65f, 1f, 0.9f * i);
        }

        // Warm round glow for the marked flight positions.
        static Color DotPixel(float u, float v)
        {
            float r = new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f;
            float a = Mathf.Exp(-(r / 0.35f) * (r / 0.35f)) + 0.25f * Mathf.Exp(-(r / 0.8f) * (r / 0.8f));
            a *= 1f - Smooth(0.85f, 1f, r);
            return new Color(1f, 0.85f, 0.4f, Mathf.Clamp01(a));
        }

        /// <summary>
        /// Unity's built-in sphere (radius 0.5) for Earth, its clouds and the sky. The builders place Earth in a "geo" layout:
        /// north +Y, longitude 0 facing -Z, east towards +X (GeoToSphere). 'layout' turns that layout into the built-in sphere's
        /// own texture layout, read from its UVs, so a transform at rotation * Inverse(layout) shows the Earth map the geo way round.
        /// </summary>
        public static Mesh UnitySphere(out Quaternion layout)
        {
            var mesh = Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx");
            Vector3[] v = mesh.vertices;
            Vector2[] uv = mesh.uv;
            layout = Quaternion.identity;
            if (v.Length == 0 || uv.Length != v.Length)
            {
                Debug.LogWarning("GSLV: Unity's sphere mesh could not be read; Earth may be turned the wrong way.");
                return mesh;
            }
            Func<Vector2, Vector3> at = target =>
            {
                int best = 0;
                for (int i = 1; i < v.Length; i++)
                    if ((uv[i] - target).sqrMagnitude < (uv[best] - target).sqrMagnitude) best = i;
                return v[best].normalized;
            };
            Vector3 north = at(new Vector2(0.5f, 1f)), lon0 = at(new Vector2(0.5f, 0.5f));
            layout = Quaternion.LookRotation(-lon0, north);
            if (Vector3.Dot(layout * Vector3.right, at(new Vector2(0.75f, 0.5f))) < 0f)
                Debug.LogWarning("GSLV: Unity's sphere maps the Earth texture mirrored.");
            return mesh;
        }

        /// <summary>Direction on the unit sphere mesh for a latitude/longitude in degrees.</summary>
        static Vector3 GeoToSphere(float latitude, float longitude)
        {
            float lat = latitude * Mathf.Deg2Rad, lon = longitude * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(lat) * Mathf.Sin(lon), Mathf.Sin(lat), -Mathf.Cos(lat) * Mathf.Cos(lon));
        }

        // ---------------------------------------------------------------- scene

        static SpaceEnvironmentSwitcher BuildEnvironment(Scene scene, Art art, SpaceEnvironmentSwitcher existing, StringBuilder report)
        {
            List<Transform> rigs = FindRigs(scene);
            GameObject rootObject;
            SpaceEnvironmentSwitcher switcher;
            Transform viewer = null;
            List<GameObject> launchpad = null;

            if (existing != null)
            {
                switcher = existing;
                rootObject = existing.gameObject;
                Undo.RecordObject(switcher, UndoName);
                var children = new List<GameObject>();
                foreach (Transform child in rootObject.transform) children.Add(child.gameObject);
                foreach (GameObject child in children) Undo.DestroyObjectImmediate(child);
            }
            else
            {
                launchpad = FindLaunchpadRoots(scene);
                viewer = PickViewer(rigs, launchpad);
                rootObject = new GameObject(RootName);
                Undo.RegisterCreatedObjectUndo(rootObject, UndoName);
                Vector3 look = viewer != null ? Vector3.ProjectOnPlane(viewer.forward, Vector3.up) : Vector3.forward;
                Vector3 basePosition = viewer != null ? viewer.position : Vector3.zero;
                rootObject.transform.SetPositionAndRotation(basePosition + Vector3.up * ParkHeight,
                    Quaternion.LookRotation(look.sqrMagnitude > 0.0001f ? look.normalized : Vector3.forward, Vector3.up));
                switcher = rootObject.AddComponent<SpaceEnvironmentSwitcher>();
            }

            Transform root = rootObject.transform;
            Vector3 eye = root.position;
            Vector3 up = root.up;
            Transform content = Child("Space Content", root);
            // On a rebuild the root already exists, so the new content needs its own undo record.
            if (existing != null) Undo.RegisterCreatedObjectUndo(content.gameObject, UndoName);

            Transform sunTransform = Child("Space Sun", content);
            sunTransform.localRotation = Quaternion.LookRotation(new Vector3(0.72f, -0.34f, 0.60f).normalized);
            var sun = sunTransform.gameObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.90f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.None;
            sun.renderMode = LightRenderMode.ForcePixel;
            sun.lightmapBakeType = LightmapBakeType.Realtime;
            sun.enabled = false; // switched on by ShowSpace(), so it never lights the launch pad

            // Earth, turned so Sriharikota faces the learner a little below centre.
            Transform earthSystem = Child("Earth System", content);
            earthSystem.localPosition = new Vector3(0f, 0f, EarthDistance);
            Quaternion earthTurn = Quaternion.AngleAxis(-ViewLatitude, Vector3.right) * Quaternion.AngleAxis(LaunchLongitude, Vector3.up);
            Transform earth = MeshChild("Earth", earthSystem, art.sphere, art.earth);
            earth.localRotation = earthTurn * Quaternion.Inverse(sphereLayout);
            earth.localScale = Vector3.one * (EarthRadius * 2f);   // Unity's sphere has a radius of 0.5
            Transform clouds = MeshChild("Clouds", earthSystem, art.sphere, art.clouds);
            clouds.localRotation = earthTurn * Quaternion.Inverse(sphereLayout);
            clouds.localScale = Vector3.one * (EarthRadius * 1.012f * 2f);
            var cloudSpin = clouds.gameObject.AddComponent<SlowSpin>();
            cloudSpin.speed = -0.6f; // eastward, like Earth's rotation
            cloudSpin.localAxis = sphereLayout * Vector3.up;   // about Earth's axis
            Transform glow = Quad("Atmosphere Glow", earthSystem, art.halo);
            float silhouette = EarthRadius * EarthDistance / Mathf.Sqrt(EarthDistance * EarthDistance - EarthRadius * EarthRadius);
            glow.localScale = Vector3.one * (2f * silhouette / 0.8f);

            // Both orbits share one plane with Earth at the focus.
            Transform plane = Child("Orbit Plane", earthSystem);
            plane.localRotation = Quaternion.AngleAxis(OrbitRoll, Vector3.forward)
                * Quaternion.AngleAxis(-OrbitTilt, Vector3.right)
                * Quaternion.AngleAxis(PerigeeBearing, Vector3.up);
            OrbitPath transfer = Orbit("Transfer Orbit (GTO)", plane, PerigeeRadius, ApogeeRadius, art.line, TransferColor);
            OrbitPath final = Orbit("Final Operational Orbit", plane, ApogeeRadius, ApogeeRadius, art.line, FinalColor);
            Vector3 centre = earthSystem.position;

            Vector3 gtoPoint = transfer.GetWorldPoint(330f);
            Label("GTO\n<size=55%>TRANSFER ORBIT</size>", transfer.transform,
                gtoPoint + (gtoPoint - centre).normalized * 1.2f - up * 0.25f, eye, up, TransferColor, 3.6f);
            Vector3 finalPoint = final.GetWorldPoint(90f);
            Label("FINAL\n<size=80%>OPERATIONAL ORBIT</size>", final.transform,
                finalPoint + (finalPoint - centre).normalized * 0.5f + up * 0.8f, eye, up, FinalColor, 4.4f);

            // Arrow from the transfer orbit out to the final orbit, past apogee where the gap between them is clear.
            Vector3 a0 = transfer.GetWorldPoint(215f), a3 = final.GetWorldPoint(235f);
            Vector3 a1 = a0 + transfer.GetWorldTangent(215f) * 0.4f + (a0 - centre).normalized * 0.8f;
            Vector3 a2 = a3 - final.GetWorldTangent(235f) * 0.4f + (a3 - centre).normalized * 0.2f;
            BezierPath arrow = Bezier("Transfer Arrow", plane, a0, a1, a2, a3, art.line, Color.white, FinalColor, PathWidth);
            arrow.arrowHead = true;
            arrow.Rebuild();

            // Ascent: leaves the launch site straight up, then joins the transfer orbit tangentially at perigee.
            Vector3 p0 = earth.TransformPoint(sphereLayout * GeoToSphere(LaunchLatitude, LaunchLongitude) * (0.5f * 1.004f));
            Vector3 p3 = transfer.GetWorldPoint(0f);
            Vector3 p1 = p0 + (p0 - centre).normalized * ((PerigeeRadius - EarthRadius) * 1.1f);
            Vector3 p2 = p3 - transfer.GetWorldTangent(0f) * 0.8f;
            BezierPath ascent = Bezier("Flight Path", earthSystem, p0, p1, p2, p3, art.line, AscentStart, AscentEnd, PathWidth);
            Child("Launch Site (Sriharikota)", ascent.transform).position = p0;
            var positions = new List<Transform>();
            float[] stops = { 0.25f, 0.55f, 0.85f };
            for (int i = 0; i < stops.Length; i++)
            {
                // Point the rocket along the path as the learner sees it, so it reads side-on rather than nose-on.
                Vector3 point = ascent.GetPoint(stops[i]);
                Vector3 view = (point - eye).normalized;
                Vector3 along = Vector3.ProjectOnPlane(ascent.GetTangent(stops[i]), view);
                if (along.sqrMagnitude < 0.0001f) along = up;
                Transform stop = Child("Flight Position " + (i + 1), ascent.transform);
                stop.SetPositionAndRotation(point, Quaternion.LookRotation(-view, along.normalized));
                Transform dot = Quad("Marker", stop, art.marker);
                dot.rotation = Quaternion.LookRotation(dot.position - eye, up);
                dot.localScale = Vector3.one * 0.28f;
                positions.Add(stop);
            }

            Transform rocket = PlaceModel(RocketModelPath, "Small Rocket", positions[0], null);
            if (rocket != null)
            {
                FitSize(rocket, RocketHeight, true);
                rocket.localPosition = Vector3.zero;
                rocket.localRotation = Quaternion.identity;
            }

            // Deployment just after injection, where the transfer orbit runs through open sky.
            Transform deploy = Child("Deploy Point", transfer.transform);
            deploy.SetPositionAndRotation(transfer.GetWorldPoint(DeployAnomaly), Quaternion.LookRotation(transfer.GetWorldTangent(DeployAnomaly), plane.up));
            Transform release = Child("Satellite Release Point", transfer.transform);
            release.SetPositionAndRotation(transfer.GetWorldPoint(DeployAnomaly + 8f), Quaternion.LookRotation(transfer.GetWorldTangent(DeployAnomaly + 8f), plane.up));
            Child("Upper Stage Slot", deploy).localPosition = new Vector3(0f, 0f, -0.9f);
            Transform satellite = PlaceModel(ProbeModelPath, "Communication Satellite", deploy, art.satellite);
            if (satellite != null)
            {
                foreach (Transform t in satellite.GetComponentsInChildren<Transform>(true))
                    if (t.name.IndexOf("fold", StringComparison.OrdinalIgnoreCase) >= 0) t.gameObject.SetActive(false);
                FitSize(satellite, SatelliteSize, false);
                satellite.localPosition = Vector3.zero;
                satellite.localRotation = Quaternion.Euler(0f, 90f, 0f); // panels along the orbit, square to the learner
            }

            if (existing == null)
            {
                switcher.launchpadObjects = launchpad.ToArray();
                switcher.launchpadSun = FindLaunchpadSun(scene, rootObject);
                switcher.viewer = viewer;
                switcher.startInSpace = false;
                switcher.spaceAmbientColor = SpaceAmbient;
            }
            switcher.spaceContent = content.gameObject;
            switcher.spaceSun = sun;
            switcher.spaceSkybox = art.skybox;
            switcher.spaceReflection = art.stars;

            report.AppendLine("Space set parked at " + root.position + " for editing; ShowSpace() moves it in front of the learner.");
            report.AppendLine("Launch-pad objects hidden in space: " + Names(switcher.launchpadObjects) +
                (switcher.launchpadObjects == null || switcher.launchpadObjects.Length == 0
                    ? " (none found under " + LaunchpadPrefabFolder + "; add them to the switcher's Launchpad Objects list)" : ""));
            report.AppendLine("Launch-pad sun: " + (switcher.launchpadSun != null ? switcher.launchpadSun.name : "none"));
            int activeRigs = rigs.FindAll(r => r.gameObject.activeInHierarchy).Count;
            if (activeRigs > 1)
                report.AppendLine("Note: " + activeRigs + " QwrCameraRigs are active (" + string.Join(", ", rigs.ConvertAll(HierarchyPath).ToArray()) +
                    "). The switcher follows the one QWR renders through, but a single rig is safer.");
            report.AppendLine("Rocket model: " + (rocket != null ? RocketModelPath : "not found, Flight Position 1 is empty"));
            return switcher;
        }

        static List<Transform> FindRigs(Scene scene)
        {
            var rigs = new List<Transform>();
            foreach (GameObject go in scene.GetRootGameObjects())
                foreach (qwr.QwrCameraRig rig in go.GetComponentsInChildren<qwr.QwrCameraRig>(true))
                    rigs.Add(rig.transform);
            return rigs;
        }

        // Prefer the active rig inside the launch-pad set-up (the authored viewpoint), then any active rig.
        static Transform PickViewer(List<Transform> rigs, List<GameObject> launchpad)
        {
            foreach (Transform rig in rigs)
                if (rig.gameObject.activeInHierarchy && launchpad.Exists(g => rig.IsChildOf(g.transform))) return rig;
            foreach (Transform rig in rigs)
                if (rig.gameObject.activeInHierarchy) return rig;
            return rigs.Count > 0 ? rigs[0] : null;
        }

        // Only the launch-pad environment prefab(s) the user built under FBX/Launch pad.
        static List<GameObject> FindLaunchpadRoots(Scene scene)
        {
            var result = new List<GameObject>();
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (!go.activeSelf || !PrefabUtility.IsPartOfPrefabInstance(go)) continue;
                string source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                if (!string.IsNullOrEmpty(source) && source.StartsWith(LaunchpadPrefabFolder)) result.Add(go);
            }
            return result;
        }

        static Light FindLaunchpadSun(Scene scene, GameObject spaceRoot)
        {
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (go == spaceRoot) continue;
                foreach (Light light in go.GetComponentsInChildren<Light>(true))
                    if (light.type == LightType.Directional && light.enabled && light.gameObject.activeInHierarchy) return light;
            }
            return null;
        }

        static string Names(GameObject[] objects)
        {
            if (objects == null || objects.Length == 0) return "none";
            var names = new List<string>();
            foreach (GameObject go in objects)
                if (go != null) names.Add(go.name);
            return string.Join(", ", names.ToArray());
        }

        static string HierarchyPath(Transform t)
        {
            string path = t.name;
            for (Transform p = t.parent; p != null; p = p.parent) path = p.name + "/" + path;
            return path;
        }

        // ---------------------------------------------------------------- object helpers

        static Transform Child(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static void QuietRenderer(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            r.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        static Transform MeshChild(string name, Transform parent, Mesh mesh, Material material)
        {
            Transform t = Child(name, parent);
            t.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = t.gameObject.AddComponent<MeshRenderer>();
            r.sharedMaterial = material;
            QuietRenderer(r);
            return t;
        }

        static Transform Quad(string name, Transform parent, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = material;
            QuietRenderer(r);
            return go.transform;
        }

        static LineRenderer Line(GameObject go, Material material, Color start, Color end, float width)
        {
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.widthMultiplier = width;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(start, 0f), new GradientColorKey(end, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            line.colorGradient = gradient;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.numCapVertices = 2;
            QuietRenderer(line);
            return line;
        }

        static OrbitPath Orbit(string name, Transform plane, float periapsis, float apoapsis, Material material, Color color)
        {
            Transform t = Child(name, plane);
            Line(t.gameObject, material, color, color, PathWidth);
            var orbit = t.gameObject.AddComponent<OrbitPath>();
            orbit.periapsis = periapsis;
            orbit.apoapsis = apoapsis;
            orbit.segments = 192;
            orbit.Rebuild();
            return orbit;
        }

        static BezierPath Bezier(string name, Transform parent, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
            Material material, Color start, Color end, float width)
        {
            Transform t = Child(name, parent);
            Line(t.gameObject, material, start, end, width);
            var path = t.gameObject.AddComponent<BezierPath>();
            path.p0 = Child("P0", t);
            path.p1 = Child("P1", t);
            path.p2 = Child("P2", t);
            path.p3 = Child("P3", t);
            path.p0.position = p0;
            path.p1.position = p1;
            path.p2.position = p2;
            path.p3.position = p3;
            path.segments = 64;
            path.Rebuild();
            return path;
        }

        static void Label(string text, Transform parent, Vector3 position, Vector3 eye, Vector3 up, Color color, float size)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.richText = true;
            tmp.fontSize = size;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.rectTransform.sizeDelta = new Vector2(4f, 1.5f);
            go.transform.SetPositionAndRotation(position, Quaternion.LookRotation(position - eye, up));
        }

        static Transform PlaceModel(string path, string name, Transform parent, Material material)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
            go.name = name;
            go.transform.SetParent(parent, false);
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                if (material == null) continue;
                var mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = material;
                r.sharedMaterials = mats;
            }
            return go.transform;
        }

        /// <summary>Uniformly scales a model so its height (or largest side) matches the target, measured upright.</summary>
        static void FitSize(Transform model, float target, bool useHeight)
        {
            Transform parent = model.parent;
            model.SetParent(null, true);
            model.rotation = Quaternion.identity;
            Bounds? bounds = null;
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (bounds.HasValue) { Bounds b = bounds.Value; b.Encapsulate(r.bounds); bounds = b; }
                else bounds = r.bounds;
            }
            if (bounds.HasValue)
            {
                Vector3 size = bounds.Value.size;
                float current = useHeight ? size.y : Mathf.Max(size.x, Mathf.Max(size.y, size.z));
                if (current > 0.0001f) model.localScale *= target / current;
            }
            model.SetParent(parent, true);
        }

        // ---------------------------------------------------------------- preview

        static string[] TryRenderPreviews(SpaceEnvironmentSwitcher sw, StringBuilder report)
        {
            try
            {
                return RenderPreviews(sw);
            }
            catch (Exception e)
            {
                report.AppendLine("Preview images could not be rendered: " + e.Message);
                return new string[0];
            }
        }

        /// <summary>
        /// Renders the learner's view and an overview to PNGs in the system temp folder, with space
        /// lighting switched on just for the shots. Everything it changes is put back afterwards.
        /// </summary>
        static string[] RenderPreviews(SpaceEnvironmentSwitcher sw)
        {
            Transform root = sw.transform;
            Vector3 savedPosition = root.position;
            Material savedSky = RenderSettings.skybox;
            AmbientMode savedMode = RenderSettings.ambientMode;
            Color savedAmbient = RenderSettings.ambientLight;
            bool savedFog = RenderSettings.fog;
            DefaultReflectionMode savedReflectionMode = RenderSettings.defaultReflectionMode;
            Cubemap savedReflection = RenderSettings.customReflection;
            bool contentActive = sw.spaceContent.activeSelf;
            bool spaceSunOn = sw.spaceSun.enabled;
            bool padSunOn = sw.launchpadSun != null && sw.launchpadSun.enabled;

            var results = new List<string>();
            var cameraObject = new GameObject("GSLV Preview Camera") { hideFlags = HideFlags.HideAndDontSave };
            var cam = cameraObject.AddComponent<Camera>();
            var rt = new RenderTexture(1600, 900, 24);
            try
            {
                root.position = savedPosition + Vector3.up * ParkHeight; // well clear of anything else in the scene
                sw.spaceContent.SetActive(true);
                sw.spaceSun.enabled = true;
                if (sw.launchpadSun != null) sw.launchpadSun.enabled = false;
                RenderSettings.skybox = sw.spaceSkybox;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = sw.spaceAmbientColor;
                RenderSettings.fog = false;
                if (sw.spaceReflection != null)
                {
                    RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                    RenderSettings.customReflection = sw.spaceReflection;
                }

                cam.stereoTargetEye = StereoTargetEyeMask.None;
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.nearClipPlane = 0.05f;
                cam.farClipPlane = 300f;
                cam.fieldOfView = 70f;
                cam.targetTexture = rt;

                results.Add(Snap(cam, rt, root.position, root.rotation, "GSLV_SpacePreview_LearnerView.png"));
                Vector3 earth = root.TransformPoint(new Vector3(0f, 0f, EarthDistance));
                Vector3 overview = root.TransformPoint(new Vector3(-6f, 9f, -3f));
                results.Add(Snap(cam, rt, overview, Quaternion.LookRotation(earth - overview, Vector3.up), "GSLV_SpacePreview_Overview.png"));
            }
            finally
            {
                cam.targetTexture = null;
                Object.DestroyImmediate(cameraObject);
                rt.Release();
                Object.DestroyImmediate(rt);
                root.position = savedPosition;
                sw.spaceContent.SetActive(contentActive);
                sw.spaceSun.enabled = spaceSunOn;
                if (sw.launchpadSun != null) sw.launchpadSun.enabled = padSunOn;
                RenderSettings.skybox = savedSky;
                RenderSettings.ambientMode = savedMode;
                RenderSettings.ambientLight = savedAmbient;
                RenderSettings.fog = savedFog;
                RenderSettings.defaultReflectionMode = savedReflectionMode;
                RenderSettings.customReflection = savedReflection;
                DynamicGI.UpdateEnvironment();
            }
            return results.ToArray();
        }

        static string Snap(Camera cam, RenderTexture rt, Vector3 position, Quaternion rotation, string fileName)
        {
            cam.transform.SetPositionAndRotation(position, rotation);
            cam.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            string file = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
            File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            return file;
        }
    }
}
