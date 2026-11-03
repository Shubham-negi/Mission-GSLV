/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

namespace qwr
{
#if UNITY_ANDROID && !UNITY_EDITOR
	public class JavaActivity
	{
		// Get this from Unity on startup so we can call Activity java functions.
		private bool androidJavaInit = false;
		private AndroidJavaObject activity;
		private AndroidJavaClass javaVrActivityClass;

		public void Init()
		{
			if (!androidJavaInit)
			{
				AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				//javaVrActivityClass = new AndroidJavaClass("com.qwr.sdk.VrActivity");
				// Prepare for the RenderThreadInit()
			    QWR_SetInitVariables(activity.GetRawObject(), (IntPtr)0);
				
				androidJavaInit = true;
			}
		}

		[DllImport(Common.LibQwr)]
		private static extern void QWR_SetInitVariables(IntPtr activity, IntPtr vrActivityClass);
	}
#endif
}