using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GslvSpace.EditorTools
{
    /// <summary>
    /// One-time Play-mode camera test, started by the trigger file PLAYTEST_ONCE.txt next to this script.
    /// Enters Play mode, captures what the learner's camera sees on the launch pad and after
    /// SpaceEnvironmentSwitcher.ShowSpace(), checks that ShowLaunchpad() restores the scene, writes a
    /// report and images to the system temp folder, then leaves Play mode. Does nothing without the trigger.
    /// </summary>
    [InitializeOnLoad]
    static class CameraPlayTest
    {
        const string Trigger = "Assets/Assets A/Space/Editor/PLAYTEST_ONCE.txt";
        const string SessionKey = "GSLV.CameraPlayTest";
        const double TimeoutSeconds = 45;
        const float HeadsetFov = 96f; // QWR's fallback eye FOV

        static double started;
        static int step;
        static readonly StringBuilder report = new StringBuilder();
        static Material padSkybox;
        static bool padSunOn;

        static CameraPlayTest()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            if (File.Exists(Trigger)) EditorApplication.delayCall += Begin;
        }

        static void Begin()
        {
            if (!File.Exists(Trigger)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Begin;
                return;
            }
            AssetDatabase.DeleteAsset(Trigger);
            if (File.Exists(Trigger)) File.Delete(Trigger);
            StartNow();
        }

        /// <summary>Starts the test straight away (used after placing the learner).</summary>
        internal static void StartNow()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            SessionState.SetBool(SessionKey, true);
            Status("entering play mode");
            EditorApplication.isPlaying = true;
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, false);
            report.Length = 0;
            step = 0;
            started = EditorApplication.timeSinceStartup;
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            double t = EditorApplication.timeSinceStartup - started;
            try
            {
                if (!EditorApplication.isPlaying) { Finish("stopped: Play mode ended early"); return; }
                if (t > TimeoutSeconds) { Finish("stopped: timed out"); return; }
                switch (step)
                {
                    case 0:
                        if (t < 3) return; // let QWR pick its rig and settle
                        LaunchpadChecks();
                        step++;
                        break;
                    case 1:
                        if (t < 4) return;
                        Switcher().ShowSpace();
                        report.AppendLine("ShowSpace() called.");
                        step++;
                        break;
                    case 2:
                        if (t < 5.5) return;
                        SpaceChecks();
                        step++;
                        break;
                    case 3:
                        if (t < 6.5) return;
                        Switcher().ShowLaunchpad();
                        report.AppendLine("ShowLaunchpad() called.");
                        step++;
                        break;
                    case 4:
                        if (t < 8) return;
                        RestoreChecks();
                        Finish("done");
                        break;
                }
            }
            catch (Exception e)
            {
                Finish("failed\n" + e);
            }
        }

        static void Finish(string result)
        {
            EditorApplication.update -= Tick;
            Status(result + "\n" + report);
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }

        // ---------------------------------------------------------------- checks

        static void LaunchpadChecks()
        {
            var rigs = Object.FindObjectsOfType<qwr.QwrCameraRig>();
            report.AppendLine("Active QwrCameraRigs in Play mode: " + rigs.Length +
                (rigs.Length > 0 ? " (" + string.Join(", ", Array.ConvertAll(rigs, r => PathOf(r.transform))) + ")" : ""));
            qwr.QwrCameraRig rig = qwr.QwrCameraRig._instance;
            report.AppendLine("Rig QWR renders through: " + (rig != null ? PathOf(rig.transform) + " at " + rig.transform.position : "none"));
            Transform head = HeadOf(rig);
            Camera eye = EyeOf(rig);
            report.AppendLine(string.Format("Learner head: {0}, facing yaw {1:F0} deg. Eye camera: fov {2:F0}, near {3}, far {4}.",
                head.position, head.eulerAngles.y, eye != null ? eye.fieldOfView : 0f, eye != null ? eye.nearClipPlane : 0f, eye != null ? eye.farClipPlane : 0f));

            padSkybox = RenderSettings.skybox;
            var sw = Switcher();
            padSunOn = sw.launchpadSun != null && sw.launchpadSun.enabled;

            Capture(eye, head.position, Level(head.forward), "1_LaunchPad_Forward");
            GameObject rocket = GameObject.Find("SLV_3_Textured");
            if (rocket == null)
            {
                report.AppendLine("Launch-pad rocket 'SLV_3_Textured' not found.");
                return;
            }
            Bounds b = BoundsOf(rocket);
            Vector3 to = b.center - head.position;
            float bearing = Vector3.SignedAngle(Flat(head.forward), Flat(to), Vector3.up);
            report.AppendLine(string.Format("Launch-pad rocket: {0:F0} m away, {1:F0} deg {2} of where the learner faces, about {3:F1} m tall, appears {4:F1} deg tall.",
                to.magnitude, Mathf.Abs(bearing), bearing >= 0 ? "right" : "left", b.size.y, 2f * Mathf.Atan(b.size.y * 0.5f / to.magnitude) * Mathf.Rad2Deg));
            Capture(eye, head.position, Quaternion.LookRotation(to.normalized, Vector3.up), "2_LaunchPad_TowardRocket");
        }

        static void SpaceChecks()
        {
            var sw = Switcher();
            qwr.QwrCameraRig rig = qwr.QwrCameraRig._instance;
            report.AppendLine(string.Format("In space: InSpace={0}, space content active={1}, space sun on={2}, launch-pad sun on={3}, space skybox shown={4}, learner's rig still active={5}.",
                sw.InSpace, sw.spaceContent.activeInHierarchy, sw.spaceSun.enabled,
                sw.launchpadSun != null && sw.launchpadSun.enabled, RenderSettings.skybox == sw.spaceSkybox, rig != null && rig.isActiveAndEnabled));
            foreach (GameObject g in sw.launchpadObjects)
                if (g != null) report.AppendLine("  launch-pad object '" + g.name + "' active=" + g.activeSelf + " (its learner-rig branch is kept on if the rig lives inside it)");

            Transform head = HeadOf(rig);
            Camera eye = EyeOf(rig);
            Transform earth = Descendant(sw.transform, "Earth");
            if (earth != null)
            {
                Vector3 to = earth.position - head.position;
                float radius = earth.lossyScale.x;
                report.AppendLine(string.Format("Earth: {0:F1} m away, {1:F0} deg from straight ahead, {2:F0} deg elevation, appears {3:F0} deg wide.",
                    to.magnitude, Vector3.Angle(Flat(head.forward), Flat(to)), Mathf.Asin(to.normalized.y) * Mathf.Rad2Deg,
                    2f * Mathf.Asin(Mathf.Min(1f, radius / to.magnitude)) * Mathf.Rad2Deg));
            }
            Capture(eye, head.position, Level(head.forward), "3_Space_Forward");
            Capture(eye, head.position, Level(head.forward) * Quaternion.Euler(25f, 0f, 0f), "4_Space_LookingDown");
        }

        static void RestoreChecks()
        {
            var sw = Switcher();
            bool envBack = true;
            foreach (GameObject g in sw.launchpadObjects)
                if (g != null && !g.activeSelf) envBack = false;
            report.AppendLine(string.Format("Back on the launch pad: InSpace={0}, space content hidden={1}, launch-pad objects back={2}, skybox restored={3}, launch-pad sun restored={4}.",
                sw.InSpace, !sw.spaceContent.activeInHierarchy, envBack, RenderSettings.skybox == padSkybox,
                (sw.launchpadSun != null && sw.launchpadSun.enabled) == padSunOn));
            qwr.QwrCameraRig rig = qwr.QwrCameraRig._instance;
            Transform head = HeadOf(rig);
            Capture(EyeOf(rig), head.position, Level(head.forward), "5_LaunchPad_Restored");
        }

        // ---------------------------------------------------------------- helpers

        static SpaceEnvironmentSwitcher Switcher()
        {
            var sw = Object.FindObjectOfType<SpaceEnvironmentSwitcher>();
            if (sw == null) throw new InvalidOperationException("No SpaceEnvironmentSwitcher in the scene.");
            return sw;
        }

        static Transform HeadOf(qwr.QwrCameraRig rig)
        {
            if (rig != null && rig._center_transform != null) return rig._center_transform;
            if (rig != null) return rig.transform;
            if (Camera.main != null) return Camera.main.transform;
            throw new InvalidOperationException("No learner camera found.");
        }

        static Camera EyeOf(qwr.QwrCameraRig rig)
        {
            if (rig == null) return Camera.main;
            if (rig._center_transform != null)
            {
                var centre = rig._center_transform.GetComponent<Camera>();
                if (centre != null) return centre;
            }
            return rig.GetComponentInChildren<Camera>(true);
        }

        static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.forward;
        }

        static Quaternion Level(Vector3 forward)
        {
            return Quaternion.LookRotation(Flat(forward), Vector3.up);
        }

        static Bounds BoundsOf(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        static Transform Descendant(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        static string PathOf(Transform t)
        {
            string path = t.name;
            for (Transform p = t.parent; p != null; p = p.parent) path = p.name + "/" + path;
            return path;
        }

        // Renders from the learner's head with the eye camera's own settings, at headset field of view.
        static void Capture(Camera reference, Vector3 position, Quaternion rotation, string name)
        {
            var go = new GameObject("GSLV Test Camera") { hideFlags = HideFlags.HideAndDontSave };
            var rt = new RenderTexture(1024, 1024, 24);
            try
            {
                var cam = go.AddComponent<Camera>();
                if (reference != null) cam.CopyFrom(reference);
                cam.stereoTargetEye = StereoTargetEyeMask.None;
                cam.fieldOfView = reference != null && reference.fieldOfView >= 80f ? reference.fieldOfView : HeadsetFov;
                cam.targetTexture = rt;
                cam.enabled = false;
                cam.transform.SetPositionAndRotation(position, rotation);
                cam.Render();

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                RenderTexture.active = previous;
                string file = Path.Combine(Path.GetTempPath(), "GSLV_PlayTest_" + name + ".png");
                File.WriteAllBytes(file, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                report.AppendLine("  image: " + file);
            }
            finally
            {
                Object.DestroyImmediate(go);
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }

        static void Status(string text)
        {
            try
            {
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "GSLV_PlayTest_Report.txt"),
                    DateTime.Now.ToString("HH:mm:ss") + " " + text);
            }
            catch (Exception)
            {
                // Reporting is best effort only.
            }
        }
    }
}
