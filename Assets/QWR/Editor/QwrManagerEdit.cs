/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace qwr
{
	[InitializeOnLoad]
	class QwrManagerEdit : ScriptableWizard
	{
		private Texture2D logo;

		private static QwrManagerEdit instance;

		private SettingFileInfo settingInfo;

		void OnEnable()
		{
			if (logo == null)
			{
				logo = Resources.Load<Texture2D>("QWR/logo");
			}
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
			SettingFileInfo settingFileInfo = JsonUtility.FromJson<SettingFileInfo>(file_info.text);
			settingInfo = settingFileInfo;
            //QwrNoloAndroidConfig.OnNoloSelected(settingInfo.peripheral == QWRPeripheral.Nolo);
        }

        [MenuItem("QWR/Settings")]
		static void Init()
		{
			instance = DisplayWizard<QwrManagerEdit>("QWR Settings");
			instance.position = new Rect(150, 150, 400, 480);
			instance.Show();
		}

		void OnGUI()
		{
			var r = EditorGUILayout.BeginVertical(GUILayout.Height(128));
			EditorGUILayout.Space();

			GUI.DrawTexture(r, logo, ScaleMode.ScaleToFit);
			EditorGUILayout.Space();

			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Qwr Unity Plugin Version: " + QwrManager.QwrUnityVersion, EditorStyles.boldLabel);
			EditorGUILayout.Space();

			#if !UNITY_ANDROID
			EditorGUILayout.BeginHorizontal();
			settingInfo.pcScreenOutputMode = (qwrcOutputMode)EditorGUILayout.EnumPopup("Pc Screen Output Mode", settingInfo.pcScreenOutputMode);
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			#endif

			EditorGUILayout.BeginHorizontal();
			settingInfo.eyeTextureDepth = (TEXTURE_DEPTH)EditorGUILayout.EnumPopup("Eye Texture Depth", settingInfo.eyeTextureDepth);
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			settingInfo.worldScale = EditorGUILayout.FloatField("World Scale", settingInfo.worldScale);
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			settingInfo.minimumVsync = EditorGUILayout.IntField("Minimum Vsync", settingInfo.minimumVsync);
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			settingInfo.resetTrackerOnLoad = EditorGUILayout.Toggle("Reset Tracker On Load", settingInfo.resetTrackerOnLoad);
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			#if !UNITY_ANDROID
			settingInfo.pcEyeTextureScale = EditorGUILayout.Slider("Eye Texture Scale", settingInfo.pcEyeTextureScale, 0.1f, 2.0f);
			#else
			settingInfo.mobileEyeTextureScale = EditorGUILayout.Slider("Eye Texture Scale", settingInfo.mobileEyeTextureScale, 0.1f, 2.0f);
			#endif
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
            settingInfo.peripheral = (QWRPeripheral)EditorGUILayout.EnumPopup("Peripheral Support", settingInfo.peripheral);
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

            #if UNITY_ANDROID
			EditorGUILayout.BeginHorizontal();
			settingInfo.androidEditorUseHmd = EditorGUILayout.Toggle("Android Editor Use Hmd", settingInfo.androidEditorUseHmd);
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
            #endif

            EditorGUILayout.BeginHorizontal();
            settingInfo.useQWRPointer = EditorGUILayout.Toggle("Use QWRPointer", settingInfo.useQWRPointer);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            settingInfo.hmdPointerSwitchable = EditorGUILayout.Toggle("HMD Pointer Switchable", settingInfo.hmdPointerSwitchable);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            settingInfo.touchPosOrig = (TouchPosOrig)EditorGUILayout.EnumPopup("Touch Pos Orig", settingInfo.touchPosOrig);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
			EditorGUI.HelpBox(new Rect(5, 430, 175, 20), "Please Apply Before Playing", MessageType.Info);
			if (GUI.Button(new Rect(205, 430, 95, 20), "Apply"))
			{
				SettingFileInfo settingFileInfo = settingInfo;
				string string_json = JsonUtility.ToJson(settingFileInfo);
				FileInfo fileInfo = new FileInfo(Application.dataPath + "/QWR/Resources/QWR/QWRUnityConfig.json");
				using (StreamWriter sw = fileInfo.CreateText())
				{
					sw.WriteLine(string_json);
					sw.Flush();
					sw.Close();
				}

                //QwrNoloAndroidConfig.OnNoloSelected(settingFileInfo.peripheral == QWRPeripheral.Nolo);
                AssetDatabase.Refresh();
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
	}
}
