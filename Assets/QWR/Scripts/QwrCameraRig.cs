/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace qwr
{
    /// <summary>
    /// Main entry point for the QwrCameraRig API.
    /// </summary>
    /// <seealso cref="qwr.QwrBasePeripheralDevicetype" />
    public class QwrCameraRig : QwrBasePeripheralDevicetype
    {
        public Camera _left_eye { get; private set; }
		public Camera _right_eye { get; private set; }
		public Camera _center_eye { get; private set; }
		public Transform _left_transform { get; private set; }
		public Transform _right_transform { get; private set; }
		public Transform _center_transform { get; private set; }
		public Transform _tracker_transform { get; private set; }
		public Transform _tracking_space { get; private set; }

        public static QwrCameraRig _instance = null;
        public static QwrCameraRig Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = FindObjectOfType<QwrCameraRig>();
                }
                return _instance;
            }
        }

        private Sensor _sensor = new Sensor();
        private bool _freezed = false;
        private bool _monoscopic = false;

     
        public void Freeze(bool enabled) { _freezed = enabled; }
        public bool GetFreezed() { return _freezed; }

        public void MonoScopic(bool enabled) { _monoscopic = enabled; }
        public bool GetMonoScopic() { return _monoscopic; }

        private qwrQuarterion pose;
        private qwrVector3 position;
        public Quaternion GetPose() { return pose.ToQuaternion(); }
        public Vector3 GetPosition() { return new Vector3(position.x, position.y, position.z); }

        static public bool VRsupport;
        static private bool CameraRigInit = false;

       
        public Transform Polaris;
        public Transform Nolo;
        public Transform Flip;
       

        public Transform Boundary;

        public QwrCameraRig()
        {
            _instance = this;
            IsRecentering = false;
        }

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public override void OnEnable()
        {
            if (!CameraRigInit)
            {
                if (_instance != this && _instance != null)
                {
                    Debug.LogWarning("There is another active QwrCameraRig in a scene, set it unactive");
                    _instance.gameObject.SetActive(false);
                }

                _instance = this;
            }

            // version adaptation : Unity 2018
#if UNITY_ANDROID && (!UNITY_EDITOR) && UNITY_2018
            InitComponent(this.transform);
            StartCoroutine(InitPeripheral_Coroutine());
#else
            InitPeripheral();
#endif

#if !UNITY_EDITOR
            StartCoroutine(_WaitForEndOfFrame());
#endif
        }

        static public bool IsRecentering
        {
            get;
            protected set;
        }

        /// <summary>
        /// Recenters this instance.
        /// </summary>
        public void Recenter()
        {
            ClearRenderTexture(_left_eye.targetTexture, Color.black);
            ClearRenderTexture(_right_eye.targetTexture, Color.black);
            IsRecentering = true;
            _sensor.RecenterPose();

            if (onRecenter != null)
                onRecenter();

            OnEndOfThisFrame(delegate 
            {
                IsRecentering = false;
            });
        }

        static public Action onRecenter;

        /// <summary>
        /// Disables the base peripheral.
        /// invoked by Unity Engine
        /// </summary>
        public override void OnDisable ()
		{
            IntPtr tempPtr = Marshal.StringToHGlobalAnsi("OnDisable");
            Composer.QwruSetIntValue(tempPtr, 1);
            base.OnDisable();
            if (_instance == this)
			{
                _instance = null;
			}

            {
                if (_left_eye)
                    _left_eye.targetTexture = null;

                if (_right_eye)
                    _right_eye.targetTexture = null;

                if (_center_eye)
                    _center_eye.targetTexture = null;
            }
		}

        private bool _initialized = false;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            if (_initialized)
            {
                if (VRsupport
#if UNITY_ANDROID && UNITY_EDITOR
                || !QwrManager.androidEditorUseHmd
#endif
                )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            InitComponent(this.transform);
            if (VRsupport
#if UNITY_ANDROID && UNITY_EDITOR
				|| !QwrManager.androidEditorUseHmd
#endif
                )
            {
                _left_eye.enabled = false;
                _right_eye.enabled = false;
                _center_eye.enabled = true;
                return false;
            }
            else
            {
                _left_eye.enabled = true;
                _right_eye.enabled = true;
                _center_eye.enabled = false;
            }
#if UNITY_EDITOR
            _center_eye.enabled = true;
#endif

            Freeze(false);
            MonoScopic(false);

            _initialized = true;
            return true;
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public void InitComponent(Transform parent)
		{
			if( null == _tracking_space )
				_tracking_space = EntityEnsure( parent , "TrackingSpace" );
			
			if( null == _left_transform )
				_left_transform = EntityEnsure( _tracking_space , "LeftEyeAnchor" );
			
			if( null == _right_transform )
				_right_transform = EntityEnsure( _tracking_space , "RightEyeAnchor" );
			
			if( null == _center_transform )
				_center_transform = EntityEnsure( _tracking_space , "CenterEyeAnchor" );
			
			if( null == _tracker_transform )
				_tracker_transform = EntityEnsure( _tracking_space , "TrackerAnchor" );
			
			if (_left_eye == null)
				_left_eye = CameraEnsure( _left_transform, true );
			
			if (_right_eye == null)
				_right_eye = CameraEnsure(_right_transform, true);

			if (_center_eye == null)
				_center_eye = CameraEnsure(_center_transform, false);

            if (_left_transform != null)
                _left_transform.localPosition = _sensor.GetEyePose(qwrcEYE.LEFT, new qwrQuarterion(1, 0, 0, 0), new qwrVector3(0, 0, 0), QwrManager.DeviceInfo.ipd).position;
            if (_left_transform != null)
                _right_transform.localPosition = _sensor.GetEyePose(qwrcEYE.RIGHT, new qwrQuarterion(1, 0, 0, 0), new qwrVector3(0, 0, 0), QwrManager.DeviceInfo.ipd).position;
        }

        /// <summary>
        /// Updates the state of the device.
        /// </summary>
        public override void QwrpUpdate()
        {
            base.QwrpUpdate();
            double displayTime = Composer.QwruGetPredictedDisplayTime(QwrManager.minimumVsync);
            pose = Composer.QwruGetPredictedPose(displayTime); // Right-handed spiral, left-handed (Inertial coordinate system)
            if (false) // (QwrManager.peripheral == QWRPeripheral.Polaris)
            {
                float[] temp_position = QwrpGetDeviceCurrentStatus().position_state[0];
                position = new qwrVector3(temp_position[0], temp_position[1], temp_position[2]);
                position.z = -position.z;
            }
#if false && UNITY_ANDROID && !UNITY_EDITOR
            else if (QwrManager.peripheral == QWRPeripheral.Nolo && NoloController._instance[(int)NoloController.NoloDevice.Nolo_Hmd] != null)
            {
                Vector3 temp_position = NoloController._instance[(int)NoloController.NoloDevice.Nolo_Hmd].transform.localPosition;
                position = new qwrVector3(temp_position.x, temp_position.y, temp_position.z);
            }
#endif
            else
            {
                position = Composer.QwruGetPredictedPosition(displayTime);
                position.z = -position.z;
            }


            position.x = position.x * QwrManager.worldScale;
            position.y = position.y * QwrManager.worldScale;
            position.z = position.z * QwrManager.worldScale;

            _tracking_space.localRotation = pose.ToQuaternion();
            
            // Left-handed spiral, left-handed (Unity coordinates) 

            UpdatePeripheral();
        }

        /// <summary>
        /// Updates the pose.
        /// </summary>
        public void UpdatePose()
        {
            Composer.UpdatePose(pose, position);
        }


        public void _Update
            (Pose left_pose, Pose right_pose
             , bool monoscopic
             , bool freezed
             )
        {
            //entities
            if (false == freezed)
            {
                _left_transform.localRotation = left_pose.orientation;
                _right_transform.localRotation = monoscopic ? left_pose.orientation : right_pose.orientation; // using left eye for now
                _center_transform.localRotation = left_pose.orientation;
                _tracker_transform.localRotation = _center_transform.localRotation;

                Vector3 pos = 0.5f * (left_pose.position + right_pose.position);
                _left_transform.localPosition = monoscopic ? pos : left_pose.position;
                _right_transform.localPosition = monoscopic ? pos : right_pose.position;
                _center_transform.localPosition = pos;
                _tracker_transform.localPosition = _center_transform.localPosition;
            }
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        /// <param name="fovy">The fovy.</param>
        /// <param name="aspect_xdy">The aspect xdy.</param>
        public void _UpdateCam(float fovy, float aspect_xdy)
		{
			CameraSetup(_left_eye, PLUGIN_EVENT_TYPE.LeftEyeEndFrame
						, fovy, aspect_xdy);
			CameraSetup(_right_eye, PLUGIN_EVENT_TYPE.RightEyeEndFrame
						, fovy, aspect_xdy);

            CameraSetup(_center_eye, 0, fovy, aspect_xdy);
		}
		
		private static Transform EntityEnsure( Transform parent , string name )
		{
			Transform entity = parent.Find( name );
			if( entity == null )
				entity = new GameObject( name ).transform;
			
			entity.name = name;
			entity.parent = parent;
			entity.localScale = Vector3.one;
			entity.localPosition = Vector3.zero;
			entity.localRotation = Quaternion.identity;
			
			return entity;
		}
		
		private static Camera CameraEnsure( Transform entity, bool render)
		{
			Camera cam = entity.GetComponent<Camera>();
			if( cam == null )
			{
				cam = entity.gameObject.AddComponent<Camera>();
			}

			// AA is documented to have no effect in deferred, but it causes black screens.
			if (cam.actualRenderingPath == RenderingPath.DeferredLighting ||
				cam.actualRenderingPath == RenderingPath.DeferredShading)
			{
				QualitySettings.antiAliasing = 0;
			}

			if (render && cam.GetComponent<QwrPostRender>() == null && !VRsupport)
			{
				cam.gameObject.AddComponent<QwrPostRender>();
			}
			return cam;
		}
		
		private static void CameraSetup
			( Camera cam , PLUGIN_EVENT_TYPE event_type
			 , float fovy , float aspect_xdy)
		{
			
			cam.fieldOfView = fovy;
			cam.aspect = aspect_xdy;
			if (QwrDevice.bVR9)
            {
                if (event_type == PLUGIN_EVENT_TYPE.LeftEyeEndFrame)
                {
                    cam.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
                }
                else if (event_type == PLUGIN_EVENT_TYPE.RightEyeEndFrame)
                {
                    cam.rect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
                }
                else
                {
                    cam.rect = new Rect(0f, 0f, 1f, 1f);
                }
            }
            else
            {
                cam.rect = new Rect(0f, 0f, 1f, 1f);
            }
			
			// Enforce camera render order
			cam.depth = (int)event_type;

			//// AA is documented to have no effect in deferred, but it causes black screens.
			//if (cam.actualRenderingPath == RenderingPath.DeferredLighting ||
			//    cam.actualRenderingPath == RenderingPath.DeferredShading)
			//{
			//    QualitySettings.antiAliasing = 0;
			//    QwrManager.instance.eyeTextureAntiAliasing = TEXTURE_ANTIALIASING._1;
			//}
		}

        public override void QwrpPause()
        {
        }

        //triggered by OnApplicationPause and OnApplicationFocus
        public override void QwrpResume()
        {
            //SetPeripheralFollowSystem();
            StartCoroutine(SetPeripheralFollowSystem_Delay());
        }

        // hmd Peripheral
        private QwrBasePeripheral _hmdPeripheral = null;
        // current Peripheral
        private QwrBasePeripheral _currentPeripheral = null;
        // default Peripheral by setting
        private QwrBasePeripheral _defaultPeripheral = null;

        public QwrBasePeripheral CurrentPeripheral
        {
            get
            {
                return _currentPeripheral;
            }
        }
        /// <summary>
        /// Sets the peripheral.
        /// </summary>
        /// <param name="peripheral">The peripheral.</param>
        public void SetPeripheral(QwrBasePeripheral peripheral)
        {
            if (_currentPeripheral)
            {
                _currentPeripheral.EnableInternalObjects(false);
            }

            _currentPeripheral = peripheral;

            if (_currentPeripheral)
            {
                _currentPeripheral.EnableInternalObjects(true);
            }

            if (onPeripheralChanged != null)
                onPeripheralChanged(_currentPeripheral);
        }

        static public Action<QwrBasePeripheral> onPeripheralChanged;

        /// <summary>
        /// Gets a value indicating whether [follow system].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [follow system]; otherwise, <c>false</c>.
        /// </value>
        static public bool followSystem
        {
            get
            {
                return _instance._interativeType != -1;
            }
        }
        private int _interativeType = -1;

        void Start()
        {
            SetPeripheralFollowSystem();
#if UNITY_ANDROID && (UNITY_5_5_0 || UNITY_5_4_3)
            // In Unity 5.5.0 and Unity 5.4.3,
            // surface is deleted and rebuilt after the first frame is completed by Unity, the second frame will be black screen.
            // So, Skip the rendering of the first frame to avoid flickering.
            StartCoroutine(Coroutine_EnableCamera());
#endif
        }

        int GetInteractiveType()
        {
            if (_defaultPeripheral == null || _defaultPeripheral.peripheral == null)
                return -1;

            if (_defaultPeripheral.PeripheralType != QWRPeripheral.Flip)
                return -1;

            IntPtr buffer = Marshal.AllocHGlobal(sizeof(int));
            int ret = _defaultPeripheral.peripheral.QwrupReadDeviceAttribute(QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE_INTERACTIVE_TYPE - QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE
            , buffer, sizeof(int));
            int type = -1;
            if (ret == 1)
            {
                int[] value = new int[1];
                Marshal.Copy(buffer, value, 0, 1);
                type = value[0];
            }

            Marshal.FreeHGlobal(buffer);
            return type;
        }

        void SetInterctiveType(int type)
        {
            if (_defaultPeripheral == null || _defaultPeripheral.peripheral == null)
                return;

            if (_defaultPeripheral.PeripheralType != QWRPeripheral.Flip)
                return;

            if (type != 0 && type != 1)
                return;

            IntPtr buffer = Marshal.AllocHGlobal(sizeof(int));
            int[] values = new int[1];
            values[0] = type;
            Marshal.Copy(values, 0, buffer, 1);

            _defaultPeripheral.peripheral.QwrupSetDeviceAttribute(QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE_INTERACTIVE_TYPE - QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE
                , buffer, sizeof(int));
            Marshal.FreeHGlobal(buffer);
        }

        private void SetPeripheralFollowSystem()
        {
            if (_defaultPeripheral == null)
                return;

            if (_defaultPeripheral.PeripheralType != QWRPeripheral.Flip)
                return;

            _interativeType = GetInteractiveType();
            
            if (_interativeType == 0)
            {
                SetPeripheral(_hmdPeripheral);
            }
            else if (_interativeType == 1)
            {
                SetPeripheral(_defaultPeripheral);
            }
        }

        /// <summary>
        /// Enables the internal objects.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        public override void EnableInternalObjects(bool enabled)
        {
            if(QwrPointerManager.Instance!=null)
              QwrPointerManager.Instance.ray.SetVisible(false);
        }

        void UpdatePeripheral()
        {
            if (_defaultPeripheral.PeripheralType == QWRPeripheral.Flip
                && (!QwrCameraRig.followSystem))
                return;

            if (_defaultPeripheral && _defaultPeripheral.IsActivating())
            {
                // peripheral
                if (_defaultPeripheral != _currentPeripheral)
                {
                    SetPeripheral(_defaultPeripheral);
                    SetInterctiveType(1);
                }
            }
            else if (QwrManager.hmdPointerSwitchable)
            {
                // touch pad
                int count = Input.touchCount;
                if (count != 0)
                {
                    if (_currentPeripheral != _hmdPeripheral)
                    {
                        SetPeripheral(_hmdPeripheral);
                        SetInterctiveType(0);
                    }
                }
            }
        }

        void OnPeripheralConnected(QwrBasePeripheral peripheral)
        {
            if (QwrCameraRig.followSystem)
                SetPeripheralFollowSystem();
            else
                SetPeripheral(peripheral);
        }

        void OnPeripheralDisconnected(QwrBasePeripheral peripheral)
        {
            if (QwrCameraRig.followSystem)
                SetPeripheralFollowSystem();
            else
                SetPeripheral(_hmdPeripheral);
        }

        void OnPeripheralUnbind(QwrBasePeripheral peripheral)
        {
            SetInterctiveType(0);
            SetPeripheral(_hmdPeripheral);
        }
        
        IEnumerator SetPeripheralFollowSystem_Delay()
        {
            yield return new WaitForSeconds(1.0f);
            SetPeripheralFollowSystem();
        }

        void EnableCamera(bool enabled)
        {
            if (_left_eye)
                _left_eye.enabled = enabled;

            if (_right_eye)
                _right_eye.enabled = enabled;

#if UNITY_EDITOR
            if (_center_eye)
                _center_eye.enabled = enabled;
#endif
        }

#if UNITY_ANDROID && (UNITY_5_5_0 || UNITY_5_4_3)

        IEnumerator Coroutine_EnableCamera()
        {
            EnableCamera(false);

            yield return new WaitForEndOfFrame();

            EnableCamera(true);
        }
#endif

        void InitPeripheral()
        {
            if (!CameraRigInit)
            {
#if UNITY_5_3_OR_NEWER || UNITY_5
                VRsupport = UnityEngine.XR.XRSettings.enabled;
#else
			    VRsupport = false;
#endif
            }
            CameraRigInit = true;
            if (!Init())
            {
                return;
            }
            base.OnEnable();
            _UpdateCam(QwrManager.DeviceInfo.fov_y
                , QwrManager.DeviceInfo.fov_x / (float)QwrManager.DeviceInfo.fov_y);

            _hmdPeripheral = this;
            _hmdPeripheral.EnableInternalObjects(false);

            switch (QwrManager.peripheral)
            {
#if false
                case QWRPeripheral.Nolo:
                    {
                        Transform Peripheral = Instantiate(Nolo);
                        Peripheral.parent = this.transform;

                        Transform boundary = Instantiate(Boundary);
                        boundary.parent = this.transform;

                        _defaultPeripheral = Peripheral.GetComponent<QwrNoloController>();
                        _defaultPeripheral.EnableInternalObjects(false);
                        break;
                    }
#endif
                case QWRPeripheral.Flip:
                    {
                        Transform Peripheral = Instantiate(Flip);
                        Peripheral.parent = this.transform;
                        Peripheral.localPosition = Vector3.zero;

                        Transform controller_right = Peripheral.Find("controller(right)");
                        if (controller_right == null)
                            break;

                        _defaultPeripheral = controller_right.GetComponent<QwrDaydreamController>();
                        _defaultPeripheral.EnableInternalObjects(false);

                        QwrDaydreamController.onUnbind += OnPeripheralUnbind;
                        break;
                    }
                case QWRPeripheral.None:
                    {
                        _defaultPeripheral = _hmdPeripheral;
                        break;
                    }
                default:
                    break;
            }

            SetPeripheral(_defaultPeripheral);
        }

#if UNITY_ANDROID && (!UNITY_EDITOR) && UNITY_2018 

        IEnumerator InitPeripheral_Coroutine()
        {
            // Wait two frames until the Unity3d initialization is complete.
            EnableCamera(false);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            EnableCamera(true);

            InitPeripheral();

            SetPeripheralFollowSystem();
        }

#endif

        static public Vector3 WorldToScreenPoint(Vector3 position)
        {
            if (_instance && _instance._center_eye)
                return _instance._center_eye.WorldToScreenPoint(position);
            else
                return Vector3.zero;
        }

#if !UNITY_EDITOR
        IEnumerator _WaitForEndOfFrame()
        {
            while(true)
            {
                yield return new WaitForEndOfFrame();
                OnEndOfFrame();
            }
        }

        void LateUpdate()
        {
            if(_center_eye)
                _center_eye.enabled = false;
        }

        void OnEndOfFrame()
        {
            if(_center_eye)
                _center_eye.enabled = true;
        }
#endif
        static public void ClearRenderTexture(RenderTexture target, Color color)
        {
            CommandBuffer cmdbuf = new CommandBuffer();
            cmdbuf.SetRenderTarget(target);
            cmdbuf.ClearRenderTarget(true, true, color);
            Graphics.ExecuteCommandBuffer(cmdbuf);
        }

        void OnEndOfThisFrame(Action action)
        {
            StartCoroutine(_OnEndOfFrame(action));
        }

        IEnumerator _OnEndOfFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            if (action != null)
                action.Invoke();
        }
    }

}
