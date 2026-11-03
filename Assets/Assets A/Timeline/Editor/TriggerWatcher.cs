using System;
using System.IO;
using UnityEditor;

namespace GslvMission.EditorTools
{
    /// <summary>
    /// The one-shot jobs next to this script (BUILD_TIMELINE_ONCE.txt, BUILD_DEVICE_ONCE.txt, ...) start when the editor
    /// reloads its scripts, which it only does when its window gets focus. This watches for a trigger file written after the
    /// last reload and reloads the scripts itself, so a queued job runs while the editor sits in the background.
    /// </summary>
    [InitializeOnLoad]
    static class TriggerWatcher
    {
        const string Folder = "Assets/Assets A/Timeline/Editor";
        static readonly DateTime loadedAt = DateTime.Now;
        static double nextCheck;
        static bool requested;

        static TriggerWatcher()
        {
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            if (requested || EditorApplication.timeSinceStartup < nextCheck) return;
            nextCheck = EditorApplication.timeSinceStartup + 2.0;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            foreach (var f in Directory.GetFiles(Folder, "*_ONCE.txt"))
            {
                if (File.GetLastWriteTime(f) <= loadedAt) continue;   // already there at the last reload: its job has it
                requested = true;
                AssetDatabase.Refresh();
                EditorUtility.RequestScriptReload();
                return;
            }
        }
    }
}
