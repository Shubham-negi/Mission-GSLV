/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/
using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;
using System;

namespace qwr
{
	//[InitializeOnLoad]
	public class ConfigEngine
	{
		//static ConfigEngine
		//{
		//}

		static public void Init()
		{
			// log the unity version
			Debug.Log( "Unity Version: " + Application.unityVersion );
			Debug.Log( "Unity Plugin Version: " + QwrManager.QwrUnityVersion );  // change when and only when unity is released. Unity plugin version.

			#if UNITY_ANDROID && !UNITY_EDITOR
			// don't allow the application to run if orientation is not landscape left.
			// set screen orientation at runtime to override PlayerSettings.defaultInterfaceOrientation;
			Screen.orientation = ScreenOrientation.LandscapeLeft;
			if( Screen.orientation != ScreenOrientation.LandscapeLeft )
			{
				Debug.LogError("********************************************************************************\n");
				Debug.LogError("***** Default screen orientation must be set to landscape left for VR.\n" +
				               "***** Stopping application.\n");
				Debug.LogError("********************************************************************************\n");
				
				Debug.Break();
				Application.Quit();
			}
			
			// don't enable gyro, it is not used and triggers expensive display calls
			if( Input.gyro.enabled )
			{
				Debug.LogError("*** Auto-disabling Gyroscope ***");
				Input.gyro.enabled = false;
			}
			
			// NOTE: On Adreno Lollipop, it is an error to have antiAliasing set on the
			// main window surface with front buffer rendering enabled. The view will
			// render black.
			// On Adreno KitKat, some tiling control modes will cause the view to render
			// black.
			// As VR9 render to fb directly, need enable aa
			if (QwrDevice.bVR9)
			{
				QualitySettings.antiAliasing = 4;
				Debug.Log("VR9 platform AA should be enabled to " + QualitySettings.antiAliasing);
			}
			else
			{
				if( QualitySettings.antiAliasing > 1 )
			 	{
					Debug.LogWarning("*** Antialiasing should be disabled for better performance and no quality losing ***");
				}
			}
			// Disable screen dimming
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			#endif
			// try to render at 70fps
			string PROPERTY_REFRESH_RATE = "refresh_rate";
			IntPtr tempPtr = Marshal.StringToHGlobalAnsi(PROPERTY_REFRESH_RATE);
			Application.targetFrameRate = (int)Composer.QwruGetFloatValue(tempPtr, 70) / QwrManager.minimumVsync;
			Debug.Log("minimumVsync " + QwrManager.minimumVsync + " targetFrameRate " + Application.targetFrameRate);
        }
	}
}
