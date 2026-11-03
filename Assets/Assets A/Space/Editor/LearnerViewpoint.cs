using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GslvSpace.EditorTools
{
    /// <summary>
    /// Menu GSLV > Place Learner At Launch Pad. Makes the scene-root QwrCameraRig the only active camera
    /// rig (other rigs, e.g. inside the launch-pad prefab, are switched off as scene overrides) and moves
    /// it to an observation point about 70 m from the rocket with a clear view of it, facing it.
    /// Also runs once automatically when VIEWPOINT_ONCE.txt sits next to this script, then starts the
    /// Play-mode camera test.
    /// </summary>
    [InitializeOnLoad]
    static class LearnerViewpoint
    {
        const string Trigger = "Assets/Assets A/Space/Editor/VIEWPOINT_ONCE.txt";
        const string RocketName = "SLV_3_Textured";
        const string Title = "Learner Viewpoint";
        const float EyeHeight = 1.6f;
        static readonly float[] Distances = { 70f, 85f, 60f, 100f };

        static LearnerViewpoint()
        {
            if (File.Exists(Trigger)) EditorApplication.delayCall += RunFromTrigger;
        }

        [MenuItem("GSLV/Place Learner At Launch Pad")]
        static void Menu()
        {
            Run();
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
            if (Run()) CameraPlayTest.StartNow();
        }

        static bool Run()
        {
            var report = new StringBuilder();
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return Stop("Stop Play mode first.");
                Scene scene = SceneManager.GetActiveScene();
                if (!scene.path.StartsWith("Assets/Assets A/")) return Stop("Open your scene from Assets/Assets A first. Active scene: " + scene.path);

                Transform rig = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                    if (root.GetComponent<qwr.QwrCameraRig>() != null) { rig = root.transform; break; }
                if (rig == null) return Stop("No QwrCameraRig at the top level of the scene.");

                Transform rocket = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                        if (t.name == RocketName && t.gameObject.activeInHierarchy) { rocket = t; break; }
                if (rocket == null) return Stop("Rocket '" + RocketName + "' not found.");

                bool wasDirty = scene.isDirty;
                Undo.IncrementCurrentGroup();
                int group = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Place Learner At Launch Pad");

                // 1. One rig only: switch off every other active QwrCameraRig.
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (qwr.QwrCameraRig other in root.GetComponentsInChildren<qwr.QwrCameraRig>(true))
                    {
                        if (other.transform == rig || !other.gameObject.activeSelf) continue;
                        Undo.RecordObject(other.gameObject, "Disable extra camera rig");
                        other.gameObject.SetActive(false);
                        report.AppendLine("Switched off extra camera rig: " + PathOf(other.transform));
                    }

                // 2. Find the open-ground spot from which the most of the rocket can actually be seen.
                Bounds rb = BoundsOf(rocket.gameObject);
                Vector3 axis = new Vector3(rb.center.x, rb.min.y, rb.center.z);
                float height = rb.size.y;
                Vector3 middle = axis + Vector3.up * (height * 0.5f);
                Vector3 preferred = rig.position - axis;
                preferred.y = 0f;
                preferred = preferred.sqrMagnitude > 1f ? preferred.normalized : Vector3.back;
                List<Renderer> nearby = Occluders(scene, rocket, axis, rig);
                Physics.SyncTransforms();

                var candidates = new List<Candidate>();
                using (var probe = new VisibilityProbe(rocket))
                {
                    foreach (float distance in Distances)
                    {
                        for (int step = 0; step < 24; step++)
                        {
                            float offset = Mathf.DeltaAngle(0f, step * 15f);
                            Vector3 dir = Quaternion.AngleAxis(offset, Vector3.up) * preferred;
                            Vector3 ground;
                            if (!GroundAt(axis + dir * distance, rb.max.y + 200f, out ground)) continue;
                            if (Mathf.Abs(ground.y - axis.y) > 15f) continue;
                            Vector3 eye = ground + Vector3.up * EyeHeight;
                            // Keep clear of buildings, masts and trees so nothing looms beside the learner.
                            if (nearby.Exists(r => r.bounds.SqrDistance(eye) < 36f)) continue;
                            candidates.Add(new Candidate
                            {
                                eye = eye,
                                distance = distance,
                                offset = offset,
                                visiblePixels = probe.VisiblePixels(eye, middle)
                            });
                        }
                    }
                }
                if (candidates.Count == 0)
                {
                    Undo.RevertAllDownToGroup(group);
                    return Stop("Could not find open ground around the rocket to stand on.");
                }
                // Most rocket visible; then closest to the side the learner used before; then nearest 70 m.
                candidates.Sort((a, b) =>
                {
                    int byPixels = b.visiblePixels.CompareTo(a.visiblePixels);
                    if (Mathf.Abs(a.visiblePixels - b.visiblePixels) > a.visiblePixels / 20) return byPixels;
                    int bySide = Mathf.Abs(a.offset).CompareTo(Mathf.Abs(b.offset));
                    return bySide != 0 ? bySide : Mathf.Abs(a.distance - 70f).CompareTo(Mathf.Abs(b.distance - 70f));
                });
                Candidate best = candidates[0];
                Vector3 bestEye = best.eye;
                string bestNote = string.Format("{0:F0} m from the rocket, {1:F0} deg from your previous side, {2} rocket pixels visible",
                    best.distance, best.offset, best.visiblePixels);
                for (int i = 1; i < Mathf.Min(4, candidates.Count); i++)
                    report.AppendLine(string.Format("Runner-up {0}: {1:F0} m, {2:F0} deg, {3} rocket pixels visible, at {4}",
                        i, candidates[i].distance, candidates[i].offset, candidates[i].visiblePixels, candidates[i].eye));
                int bestVisible = best.visiblePixels;

                // 3. Move the rig there, facing the rocket.
                Vector3 look = middle - bestEye;
                look.y = 0f;
                Undo.RecordObject(rig, "Move learner");
                rig.SetPositionAndRotation(bestEye, Quaternion.LookRotation(look.normalized, Vector3.up));

                var switcher = Object.FindObjectOfType<SpaceEnvironmentSwitcher>();
                if (switcher != null)
                {
                    Undo.RecordObject(switcher, "Set learner rig");
                    switcher.viewer = rig;
                }
                Undo.CollapseUndoOperations(group);

                float dist = Vector3.Distance(new Vector3(bestEye.x, 0f, bestEye.z), new Vector3(axis.x, 0f, axis.z));
                report.Insert(0, string.Format("[GSLV] Learner rig '{0}' placed at {1} ({2}), facing the rocket. Rocket: {3:F1} m tall, {4:F0} m away, appears {5:F0} deg tall.\n",
                    rig.name, bestEye, bestNote, height, dist,
                    (Mathf.Atan2(rb.max.y - bestEye.y, dist) - Mathf.Atan2(rb.min.y - bestEye.y, dist)) * Mathf.Rad2Deg));
                if (bestVisible == 0) report.AppendLine("Warning: the rocket is hidden from every open spot checked; place QwrCameraRig by hand.");
                report.AppendLine("Preview: " + Preview(rig, bestEye));

                EditorSceneManager.MarkSceneDirty(scene);
                if (!wasDirty && EditorSceneManager.SaveScene(scene)) report.AppendLine("Scene saved.");
                else report.AppendLine("Scene not saved automatically (it had other unsaved changes). Save it (Ctrl+S) when ready.");
                Debug.Log(report.ToString());
                Status("done\n" + report);
                Selection.activeTransform = rig;
                return true;
            }
            catch (Exception e)
            {
                Status("failed\n" + e + "\n" + report);
                throw;
            }
        }

        // Renderers that could stand between the learner and the rocket: not the rocket's own pad, cart or
        // tower (within 15 m of its axis), not huge ground pieces, and not the learner's rig.
        static List<Renderer> Occluders(Scene scene, Transform rocket, Vector3 axis, Transform rig)
        {
            var list = new List<Renderer>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
                {
                    if (!r.enabled || r.transform.IsChildOf(rocket) || r.transform.IsChildOf(rig)) continue;
                    if (r is ParticleSystemRenderer || r is LineRenderer || r is TrailRenderer) continue;
                    if (r.GetComponentInParent<SpaceEnvironmentSwitcher>() != null) continue;
                    Bounds b = r.bounds;
                    if (b.extents.x > 60f || b.extents.z > 60f) continue;
                    Vector2 c = new Vector2(b.center.x - axis.x, b.center.z - axis.z);
                    if (c.magnitude < 15f + Mathf.Max(b.extents.x, b.extents.z) * 0.5f) continue;
                    list.Add(r);
                }
            }
            return list;
        }

        sealed class Candidate
        {
            public Vector3 eye;
            public float distance;
            public float offset;
            public int visiblePixels;
        }

        /// <summary>
        /// Counts how much of the rocket can be seen from a spot: the rocket is drawn flat red for the
        /// probe renders and everything else as normal, so surviving red pixels are visible rocket.
        /// The rocket's own materials and the fog setting are restored on Dispose.
        /// </summary>
        sealed class VisibilityProbe : IDisposable
        {
            const int Size = 128;
            readonly Renderer[] renderers;
            readonly Material[][] saved;
            readonly Material red;
            readonly GameObject cameraObject;
            readonly Camera cam;
            readonly RenderTexture rt;
            readonly Texture2D readback;
            readonly bool savedFog;

            public VisibilityProbe(Transform rocket)
            {
                savedFog = RenderSettings.fog;
                RenderSettings.fog = false;
                red = new Material(Shader.Find("Unlit/Color")) { hideFlags = HideFlags.HideAndDontSave, color = Color.red };
                renderers = rocket.GetComponentsInChildren<Renderer>();
                saved = new Material[renderers.Length][];
                for (int i = 0; i < renderers.Length; i++)
                {
                    saved[i] = renderers[i].sharedMaterials;
                    var flat = new Material[saved[i].Length];
                    for (int m = 0; m < flat.Length; m++) flat[m] = red;
                    renderers[i].sharedMaterials = flat;
                }
                cameraObject = new GameObject("GSLV Visibility Probe") { hideFlags = HideFlags.HideAndDontSave };
                cam = cameraObject.AddComponent<Camera>();
                cam.enabled = false;
                cam.stereoTargetEye = StereoTargetEyeMask.None;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.fieldOfView = 50f;
                cam.nearClipPlane = 0.05f;
                cam.farClipPlane = 2000f;
                rt = new RenderTexture(Size, Size, 24);
                cam.targetTexture = rt;
                readback = new Texture2D(Size, Size, TextureFormat.RGB24, false);
            }

            public int VisiblePixels(Vector3 eye, Vector3 target)
            {
                cam.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(target - eye, Vector3.up));
                cam.Render();
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = rt;
                readback.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                readback.Apply(false);
                RenderTexture.active = previous;
                int count = 0;
                foreach (Color32 c in readback.GetPixels32())
                    if (c.r > 240 && c.g < 16 && c.b < 16) count++;
                return count;
            }

            public void Dispose()
            {
                for (int i = 0; i < renderers.Length; i++)
                    if (renderers[i] != null) renderers[i].sharedMaterials = saved[i];
                RenderSettings.fog = savedFog;
                cam.targetTexture = null;
                Object.DestroyImmediate(cameraObject);
                rt.Release();
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(readback);
                Object.DestroyImmediate(red);
            }
        }

        // Open ground only: if something with a collider stands higher than the terrain here (a roof,
        // a tree), the spot is skipped rather than putting the learner on top of it.
        static bool GroundAt(Vector3 point, float fromHeight, out Vector3 ground)
        {
            float terrainY = float.NaN;
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                Vector3 p = terrain.transform.position, size = terrain.terrainData.size;
                if (point.x < p.x || point.z < p.z || point.x > p.x + size.x || point.z > p.z + size.z) continue;
                terrainY = p.y + terrain.SampleHeight(point);
                break;
            }

            RaycastHit hit;
            bool hasHit = Physics.Raycast(new Vector3(point.x, fromHeight, point.z), Vector3.down, out hit, fromHeight + 1000f);
            if (hasHit && !float.IsNaN(terrainY) && hit.point.y > terrainY + 0.5f)
            {
                ground = Vector3.zero;
                return false;
            }
            if (hasHit)
            {
                ground = hit.point;
                return true;
            }
            if (!float.IsNaN(terrainY))
            {
                ground = new Vector3(point.x, terrainY, point.z);
                return true;
            }
            ground = Vector3.zero;
            return false;
        }

        static Bounds BoundsOf(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        static string Preview(Transform rig, Vector3 eye)
        {
            var go = new GameObject("GSLV Viewpoint Camera") { hideFlags = HideFlags.HideAndDontSave };
            var rt = new RenderTexture(1024, 1024, 24);
            try
            {
                var cam = go.AddComponent<Camera>();
                Camera reference = rig.GetComponentInChildren<Camera>(true);
                if (reference != null) cam.CopyFrom(reference);
                cam.stereoTargetEye = StereoTargetEyeMask.None;
                cam.fieldOfView = 96f;
                cam.targetTexture = rt;
                cam.enabled = false;
                cam.transform.SetPositionAndRotation(eye, rig.rotation);
                cam.Render();
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                RenderTexture.active = previous;
                string file = Path.Combine(Path.GetTempPath(), "GSLV_Viewpoint_Preview.png");
                File.WriteAllBytes(file, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                return file;
            }
            finally
            {
                Object.DestroyImmediate(go);
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }

        static bool Stop(string reason)
        {
            Debug.LogWarning("[GSLV] Viewpoint not changed: " + reason);
            Status("stopped\n" + reason);
            EditorUtility.DisplayDialog(Title, reason, "OK");
            return false;
        }

        static string PathOf(Transform t)
        {
            string path = t.name;
            for (Transform p = t.parent; p != null; p = p.parent) path = p.name + "/" + path;
            return path;
        }

        static void Status(string text)
        {
            try
            {
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "GSLV_Viewpoint_Status.txt"), DateTime.Now.ToString("HH:mm:ss") + " " + text);
            }
            catch (Exception)
            {
                // Reporting is best effort only.
            }
        }
    }
}
