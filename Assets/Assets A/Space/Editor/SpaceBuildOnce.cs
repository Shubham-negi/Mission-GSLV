using System.IO;
using UnityEditor;
using UnityEngine;

namespace GslvSpace.EditorTools
{
    /// <summary>
    /// Runs GSLV > Build Space Environment once, automatically, after scripts reload, if the trigger
    /// file BUILD_ONCE.txt sits next to this script. The trigger is deleted before the build runs,
    /// so it never repeats. Without the trigger file this does nothing.
    /// </summary>
    [InitializeOnLoad]
    static class SpaceBuildOnce
    {
        const string Trigger = "Assets/Assets A/Space/Editor/BUILD_ONCE.txt";

        static SpaceBuildOnce()
        {
            if (File.Exists(Trigger)) EditorApplication.delayCall += Run;
        }

        static void Run()
        {
            if (!File.Exists(Trigger)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Run; // try again once the editor is idle
                return;
            }
            AssetDatabase.DeleteAsset(Trigger);
            if (File.Exists(Trigger)) File.Delete(Trigger);
            SpaceEnvironmentBuilder.Run(false); // the trigger file is the go-ahead, so no rebuild prompt
        }
    }
}
