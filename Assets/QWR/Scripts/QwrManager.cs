/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

//using System.IO;
//using System.Collections;
//using System.Runtime.InteropServices;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.IO;

namespace qwr
{
	/*
	[System.Serializable]
	public struct SettingInfo
	{
		public qwrcOutputMode pcScreenOutputMode;
		public TEXTURE_DEPTH eyeTextureDepth;
		public float worldScale;
		public int minimumVsync;
		public bool resetTrackerOnLoad;
		public float eyeTextureScale;
		public bool polarisSupport;
		public bool androidEditorUseHmd;
	}
	*/

	public struct SettingFileInfo
	{
		public qwrcOutputMode pcScreenOutputMode;
		public TEXTURE_DEPTH eyeTextureDepth;
		public float worldScale;
		public int minimumVsync;
		public bool resetTrackerOnLoad;
		public float pcEyeTextureScale;
		public float mobileEyeTextureScale;
        public QWRPeripheral peripheral;
        public QWRKeyMode controllerKeyMode;
        public bool androidEditorUseHmd;
        public bool useQWRPointer;
        public bool hmdPointerSwitchable;
        public TouchPosOrig touchPosOrig;
	}

    public enum QWRKeyMode
    {
        QWR,
        STEAM
    }

    public enum TouchPosOrig
    {
        TOP_LEFT,
        CENTER,
    }

    public enum QWRPeripheral
    {
        None = 0,
        //Nolo = 3,        //mobile support only
        Flip = 4,        //mobile support only
    }

    /// <summary>
    /// Configuration for Qwr virtual reality.
    /// Will be deprecated in the future
    /// </summary>
    public static class QwrManager
	{
		static QwrManager()
		{
			TextAsset file_info = Resources.Load("QWR/QWRUnityConfig") as TextAsset;
			if (!file_info)
			{
				Debug.LogError("QWRUnityConfig file don't exists");
				return;
			}
			if (file_info.text == string.Empty)
			{
				Debug.LogError("QWRUnityConfig file has no data");
				return;
			}
			SettingFileInfo settingInfo = JsonUtility.FromJson<SettingFileInfo>(file_info.text);
			#if !UNITY_ANDROID
			pcScreenOutputMode = settingInfo.pcScreenOutputMode;
			#endif
			eyeTextureDepth = settingInfo.eyeTextureDepth;
			worldScale = settingInfo.worldScale;
			minimumVsync = settingInfo.minimumVsync;
			resetTrackerOnLoad = settingInfo.resetTrackerOnLoad;
            peripheral = settingInfo.peripheral;
			#if !UNITY_ANDROID
			eyeTextureScale = settingInfo.pcEyeTextureScale;
            controllerKeyMode = settingInfo.controllerKeyMode;
            #else
			eyeTextureScale = settingInfo.mobileEyeTextureScale;
            #endif
            #if UNITY_ANDROID
			androidEditorUseHmd = settingInfo.androidEditorUseHmd;
            #else
            androidEditorUseHmd = true;
			#endif

            QWRPointer = settingInfo.useQWRPointer;

            touchPosOrig = settingInfo.touchPosOrig;
            hmdPointerSwitchable = settingInfo.hmdPointerSwitchable;
		}
		/// <summary>
		/// Qwr unity version
		/// </summary>
		public static string QwrUnityVersion = "0.1.11";

		/// <summary>
		/// The format of each eye texture.
		/// </summary>
		//public RenderTextureFormat eyeTextureFormat = RenderTextureFormat.Default;
		public static RenderTextureFormat eyeTextureFormat
		{
			get
			{
				return RenderTextureFormat.ARGB32;
			}
		}

		#if !UNITY_ANDROID
		public static qwrcOutputMode pcScreenOutputMode = qwrcOutputMode.LEFT_EYE;
		#endif
		
		/// <summary>
		/// The depth of each eye texture in bits. Valid Unity render texture depths are 0, 16, and 24.
		/// </summary>
		public static TEXTURE_DEPTH eyeTextureDepth = TEXTURE_DEPTH._24;

		public static float worldScale = 1.0f;

		public static int minimumVsync = 1;
		
		/// <summary>
		/// If true, each scene load will cause the head pose to reset.
		/// </summary>
		public static bool resetTrackerOnLoad = true;

		/// <summary>
		/// Controls the size of the eye textures.
		/// Values must be above 0.
		/// Values below 1 permit sub-sampling for improved performance.
		/// Values above 1 permit super-sampling for improved sharpness.
		/// </summary>
		[Range(0.1f, 2.0f)]
		public static float eyeTextureScale = 1.5f;

		private static bool Bool_DeviceInfo = false;

		public static bool androidEditorUseHmd = false;

        public static QWRKeyMode controllerKeyMode = QWRKeyMode.QWR;

        public static QWRPeripheral peripheral = QWRPeripheral.None;

        public static bool QWRPointer = true;

        public static TouchPosOrig touchPosOrig = TouchPosOrig.TOP_LEFT;
        public static bool hmdPointerSwitchable = true;
        public static bool IsScriptRenderingPipeline
        {
            get
            {
#if UNITY_2019_1_OR_NEWER
                return UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null;
#else
                return false;
#endif
            }
        }

        private static qwrnDeviceInfo _DeviceInfo;

		public static qwrnDeviceInfo DeviceInfo
		{
			get
			{
				if (!Bool_DeviceInfo)
				{
				#if UNITY_ANDROID && !UNITY_EDITOR
					QwrDevice.InitJavaActivity();
				#endif

					Composer.Init();

					bool ret = Composer.QwruGetDeviceInfo(ref _DeviceInfo);
					if ((!ret) || (_DeviceInfo.resolution_x <= 0) || (_DeviceInfo.resolution_y <= 0))
					{
						//case: editor mode with no platform sdk installed
						_DeviceInfo.ipd = 0.064f * worldScale;
						#if UNITY_ANDROID && !UNITY_EDITOR
						_DeviceInfo.fov_x = 96.0f;
						_DeviceInfo.fov_y = 96.0f;
						#else
						_DeviceInfo.fov_x = 100.0f;
						_DeviceInfo.fov_y = 100.0f;
						#endif
						_DeviceInfo.resolution_x = 1024;
						_DeviceInfo.resolution_y = 1024;
					}
					_DeviceInfo.ipd = _DeviceInfo.ipd * worldScale;
					_DeviceInfo.resolution_x = (int)(_DeviceInfo.resolution_x * QwrManager.eyeTextureScale);
					_DeviceInfo.resolution_y = (int)(_DeviceInfo.resolution_y * QwrManager.eyeTextureScale);
					Bool_DeviceInfo = ret;
				}
				return _DeviceInfo;
			}
		}
	}
}
