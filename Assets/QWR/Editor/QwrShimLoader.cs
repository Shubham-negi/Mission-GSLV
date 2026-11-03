/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Diagnostics;

[InitializeOnLoad]
public class QwrShimLoader
{
	static QwrShimLoader()
	{

	}

	[PostProcessBuild]
	public static void OnPostProcessBuild(BuildTarget target, string project_path)
	{


	}

	private static void OverwriteFile( string src , string dest )
	{
		if (File.Exists(dest))
			File.Delete(dest);
		
		File.Copy(src, dest);
	}

	[MenuItem("QWR/Developer Website")]
    static void OpenQwrDeveloperSite()
    {
        UnityEngine.Debug.Log("Open developer.qwr.asia");
        Process foo = new Process();
		foo.StartInfo.FileName = "Assets\\QWR\\Utilities\\Qwrwebsite\\OpenDeveloperSiteOfQwr.bat";
        foo.StartInfo.Arguments = "";
        foo.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        foo.Start();
    }

}
