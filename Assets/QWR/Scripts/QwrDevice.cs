/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace qwr
{
    public struct PeripheralList
    {
        public QwrPeripheral peripheral;
        public List<QwrBasePeripheral> list;

        public PeripheralList (QwrPeripheral t)
        {
            peripheral = t;
            list = new List<QwrBasePeripheral>();
        }
    }
    
	public class QwrDevice : MonoBehaviour
	{	
		#if UNITY_ANDROID && !UNITY_EDITOR
		static private JavaActivity _java_activity = new JavaActivity();
		#endif

		
		private Buffers _buffers = new Buffers();
		private Composer _composer = new Composer();
        private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private static readonly Dictionary<string, PeripheralList> s_Peripherals = new Dictionary<string, PeripheralList>();

		private const int BUFF_NUM = 2;

		public static QwrDevice _instance;

        public static bool bVR9 = false;
        public static bool bQCOM = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        private string PROPERTY_ISVR9 = "is_vr9";
        private string PROPERTY_ISQCOM = "is_qcom";
#endif

		private qwrnPrediction _prediction;
		
		void OnEnable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            IntPtr tempPtr = Marshal.StringToHGlobalAnsi(PROPERTY_ISVR9);
            bVR9 = Composer.QwruGetIntValue(tempPtr, 0) == 0? false: true;
            IntPtr tempPtrQcom = Marshal.StringToHGlobalAnsi(PROPERTY_ISQCOM);
            bQCOM = Composer.QwruGetIntValue(tempPtrQcom, 0) == 0? false: true;
            Debug.Log("QwrDevice Init bVR9 " + bVR9  + " bQCOM " + bQCOM);
#endif
            ConfigEngine.Init();
            if (!bVR9)
            {
                _buffers.Init
                    (QwrManager.eyeTextureDepth
                    , QwrManager.eyeTextureFormat
                    , QwrManager.DeviceInfo.resolution_x
                    , QwrManager.DeviceInfo.resolution_y);
#if UNITY_ANDROID && !UNITY_EDITOR
#else
            IntPtr[] buf_ptr = new IntPtr[(int)qwrcEYE.NUM]
                    { _buffers.GetEyeTexturePtr( qwrcEYE.LEFT )
                        , _buffers.GetEyeTexturePtr( qwrcEYE.RIGHT ) };
            Composer.SetTextures(buf_ptr[0], qwrcEYE.LEFT);
            Composer.SetTextures(buf_ptr[1], qwrcEYE.RIGHT);
#endif
            }
            //
#if !UNITY_ANDROID
            string pcScreenOutput = "pcScreenOutput";
#if UNITY_EDITOR
			IntPtr OutputPtr = Marshal.StringToHGlobalAnsi(pcScreenOutput);
			Composer.QwruSetIntValue(OutputPtr, (int)qwrcOutputMode.NONE);
			Marshal.FreeHGlobal(OutputPtr);
#else
			IntPtr OutputPtr = Marshal.StringToHGlobalAnsi(pcScreenOutput);
			Composer.QwruSetIntValue(OutputPtr, (int)QwrManager.pcScreenOutputMode);
			Marshal.FreeHGlobal(OutputPtr);
#endif
#endif
            if (QwrManager.IsScriptRenderingPipeline)
            {
#if UNITY_2019_1_OR_NEWER
                RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
#endif
            }
            else
            {
                Camera.onPreRender += CameraPreRender;
                Camera.onPreCull += CameraPreCull;
            }
            
            //cameras
            StartCoroutine(CallbackCoroutine());
		}

        void Start ()
        {
            if (!bVR9)
            {
                QwrCameraRig._instance._left_eye.targetTexture = _instance._buffers.GetEyeTexture(qwrcEYE.LEFT);
                QwrCameraRig._instance._right_eye.targetTexture = _instance._buffers.GetEyeTexture(qwrcEYE.RIGHT);
            }
        }

		void OnDisable()
		{
			IntPtr tempPtr = Marshal.StringToHGlobalAnsi("OnDisable");
			Composer.QwruSetIntValue(tempPtr, 1);

            if (QwrManager.IsScriptRenderingPipeline)
            {
#if UNITY_2019_1_OR_NEWER
                RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
#endif
            }
            else
            {
                Camera.onPreRender -= CameraPreRender;
                Camera.onPreCull -= CameraPreCull;
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR

       static public void InitJavaActivity()
       {
            _java_activity.Init();
       }
#endif

        /// <summary>
        /// Run Qwr HMD
        /// </summary>
        public static void create()
        {
            if (_instance != null)
            {
                _instance.gameObject.SetActive(true);
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
			_java_activity.Init();
#endif
            Composer.Init();

            //create a new DeviceObject
            GameObject device_object = new GameObject
                (Common.QwrDeviceGameObjectName
                    , typeof(QwrDevice));

            if (QwrManager.QWRPointer)
            {
                EventSystem eventSystem = FindObjectOfType<EventSystem>();
                if (eventSystem == null)
                {
                    Debug.LogWarning("QwrEventSystem is needed after version 0.7.5.");
                    GameObject gameObject = new GameObject("QwrEventSystem", typeof(EventSystem));
                    eventSystem = gameObject.GetComponent<EventSystem>();
                }
                {
                    QwrStandaloneInputModule inputModule = eventSystem.GetComponent<QwrStandaloneInputModule>();
                    if(inputModule == null)
                    {
                        Debug.LogWarning("QwrStandaloneInputModule component must be in the EventSystem, after version 0.7.5.");
                        eventSystem.gameObject.AddComponent<QwrStandaloneInputModule>();
                    }

                }
            }

            //Don't destroy it.
            DontDestroyOnLoad(device_object);

            //get device
            _instance = device_object.GetComponent<QwrDevice>();

            _instance.StartCoroutine(_instance._OnResume());
        }
		
		private void OnDestroy()
		{
			StopAllCoroutines();

#if UNITY_ANDROID && !UNITY_EDITOR
			RenderTexture.active = null;
#else
            // do nothing
#endif

            Composer.Uninit();
            if (!bVR9)
            {
                if (QwrCameraRig._instance != null)
                {
                    if (QwrCameraRig._instance._left_eye)
                        QwrCameraRig._instance._left_eye.targetTexture = null;

                    if (QwrCameraRig._instance._right_eye)
                        QwrCameraRig._instance._right_eye.targetTexture = null;

                    if (QwrCameraRig._instance._center_eye)
                        QwrCameraRig._instance._center_eye.targetTexture = null;
                }
                
                _buffers.clear();
            }
        }

		/// <summary>
		/// event functions 
		/// </summary>
		private void CameraPreRender(Camera cam)
		{
		    if (!bVR9)
            {      
                UpdateCameraTexture(cam);
            }
		}

        static int lastFrameCount = -1;
        private void CameraPreCull(Camera cam)
        {
            // Only update poses on the first camera per frame.
            if (Time.frameCount != lastFrameCount)
            {
                lastFrameCount = Time.frameCount;
                DeviceUpdate();
                foreach (KeyValuePair<string, PeripheralList> i in s_Peripherals)
                {
                    if (i.Value.list.Count > 0)
                    {
                        foreach (QwrBasePeripheral s in i.Value.list)
                        {
                            s.QwrpUpdate();
                        }
                    }
                }
            }
        }
        
		private void DeviceUpdate()
		{
		    if (!bVR9)
            {      
			    if (false == _buffers.SwapBuffers())
			    {
#if UNITY_ANDROID && !UNITY_EDITOR
#else
				    Debug.Log("QWR texture is recreated.");
				    IntPtr[] buf_ptr = new IntPtr[(int)qwrcEYE.NUM]
				    { _buffers.GetEyeTexturePtr( qwrcEYE.LEFT )
					    , _buffers.GetEyeTexturePtr( qwrcEYE.RIGHT ) };
                    Composer.SetTextures(buf_ptr[0], qwrcEYE.LEFT);
                    Composer.SetTextures(buf_ptr[1], qwrcEYE.RIGHT);
#endif
			    }
            }
        }

        private void UpdateCameraTexture(Camera cam)
        {
            if (cam == QwrCameraRig._instance._left_eye)
            {
                cam.targetTexture = _instance._buffers.GetEyeTexture(qwrcEYE.LEFT);
            }
            if (cam == QwrCameraRig._instance._right_eye)
            {
                cam.targetTexture = _instance._buffers.GetEyeTexture(qwrcEYE.RIGHT);
            }
        }

        private IEnumerator CallbackCoroutine()
		{
			while (true)
			{
				yield return waitForEndOfFrame;
				if( null != QwrCameraRig._instance)
				{
                    QwrCameraRig._instance.UpdatePose();
                    _composer.Compose();
#if UNITY_ANDROID && !UNITY_EDITOR
                    HideSplash();
#endif
                }
			}
		}

        private bool hasSplash = true;

#if UNITY_ANDROID && !UNITY_EDITOR

        private void HideSplash()
        {
            if (!hasSplash)
                return;

            Debug.Log("HideSplash");
            if (Application.platform.Equals(RuntimePlatform.Android))
            {
                using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        hasSplash = false;
                        jo.Call("HideSplash");
                    }
                }
            }
        }
#endif

		private void OnApplicationPause(bool pause)
		{
            if (pause)
            {
                foreach (KeyValuePair<string, PeripheralList> i in s_Peripherals)
                {
                    if (i.Value.list.Count > 0)
                    {
                        foreach (QwrBasePeripheral s in i.Value.list)
                        {
                            s.QwrpPause();
                        }
                    }
                }
                Composer.Pause();
            }
            else
                StartCoroutine(_OnResume());
        }

        private void OnApplicationFocus(bool focus)
		{
			// OnApplicationFocus() does not appear to be called 
			// consistently while OnApplicationPause is. Moved
			// functionality to OnApplicationPause().
		}
 
		//triggered by OnApplicationPause and OnApplicationFocus
		private IEnumerator _OnResume()
		{
			yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
           
            Composer.Resume();
            foreach (KeyValuePair<string, PeripheralList> i in s_Peripherals)
            {
                if (i.Value.list.Count > 0)
                {
                    foreach (QwrBasePeripheral s in i.Value.list)
                    {
                        s.QwrpResume();
                    }
                }
            }
        }

        public static QwrPeripheral OpenPeripheral(string deviceId, QwrBasePeripheral basePeripheral)
        {
            if (deviceId == null)
            {
                return null;
            }
            if (s_Peripherals.ContainsKey(deviceId))
            {
                if (s_Peripherals[deviceId].list.Contains(basePeripheral))
                {
                    return basePeripheral.peripheral;
                }
                else
                {
                    basePeripheral.peripheral = s_Peripherals[deviceId].peripheral;
                    s_Peripherals[deviceId].list.Add(basePeripheral);
                    return basePeripheral.peripheral;
                }
            }
            QwrPeripheral temp = QwrPeripheral.OpenPeripheralDevice(deviceId);
            if (temp == null)
            {
                return null;
            }
            basePeripheral.peripheral = temp;
            s_Peripherals.Add(deviceId, new PeripheralList(temp));
            s_Peripherals[deviceId].list.Add(basePeripheral);
            return basePeripheral.peripheral;
        }

        public static Dictionary<string, PeripheralList> GetPeripherals()
        {
            return s_Peripherals;
        }

        public static void ClosePeripheral(QwrBasePeripheral basePeripheral)
        {
            if (!s_Peripherals[basePeripheral.peripheral._deviceId].list.Contains(basePeripheral))
            {
                return;
            }
            s_Peripherals[basePeripheral.peripheral._deviceId].list.Remove(basePeripheral);
            if (s_Peripherals[basePeripheral.peripheral._deviceId].list.Count == 0)
            {
                QwrPeripheral.ClosePeripheralDevice(basePeripheral.peripheral);
                s_Peripherals.Remove(basePeripheral.peripheral._deviceId);
            }
            basePeripheral.peripheral = null;
            return;
        }


#if UNITY_2019_1_OR_NEWER
        void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            CameraPreCull(camera);
            CameraPreRender(camera);
        }
#endif
    }


}
