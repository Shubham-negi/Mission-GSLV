using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GslvMission.EditorTools
{
    /// <summary>
    /// Bakes the launch-pad lighting for the headset. The scenery models get lightmap UVs first (their FBX files had none,
    /// so the lightmaps were laid over their texture UVs); a model outside Assets A is left alone and its renderers take
    /// their light from the light probes instead. The scenery flicker fix runs again so its mesh copies carry the new UVs.
    /// The settings suit a mobile GPU: one lightmap sample per pixel (non-directional), 10 texels a metre, 1024 atlases,
    /// compressed. Menu GSLV/Bake Lighting, or the trigger file; the report goes to %TEMP%\GSLV_Light_Bake.txt.
    /// </summary>
    [InitializeOnLoad]
    static class BakeLighting
    {
        const string Trigger = "Assets/Assets A/Timeline/Editor/BAKE_LIGHT_ONCE.txt";
        const string TimelineTrigger = "Assets/Assets A/Timeline/Editor/BUILD_TIMELINE_ONCE.txt";
        const string SceneryTrigger = "Assets/Assets A/Timeline/Editor/FIX_SCENERY_ONCE.txt";
        const string FlameShotsTrigger = "Assets/Assets A/Timeline/Editor/FLAME_SHOTS_ONCE.txt";
        static readonly string ReportFile = Path.Combine(Path.GetTempPath(), "GSLV_Light_Bake.txt");

        static BakeLighting()
        {
            if (File.Exists(Trigger)) EditorApplication.update += RunFromTrigger;   // update, not delayCall: only update ticks with the editor in the background
        }

        static void RunFromTrigger()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating || File.Exists(TimelineTrigger) || File.Exists(SceneryTrigger) || File.Exists(FlameShotsTrigger)) return;   // next tick
            EditorApplication.update -= RunFromTrigger;
            if (!File.Exists(Trigger)) return;
            AssetDatabase.DeleteAsset(Trigger);
            if (File.Exists(Trigger)) File.Delete(Trigger);
            Run();
        }

        [MenuItem("GSLV/Bake Lighting")]
        static void Run()
        {
            var sb = new StringBuilder();
            try { Bake(sb); }
            catch (Exception e) { sb.AppendLine("BAKE FAILED: " + e); }
            File.WriteAllText(ReportFile, sb.ToString());
            Debug.Log("[GSLV] Light bake: " + ReportFile);
        }

        static void Bake(StringBuilder sb)
        {
            var scene = EditorSceneManager.GetActiveScene();
            sb.AppendLine("LIGHT BAKE " + DateTime.Now + "  scene " + scene.path);
            var env = scene.GetRootGameObjects().FirstOrDefault(r => r.name == "ENVIRONMENT");
            if (env == null) throw new Exception("no ENVIRONMENT root in the open scene");

            // lightmap UVs
            var models = new HashSet<string>();
            var outside = new List<MeshRenderer>();
            foreach (var mr in env.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!GameObjectUtility.AreStaticEditorFlagsSet(mr.gameObject, StaticEditorFlags.ContributeGI) || mr.receiveGI != ReceiveGI.Lightmaps) continue;
                var mf = mr.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                var mesh = DepthSafeScenery.OriginalOf(mf.sharedMesh);
                if (mesh.HasVertexAttribute(VertexAttribute.TexCoord1)) continue;   // the model has its own
                string path = AssetDatabase.GetAssetPath(mesh);
                var mi = AssetImporter.GetAtPath(path) as ModelImporter;
                if (mi != null && path.StartsWith("Assets/Assets A/")) models.Add(path);
                else outside.Add(mr);
            }
            if (models.Count > 0)
            {
                try
                {
                    AssetDatabase.StartAssetEditing();
                    foreach (var path in models)
                    {
                        var mi = (ModelImporter)AssetImporter.GetAtPath(path);
                        mi.generateSecondaryUV = true;
                        mi.SaveAndReimport();
                    }
                }
                finally { AssetDatabase.StopAssetEditing(); }
                AssetDatabase.Refresh();
            }
            sb.AppendLine("lightmap UVs generated for " + models.Count + " model(s):");
            foreach (var path in models) sb.AppendLine("  " + path);
            foreach (var mr in outside)
            {
                Undo.RecordObject(mr, "Bake Lighting");
                mr.receiveGI = ReceiveGI.LightProbes;
            }
            sb.AppendLine(outside.Count + " renderer(s) with no lightmap UVs outside Assets A now take light probes");

            // the scenery's mesh copies are made from the models: again, so they carry the new UVs
            DepthSafeScenery.Run();
            sb.AppendLine("scenery flicker fix run again (report: " + Path.Combine(Path.GetTempPath(), "GSLV_Scenery_Fix.txt") + ")");

            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            Lightmapping.bakedGI = true;
            Lightmapping.realtimeGI = false;
            LightmapEditorSettings.lightmapsMode = LightmapsMode.NonDirectional;
            LightmapEditorSettings.bakeResolution = 10f;
            LightmapEditorSettings.maxAtlasSize = 1024;
            LightmapEditorSettings.textureCompression = true;
            LightmapEditorSettings.padding = 2;
            var t0 = DateTime.Now;
            bool ok = Lightmapping.Bake();
            sb.AppendLine(string.Format("{0} in {1:0} s: {2} lightmap(s), non-directional, 10 texels/m, atlas 1024, compressed",
                ok ? "BAKED" : "BAKE DID NOT FINISH", (DateTime.Now - t0).TotalSeconds, LightmapSettings.lightmaps.Length));
            EditorSceneManager.MarkSceneDirty(scene);
            sb.AppendLine(EditorSceneManager.SaveScene(scene) ? "Scene saved." : "SCENE NOT SAVED");
        }
    }
}
