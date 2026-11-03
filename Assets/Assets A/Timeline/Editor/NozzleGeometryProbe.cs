using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GslvMission.EditorTools
{
    static class NozzleGeometryProbe
    {
        [MenuItem("GSLV/Timeline/Report Nozzle Geometry")]
        static void Report()
        {
            var scene = SceneManager.GetActiveScene();
            var model = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == "gslv rocket model com");
            if (model == null) { Debug.LogError("[NOZZLE PROBE] model not found"); return; }

            foreach (string name in new[] { "Louncher.001", "Louncher.005", "Louncher.006", "Middle engine", "J_2S_J_2_0" })
            {
                var part = model.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => string.Equals(t.name.Trim(), name, StringComparison.OrdinalIgnoreCase));
                if (part == null) { Debug.Log("[NOZZLE PROBE] " + name + ": missing"); continue; }
                Debug.Log(string.Format("[NOZZLE PROBE] PART {0} pos={1:F4} local={2:F4} scale={3:F4}", name, part.position, part.localPosition, part.lossyScale));
                foreach (var r in part.GetComponentsInChildren<Renderer>(true))
                    Debug.Log(string.Format("[NOZZLE PROBE] RENDERER {0}/{1} center={2:F4} size={3:F4} min={4:F4} max={5:F4}", name, r.name, r.bounds.center, r.bounds.size, r.bounds.min, r.bounds.max));

                foreach (var mf in part.GetComponentsInChildren<MeshFilter>(true))
                {
                    var mesh = mf.sharedMesh;
                    if (mesh == null || !mesh.isReadable) { Debug.Log("[NOZZLE PROBE] MESH " + name + "/" + mf.name + ": unreadable path=" + AssetDatabase.GetAssetPath(mesh)); continue; }
                    var world = mesh.vertices.Select(v => mf.transform.TransformPoint(v)).ToArray();
                    float minY = world.Min(v => v.y), maxY = world.Max(v => v.y);
                    float band = Mathf.Max(0.02f, (maxY - minY) * 0.025f);
                    var low = world.Where(v => v.y <= minY + band).ToArray();
                    Debug.Log(string.Format("[NOZZLE PROBE] MESH {0}/{1} verts={2} y={3:F4}..{4:F4} low={5} lowX={6:F4}..{7:F4} lowZ={8:F4}..{9:F4} lowAvg=({10:F4},{11:F4},{12:F4})",
                        name, mf.name, world.Length, minY, maxY, low.Length, low.Min(v => v.x), low.Max(v => v.x), low.Min(v => v.z), low.Max(v => v.z), low.Average(v => v.x), low.Average(v => v.y), low.Average(v => v.z)));
                    if (name == "Louncher.001")
                    {
                        float z0 = low.Min(v => v.z), z1 = low.Max(v => v.z);
                        for (int bin = 0; bin < 12; bin++)
                        {
                            float a = Mathf.Lerp(z0, z1, bin / 12f), b = Mathf.Lerp(z0, z1, (bin + 1) / 12f);
                            var slice = low.Where(v => v.z >= a && (bin == 11 ? v.z <= b : v.z < b)).ToArray();
                            if (slice.Length > 0) Debug.Log(string.Format("[NOZZLE PROBE] L110 ZBIN {0}: {1:F4}..{2:F4} n={3} avgX={4:F4} avgZ={5:F4}", bin, a, b, slice.Length, slice.Average(v => v.x), slice.Average(v => v.z)));
                        }
                    }
                }
            }
        }
    }
}
