/************************************************************************************

Copyright: Copyright(c) 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

class ProjectBuild : Editor {
	[MenuItem("QWR/QwrBuild/Android")]
    public static void BuildForAndroid()
    {
        QualitySettings.vSyncCount = 0;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
		string[] levels = new string[] { "Assets/QWR/Scenes/Cubes.unity", "Assets/QWR/Scenes/Spheres.unity" };
		BuildPipeline.BuildPlayer(levels, projectPath + "/QWRUnityForAndroid.apk", BuildTarget.Android, BuildOptions.None);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
    }

	[MenuItem("QWR/QwrBuild/Win32")]
    public static void BuildForWin32()
    {
        QualitySettings.vSyncCount = 0;
		string[] levels = new string[] { "Assets/QWR/Scenes/Cubes.unity", "Assets/QWR/Scenes/Spheres.unity" };
		BuildPipeline.BuildPlayer(levels, projectPath + "/QWRUnityForWin32.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
    }

	[MenuItem("QWR/QwrBuild/Win64")]
    public static void BuildForx64()
    {
        QualitySettings.vSyncCount = 0;
		string[] levels = new string[] { "Assets/QWR/Scenes/Cubes.unity", "Assets/QWR/Scenes/Spheres.unity" };
		BuildPipeline.BuildPlayer(levels, projectPath + "/QWRUnityForWin64.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    public static string projectPath
    {
        get
        {
			string path = Path.GetFullPath(".");
			if (path.Contains("QwrAsiaUnity"))
			{
				path += "/../Release";
			}
			else
			{
				path += "/Release";
			}
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string releasepath = path + "/" + Application.unityVersion;
            if (!Directory.Exists(releasepath))
            {
                Directory.CreateDirectory(releasepath); ;
            }
            return releasepath;
        }
    }
}
