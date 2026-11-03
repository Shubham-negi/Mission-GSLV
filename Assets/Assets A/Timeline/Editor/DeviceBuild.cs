using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GslvMission.EditorTools
{
    /// <summary>
    /// Build and Run for the headset, the same as File > Build And Run (the scenes ticked in Build Settings, the Android
    /// player settings as they are), started by a trigger file so it can be queued from outside the editor. The APK goes to
    /// the temp folder; the result is written to %TEMP%\GSLV_Device_Build.txt. A queued Timeline rebuild or scenery fix runs first.
    /// </summary>
    [InitializeOnLoad]
    static class DeviceBuild
    {
        const string Trigger = "Assets/Assets A/Timeline/Editor/BUILD_DEVICE_ONCE.txt";
        const string TimelineTrigger = "Assets/Assets A/Timeline/Editor/BUILD_TIMELINE_ONCE.txt";
        const string SceneryTrigger = "Assets/Assets A/Timeline/Editor/FIX_SCENERY_ONCE.txt";
        static readonly string StatusFile = Path.Combine(Path.GetTempPath(), "GSLV_Device_Build.txt");
        static readonly string ApkPath = Path.Combine(Path.Combine(Path.GetTempPath(), "GSLV_Device"), "GSLV Mission.apk");

        static DeviceBuild()
        {
            if (File.Exists(Trigger)) EditorApplication.delayCall += RunFromTrigger;
        }

        static void RunFromTrigger()
        {
            if (!File.Exists(Trigger)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating || File.Exists(TimelineTrigger) || File.Exists(SceneryTrigger))
            {
                EditorApplication.delayCall += RunFromTrigger;
                return;
            }
            AssetDatabase.DeleteAsset(Trigger);
            if (File.Exists(Trigger)) File.Delete(Trigger);
            Build();
        }

        [MenuItem("GSLV/Build And Run On Headset")]
        static void Build()
        {
            string result;
            try
            {
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    throw new Exception("the active build target is " + EditorUserBuildSettings.activeBuildTarget + ", not Android");
                var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
                if (scenes.Length == 0) throw new Exception("no scene is ticked in Build Settings");
                bool saved = false;
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    var s = EditorSceneManager.GetSceneAt(i);
                    if (s.isDirty && scenes.Contains(s.path)) saved |= EditorSceneManager.SaveScene(s);   // a build reads the scenes from disk
                }
                Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
                var t0 = DateTime.Now;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = ApkPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.AutoRunPlayer
                });
                var sum = report.summary;
                result = string.Format("{0}: {1} in {2:0} s, {3:0.0} MB, {4} error(s), {5} warning(s)\nscenes: {6}\napk: {7}{8}",
                    sum.result == BuildResult.Succeeded ? "BUILT AND SENT TO THE HEADSET" : "BUILD " + sum.result.ToString().ToUpper(),
                    sum.result, (DateTime.Now - t0).TotalSeconds, sum.totalSize / 1048576.0, sum.totalErrors, sum.totalWarnings,
                    string.Join(", ", scenes), ApkPath, saved ? "\n(the open scene had unsaved changes and was saved first)" : "");
                foreach (var step in report.steps)
                    foreach (var m in step.messages.Where(m => m.type == LogType.Error || m.type == LogType.Exception))
                        result += "\nERROR: " + m.content;
            }
            catch (Exception e)
            {
                result = "BUILD NOT STARTED: " + e.Message;
            }
            File.WriteAllText(StatusFile, result);
            Debug.Log("[GSLV] Headset build: " + StatusFile);
        }
    }
}
