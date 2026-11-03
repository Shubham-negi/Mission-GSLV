using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GslvMission.EditorTools
{
    // Temporary: sizes of the heavy parts shown in space. Report in %TEMP%\GSLV_Heavy_Parts.txt.
    [InitializeOnLoad]
    static class HeavyPartsReport
    {
        const string Trigger = "Assets/Assets A/Timeline/Editor/REPORT_HEAVY_ONCE.txt";

        static HeavyPartsReport()
        {
            if (File.Exists(Trigger)) EditorApplication.delayCall += Run;
        }

        static void Run()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Run; return; }
            AssetDatabase.DeleteAsset(Trigger);
            var sb = new StringBuilder("HEAVY PARTS " + DateTime.Now + "\n");
            try
            {
                var all = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToList();
                var model = all.First(t => t.name == "gslv rocket model com" && t.GetComponentInParent<Animator>() != null);
                foreach (var t in model.GetComponentsInChildren<Transform>(true).Where(t => t.parent == model || (t.parent != null && t.parent.parent == model)))
                {
                    var mfs = t.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null).ToArray();
                    if (mfs.Length == 0) continue;
                    long tris = mfs.Sum(f => (long)f.sharedMesh.triangles.Length / 3), verts = mfs.Sum(f => (long)f.sharedMesh.vertexCount);
                    var rs = t.GetComponentsInChildren<MeshRenderer>(true);
                    var mats = rs.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
                    var b = rs.Length > 0 ? rs[0].bounds : new Bounds(); foreach (var r in rs) b.Encapsulate(r.bounds);
                    sb.AppendFormat("{0}  renderers {1}  meshes {2}  tris {3:N0}  verts {4:N0}  materials {5} [{6}]  size {7}  scale {8}\n",
                        AnimationUtility.CalculateTransformPath(t, model.parent), rs.Length, mfs.Select(f => f.sharedMesh).Distinct().Count(), tris, verts, mats.Length,
                        string.Join(", ", mats.Select(m => m.name + "/" + m.shader.name + "/q" + m.renderQueue).ToArray()), b.size.ToString("F2"), t.lossyScale.ToString("F3"));
                    if (t.name == "Satellite" || t.name.Trim() == "Middle engine")
                    {
                        var sizes = mfs.Select(f => f.GetComponent<Renderer>() != null ? f.GetComponent<Renderer>().bounds.size.magnitude : 0f).OrderBy(x => x).ToArray();
                        sb.AppendFormat("    part sizes (m): min {0:0.000} p25 {1:0.000} median {2:0.000} p75 {3:0.000} max {4:0.00}; parts under 5 cm: {5} with {6:N0} tris\n",
                            sizes.First(), sizes[sizes.Length / 4], sizes[sizes.Length / 2], sizes[sizes.Length * 3 / 4], sizes.Last(),
                            mfs.Count(f => f.GetComponent<Renderer>() != null && f.GetComponent<Renderer>().bounds.size.magnitude < 0.05f),
                            mfs.Where(f => f.GetComponent<Renderer>() != null && f.GetComponent<Renderer>().bounds.size.magnitude < 0.05f).Sum(f => (long)f.sharedMesh.triangles.Length / 3));
                        sb.AppendFormat("    readable meshes: {0} of {1}; source: {2}\n", mfs.Count(f => f.sharedMesh.isReadable), mfs.Length,
                            string.Join(", ", mfs.Select(f => AssetDatabase.GetAssetPath(f.sharedMesh)).Distinct().ToArray()));
                        foreach (var c in t.Cast<Transform>().Take(12))
                            sb.AppendFormat("    child {0}: {1} renderers, {2:N0} tris\n", c.name, c.GetComponentsInChildren<MeshRenderer>(true).Length,
                                c.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null).Sum(f => (long)f.sharedMesh.triangles.Length / 3));
                    }
                }
            }
            catch (Exception e) { sb.AppendLine("FAILED: " + e); }
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "GSLV_Heavy_Parts.txt"), sb.ToString());
        }
    }
}
