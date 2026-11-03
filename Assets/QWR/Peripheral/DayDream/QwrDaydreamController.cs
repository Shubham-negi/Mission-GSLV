/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace qwr
{
    /// <summary>
    /// Represents the controller's current connection state.
    /// </summary>
    public enum QwrConnectionState
    {
        /// <summary>
        /// Indicates that an error has occurred.
        /// </summary>
        Error = -1,

        /// <summary>
        /// Indicates that the controller is disconnected.
        /// </summary>
        Disconnected = 0,
        /// <summary>
        /// Indicates that the device is scanning for controllers.
        /// </summary>
        Scanning = 1,
        /// <summary>
        /// Indicates that the device is connecting to a controller.
        /// </summary>
        Connecting = 2,
        /// <summary>
        /// Indicates that the device is connected to a controller.
        /// </summary>
        Connected = 3,

        /// <summary>
        /// Indicates that the device is bond to a controller.
        /// </summary>
        Bond = 4,
        /// <summary>
        /// Indicates that the controller is unbond.
        /// </summary>
        Unbond = 5,
    };

    /// <summary>
    /// Represents the API status of the current controller state.
    /// </summary>
    public enum QwrControllerApiStatus
    {
        /// <summary>
        /// A Unity-localized error occurred.
        /// </summary>
        Error = -1,

        /// <summary>
        ///API is happy and healthy. This doesn't mean the controller itself
        ///is connected, it just means that the underlying service is working
        ///properly.
        /// </summary>
        Ok = 0,

        /// <summary>
        /// Any other status represents a permanent failure that requires
        /// external action to fix:

        /// API failed because this device does not support controllers (API is too
        /// low, or other required feature not present).
        /// </summary>
        Unsupported = 1,
        /// <summary>
        /// This app was not authorized to use the service (e.g., missing permissions,
        /// the app is blacklisted by the underlying service, etc).
        /// </summary>
        NotAuthorized = 2,
        /// <summary>
        /// The underlying VR service is not present.
        /// </summary>
        Unavailable = 3,
        /// <summary>
        /// The underlying VR service is too old, needs upgrade.
        /// </summary>
        ApiServiceObsolete = 4,
        /// <summary>
        /// The underlying VR service is too new, is incompatible with current client.
        /// </summary>
        ApiClientObsolete = 5,
        /// <summary>
        /// The underlying VR service is malfunctioning. Try again later.
        /// </summary>
        ApiMalfunction = 6,
    };



    /// <summary>
    /// Main entry point for the Daydream controller API.
    /// To use this API, add this behavior to a GameObject in your scene, or use the
    /// QwrControllerMain prefab. There can only be one object with this behavior on your scene.
    /// This is a singleton object.
    /// To access the controller state, simply read the static properties of this class. For example,
    /// to know the controller's current orientation, use QwrController.Orientation.
    /// </summary>
    public class QwrDaydreamController : QwrBasePeripheral
    {
        public QwrDaydreamController()
        {
            PeripheralType = QWRPeripheral.Flip;
        }

        private static QwrDaydreamController instance = null;

        [Tooltip("If not set, relative to parent")]
        public Transform origin;

        public Transform model;

        private QwrDaydreamControllerState controllerState = new QwrDaydreamControllerState();
        private QwrDaydreamControllerState lastcontrollerState = new QwrDaydreamControllerState();

        //private Transform Pointer;

        static private Vector3 controllerLocalPosition = new Vector3(0.0f, 0.0019f, -0.00365f);

        private Vector2 _touchUpPos;


        [SerializeField]
        private Qwr3DofArmModel armModel;

        /// <summary>
        /// Returns the controller's current connection state.
        /// </summary>
        public static QwrConnectionState State
        {
            get
            {
                return instance != null ? instance.controllerState.connectionState : QwrConnectionState.Error;
            }
        }

        /// <summary>
        /// Returns the API status of the current controller state.
        /// </summary>
        public static QwrControllerApiStatus ApiStatus
        {
            get
            {
                return instance != null ? instance.controllerState.apiStatus : QwrControllerApiStatus.Error;
            }
        }

        /// <summary>
        /// Returns the controller's current orientation in space, as a quaternion.
        /// The space in which the orientation is represented is the usual Unity space, with
        /// X pointing to the right, Y pointing up and Z pointing forward. Therefore, to make an
        /// object in your scene have the same orientation as the controller, simply assign this
        /// quaternion to the GameObject's transform.rotation.
        /// </summary>
        public static Quaternion Orientation
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.orientation : Quaternion.identity;
            }
        }

        /// <summary>
        /// Returns the controller's gyroscope reading. The gyroscope indicates the angular
        /// about each of its local axes. The controller's axes are: X points to the right,
        /// Y points perpendicularly up from the controller's top surface and Z lies
        /// along the controller's body, pointing towards the front. The angular speed is given
        /// in radians per second, using the right-hand rule (positive means a right-hand rotation
        /// about the given axis).
        /// </summary>
        public static Vector3 Gyro
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.gyro : Vector3.zero;
            }
        }





        /// <summary>
        /// Gets the accel.
        /// Returns the controller's accelerometer reading. The accelerometer indicates the
        /// effect of acceleration and gravity in the direction of each of the controller's local
        /// axes. The controller's local axes are: X points to the right, Y points perpendicularly
        /// up from the controller's top surface and Z lies along the controller's body, pointing
        /// towards the front. The acceleration is measured in meters per second squared. Note that
        /// gravity is combined with acceleration, so when the controller is resting on a table top,
        /// it will measure an acceleration of 9.8 m/s^2 on the Y axis. The accelerometer reading
        /// will be zero on all three axes only if the controller is in free fall, or if the user
        /// is in a zero gravity environment like a space station.
        /// </summary>
        public static Vector3 Accel
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.accel : Vector3.zero;
            }
        }

        /// <summary>
        /// If true, the user is currently touching the controller's touchpad.
        /// </summary>
        public static bool IsTouching
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.isTouching : false;
            }
        }

        /// <summary>
        /// If true, the user just started touching the touchpad. This is an event flag (it is true
        /// for only one frame after the event happens, then reverts to false).
        /// </summary>
        public static bool TouchDown
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.touchDown : false;
            }
        }

        /// <summary>
        /// If true, the user just stopped touching the touchpad. This is an event flag (it is true
        /// for only one frame after the event happens, then reverts to false).
        /// </summary>
        public static bool TouchUp
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.touchUp : false;
            }
        }

        /// <summary>
        /// Gets touch position.
        /// </summary>
        public static Vector2 TouchPos
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.touchPos : Vector2.zero;
            }
        }

        /// <summary>
        /// Gets the position where we touch up from the touchpad.
        /// </summary>
        public static Vector2 TouchUpPos
        {
            get
            {
                return instance != null && instance.isValid ? instance._touchUpPos : Vector2.zero;
            }
        }

        /// <summary>
        /// If true, the user is starting to perform the recentering gesture. 
        /// </summary>
        public static bool RecenterBegin
        {
            get
            {
                return instance && instance.isValid && instance.controllerState.recenterBegin;
            }
        }
        /// <summary>
        /// If true, the user is currently performing the recentering gesture. Most apps will want
        /// to pause the interaction while this remains true.
        /// </summary>
        public static bool Recentering
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.recentering : false;
            }
        }

        /// <summary>
        /// If true, the user just completed the recenter gesture. The controller's orientation is
        /// now being reported in the new recentered coordinate system (the controller's orientation
        /// when recentering was completed was remapped to mean "forward"). This is an event flag
        /// (it is true for only one frame after the event happens, then reverts to false).
        /// The headset is recentered together with the controller.
        /// </summary>
        public static bool Recentered
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.recentered : false;
            }
        }

        /// <summary>
        /// If true,the recenter gesture is indicated to be canceled.
        /// </summary>
        public static bool RecenterCancel
        {
            get
            {
                return instance && instance.isValid && instance.controllerState.recenterCancel;
            }
        }

        /// <summary>
        /// If true, the click button (touchpad button) is currently being pressed. This is not
        /// an event: it represents the button's state (it remains true while the button is being
        /// pressed).
        /// </summary>
        public static bool ClickButton
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.clickButtonState : false;
            }
        }

        /// <summary>
        /// If true, the click button (touchpad button) was just pressed. This is an event flag:
        /// it will be true for only one frame after the event happens.
        /// </summary>
        public static bool ClickButtonDown
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.clickButtonDown : false;
            }
        }

        /// <summary>
        /// If true, the click button (touchpad button) was just released. This is an event flag:
        /// it will be true for only one frame after the event happens.
        /// </summary>
        public static bool ClickButtonUp
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.clickButtonUp : false;
            }
        }

        /*
		/// If true, the app button (touchpad button) is currently being pressed. This is not
		/// an event: it represents the button's state (it remains true while the button is being
		/// pressed).
		public static bool AppButton
		{
			get
			{
				return instance != null ? instance.controllerState.appButtonState : false;
			}
		}

		/// If true, the app button was just pressed. This is an event flag: it will be true for
		/// only one frame after the event happens.
		public static bool AppButtonDown
		{
			get
			{
				return instance != null ? instance.controllerState.appButtonDown : false;
			}
		}

		/// If true, the app button was just released. This is an event flag: it will be true for
		/// only one frame after the event happens.
		public static bool AppButtonUp
		{
			get
			{
				return instance != null ? instance.controllerState.appButtonUp : false;
			}
		}
        */

        /// <summary>
        /// If true, the trigger button is currently being pressed. This is not an event: 
        /// it represents the button's state (it remains true while the button is being pressed).
        /// It is not supported on normal daydream controller.
        /// </summary>
        public static bool TriggerButton
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.triggerButtonState : false;
            }
        }

        /// <summary>
        /// If true, the trigger button was just pressed. This is an event flag: it will be true for
        /// only one frame after the event happens.
        /// </summary>
        public static bool TriggerButtonDown
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.triggerButtonDown : false;
            }
        }

        /// <summary>
        /// If true, the trigger button was just released. This is an event flag: it will be true for
        /// only one frame after the event happens.
        /// </summary>
        public static bool TriggerButtonUp
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.triggerButtonUp : false;
            }
        }
        /*
        /// <summary>
        /// If true, the volumeUp button (touchpad button) is currently being pressed. This is not
        /// an event: it represents the button's state (it remains true while the button is being
        /// pressed).
        /// </summary>
        public static bool volumeUpButton
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.volumeUpButtonState : false;
            }
        }

        /// If true, the volumeUp button was just pressed. This is an event flag: it will be true for
        /// only one frame after the event happens.
        public static bool volumeUpButtonDown
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.volumeUpButtonDown : false;
            }
        }

        /// If true, the volumeUp button was just released. This is an event flag: it will be true for
        /// only one frame after the event happens.
        public static bool volumeUpButtonUp
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.volumeUpButtonUp : false;
            }
        }

        /// If true, the volumeDown button (touchpad button) is currently being pressed. This is not
        /// an event: it represents the button's state (it remains true while the button is being
        /// pressed).
        public static bool volumeDownButton
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.volumeDownButtonState : false;
            }
        }

        /// If true, the volumeDown button was just pressed. This is an event flag: it will be true for
        /// only one frame after the event happens.
        public static bool volumeDownButtonDown
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.volumeDownButtonDown : false;
            }
        }

        /// <summary>
        /// If true, the volumeDown button was just released. This is an event flag: it will be true for
        /// only one frame after the event happens.
        /// </summary>
        public static bool volumeDownButtonUp
        {
            get
            {
                return instance != null && instance.isValid ? instance.controllerState.volumeDownButtonUp : false;
            }
        } */

        /// <summary>
        /// It shows the value of the Battery Power.
        /// </summary>
        public static int BatteryPower
        {
            get
            {
                if (instance == null)
                    return 100;
                else
                    return instance.controllerState.batteryPower;
            }
        }

        /// <summary>
        /// If State == QwrConnectionState.Error, this contains details about the error.
        /// </summary>
        public static string ErrorDetails
        {
            get
            {
                if (instance != null)
                {
                    return instance.controllerState.connectionState == QwrConnectionState.Error ?
                           instance.controllerState.errorDetails : "";
                }
                else
                {
                    return "QwrController instance not found in scene. It may be missing, or it might "
                           + "not have initialized yet.";
                }
            }
        }

        /// <summary>
        /// Enables this controller.
        /// </summary>
        public void OnEnable()
        {
            if (instance != null)
            {
                Debug.LogError("More than one QwrController instance was found in your scene. "
                       + "Ensure that there is only one QwrController.");
                instance.enabled = false;
            }
            //string deviceName = "WiseVision_DayDream_Controller";
            string deviceName = null;
            if (QwrManager.peripheral == QWRPeripheral.Flip)
            {
                deviceName = "SkyWorth_DayDream_Controller";
            }
            if (!OpenPeripheral(deviceName))
            {
                Debug.Log("Open Peripheral " + deviceName + " fails.");
                return;
            }

            instance = this;

        }

        /// <summary>
        /// Disables this controller.
        /// </summary>
        public override void OnDisable()
        {
            base.OnDisable();
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// Update the state of the controller and device.
        /// </summary>
        public override void QwrpUpdate()
        {
            int fff = peripheral.QwrupReadDeviceAttribute((int)QWRP_DAYDREAM_ATTRIBUTE.QWRP_DAYDREAM_ATTRIBUTE_UPDATE, IntPtr.Zero, 0);
            if (1 != fff)
            {
                Debug.Log("Qwr: Controller update fails");
            }

            base.QwrpUpdate();

            ReadState(controllerState, lastcontrollerState);

            UpdateGesturePos();

            if (controllerState.connectionState != lastcontrollerState.connectionState)
            {
                switch (controllerState.connectionState)
                {
                    case QwrConnectionState.Error:
                        break;
                    case QwrConnectionState.Disconnected:
                        OnDisconnected();
                        break;
                    case QwrConnectionState.Connected:
                        OnConnected();
                        break;
                }
            }
            // If a headset recenter was requested, do it now.
            if (controllerState.headsetRecenterRequested)
            {
                QwrCameraRig._instance.Recenter();
            }

            lastcontrollerState.CopyFrom(controllerState);

        }

        int GetBatteryPower()
        {
            IntPtr temp = Marshal.AllocHGlobal(sizeof(int));
            peripheral.QwrupReadDeviceAttribute(QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE_BATTERY_POWER - QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE, temp, sizeof(int));
            int power = Marshal.ReadInt32(temp);
            Marshal.FreeHGlobal(temp);
            return power;
        }

        void ReadState(QwrDaydreamControllerState outState, QwrDaydreamControllerState lastState)
        {
            outState.connectionState = (QwrConnectionState)QwrpGetDeviceCurrentStatus().device_status;
            outState.apiStatus = QwrControllerApiStatus.Ok;

            float[] pose = QwrpGetDeviceCurrentStatus().pose_state[0];
            qwrQuarterion rawOri = new qwrQuarterion { s = pose[0], i = pose[1], j = pose[2], k = pose[3] };
            qwrVector3 rawAccel = new qwrVector3 { x = pose[7], y = pose[8], z = pose[9] };
            qwrVector3 rawGyro = new qwrVector3 { x = pose[4], y = pose[5], z = pose[6] };
            outState.orientation = rawOri.ToQuaternion();
            //outState.orientation = new Quaternion(pose[1], pose[2], pose[3], pose[0]);
            outState.accel = new Vector3(rawAccel.x, rawAccel.y, -rawAccel.z);
            outState.gyro = new Vector3(-rawGyro.x, -rawGyro.y, rawGyro.z);

            outState.batteryPower = GetBatteryPower();
            switch (QwrManager.peripheral)
            {
                case QWRPeripheral.Flip:
                    {
                        float touchPos_x = QwrpGetDeviceCurrentStatus().axis_state[(int)QWRP_DAYDREAM_AXES.QWRP_DAYDREAM_AXIS_X][0];
                        float touchPos_y = QwrpGetDeviceCurrentStatus().axis_state[(int)QWRP_DAYDREAM_AXES.QWRP_DAYDREAM_AXIS_Y][0];

                        switch (QwrManager.touchPosOrig)
                        {
                            case TouchPosOrig.TOP_LEFT:
                                outState.touchPos = new Vector2(touchPos_x, touchPos_y);
                                break;
                            case TouchPosOrig.CENTER:
                                outState.touchPos = new Vector2(touchPos_x, touchPos_y) * 2.0f - Vector2.one;
                                break;
                        }

                        outState.isTouching = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_FLIP_BUTTONS.QWRP_FLIP_BUTTON_TOUCH][0];

                        outState.touchDown = !lastState.isTouching && outState.isTouching;
                        outState.touchUp = lastState.isTouching && !outState.isTouching;

                        outState.appButtonState = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_FLIP_BUTTONS.QWRP_FLIP_BUTTON_APP][0];
                        outState.appButtonDown = !lastState.appButtonState && outState.appButtonState;
                        outState.appButtonUp = lastState.appButtonState && !outState.appButtonState;

                        outState.clickButtonState = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_FLIP_BUTTONS.QWRP_FLIP_BUTTON_CLICK][0];
                        outState.clickButtonDown = !lastState.clickButtonState && outState.clickButtonState;
                        outState.clickButtonUp = lastState.clickButtonState && !outState.clickButtonState;

                        outState.triggerButtonState = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_FLIP_BUTTONS.QWRP_FLIP_BUTTON_TRIGGGER][0];
                        outState.triggerButtonDown = !lastState.triggerButtonState && outState.triggerButtonState;
                        outState.triggerButtonUp = lastState.triggerButtonState && !outState.triggerButtonState;


                        outState.homeButtonState =  0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_FLIP_BUTTONS.QWRP_FLIP_BUTTON_HOME][0];
                        outState.homeButtonDown  =  !lastState.homeButtonState && outState.homeButtonState;
                        outState.homeButtonUp    =  lastState.homeButtonState && !outState.homeButtonState;


                        //outState.volumeUpButtonState = false;
                        //outState.volumeUpButtonDown = false;
                        //outState.volumeUpButtonUp = false;

                        //outState.volumeDownButtonState = false;
                        //outState.volumeDownButtonDown = false;
                        //outState.volumeDownButtonUp = false;

                        outState.recentering = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_FLIP_BUTTONS.QWRP_FLIP_BUTTON_RECENTERING][0];
                        outState.recentered = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_FLIP_BUTTONS.QWRP_FLIP_BUTTON_RECENTERED][0];
                        outState.recenterBegin = controllerState.recentering && !lastcontrollerState.recentering;
                        outState.recenterCancel = !controllerState.recentered && !controllerState.recentering && lastcontrollerState.recentering;

                        //Debug.Log(" QwrDaydreamControllerState GetData:" + "connectionState = " + outState.connectionState
                        //+ "  outState.accX = " + outState.accel.x + "   outState.accY = " + outState.accel.y + "  outState.accZ = " + outState.accel.z
                        //+ "  outState.gyroX = " + outState.gyro.x + "   outState.gyroY = " + outState.gyro.y + "  outState.gyroZ = " + outState.gyro.z
                        //+ "  outState.oriX = " + outState.orientation.x + "   outState.oriY = " + outState.orientation.y + "  outState.oriZ = " + outState.orientation.z
                        //+ "  outState.touchX = " + outState.touchPos.x + "   outState.touchY = " + outState.touchPos.y
                        //+ "  outState.btnAPP = " + outState.appButtonState + "   outState.btnHome = " + outState.recentering + "   outState.btnClick = " + outState.clickButtonState
                        //+ "  outState.btnvolumeUp = " + outState.volumeUpButtonState + "   outState.btnvolumeDown = " + outState.volumeDownButtonState);

                        // If the controller was recentered, we may also need to request that the headset be
                        // recentered. We should do that only if VrCore does NOT implement recentering.
                        outState.headsetRecenterRequested = outState.recentered;
                        if (outState.touchUp)
                        {
                            _touchUpPos = lastcontrollerState.touchPos;
                        }
                        break;
                    }
                default:
                    {
                        float touchPos_x = QwrpGetDeviceCurrentStatus().axis_state[(int)QWRP_DAYDREAM_AXES.QWRP_DAYDREAM_AXIS_X][0];
                        float touchPos_y = QwrpGetDeviceCurrentStatus().axis_state[(int)QWRP_DAYDREAM_AXES.QWRP_DAYDREAM_AXIS_Y][0];
                        outState.touchPos = new Vector2(touchPos_x, touchPos_y);
                        switch (QwrManager.touchPosOrig)
                        {
                            case TouchPosOrig.TOP_LEFT:
                                outState.touchPos = new Vector2(touchPos_x, touchPos_y);
                                outState.isTouching = (0 != touchPos_x) && (0 != touchPos_y);
                                break;
                            case TouchPosOrig.CENTER:
                                outState.touchPos = new Vector2(touchPos_x, touchPos_y) * 2.0f - Vector2.one;
                                outState.isTouching = (-1 != touchPos_x) && (-1 != touchPos_y);
                                break;
                        }

                        outState.touchDown = !lastState.isTouching && outState.isTouching;
                        outState.touchUp = lastState.isTouching && !outState.isTouching;

                        outState.appButtonState = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_DAYDREAM_BUTTONS.QWRP_DAYDREAM_BUTTON_APP][0];
                        outState.appButtonDown = !lastState.appButtonState && outState.appButtonState;
                        outState.appButtonUp = lastState.appButtonState && !outState.appButtonState;

                        outState.clickButtonState = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_DAYDREAM_BUTTONS.QWRP_DAYDREAM_BUTTON_CLICK][0];
                        outState.clickButtonDown = !lastState.clickButtonState && outState.clickButtonState;
                        outState.clickButtonUp = lastState.clickButtonState && !outState.clickButtonState;


                        outState.homeButtonState = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_DAYDREAM_BUTTONS.QWRP_DAYDREAM_BUTTON_HOME][0];
                        outState.homeButtonDown = !lastState.homeButtonState && outState.homeButtonState;
                        outState.homeButtonUp = lastState.homeButtonState && !outState.homeButtonState;

                        outState.triggerButtonState = false;
                        outState.triggerButtonDown = false;
                        outState.triggerButtonUp = false;

                        //outState.volumeUpButtonState = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_DAYDREAM_BUTTONS.QWRP_DAYDREAM_BUTTON_VOLUMEUP][0];
                        //outState.volumeUpButtonDown = !lastState.volumeUpButtonState && outState.volumeUpButtonState;
                        //outState.volumeUpButtonUp = lastState.volumeUpButtonState && !outState.volumeUpButtonState;

                        //outState.volumeDownButtonState = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_DAYDREAM_BUTTONS.QWRP_DAYDREAM_BUTTON_VOLUMEDOWN][0];
                        //outState.volumeDownButtonDown = !lastState.volumeDownButtonState && outState.volumeDownButtonState;
                        //outState.volumeDownButtonUp = lastState.volumeDownButtonState && !outState.volumeDownButtonState;

                        outState.recentering = 0 != QwrpGetDeviceCurrentStatus().button_state[(int)QWRP_DAYDREAM_BUTTONS.QWRP_DAYDREAM_BUTTON_HOME][0];
                        outState.recentered = lastState.recentering && !outState.recentering;
                        outState.recenterBegin = controllerState.recentering && !lastcontrollerState.recentering;
                        outState.recenterCancel = !controllerState.recentered && !controllerState.recentering && lastcontrollerState.recentering;

                        //Debug.Log(" QwrDaydreamControllerState GetData:" + "connectionState = " + outState.connectionState
                        //+ "  outState.accX = " + outState.accel.x + "   outState.accY = " + outState.accel.y + "  outState.accZ = " + outState.accel.z
                        //+ "  outState.gyroX = " + outState.gyro.x + "   outState.gyroY = " + outState.gyro.y + "  outState.gyroZ = " + outState.gyro.z
                        //+ "  outState.oriX = " + outState.orientation.x + "   outState.oriY = " + outState.orientation.y + "  outState.oriZ = " + outState.orientation.z
                        //+ "  outState.touchX = " + outState.touchPos.x + "   outState.touchY = " + outState.touchPos.y
                        //+ "  outState.btnAPP = " + outState.appButtonState + "   outState.btnHome = " + outState.recentering + "   outState.btnClick = " + outState.clickButtonState
                        //+ "  outState.btnvolumeUp = " + outState.volumeUpButtonState + "   outState.btnvolumeDown = " + outState.volumeDownButtonState);

                        // If the controller was recentered, we may also need to request that the headset be
                        // recentered. We should do that only if VrCore does NOT implement recentering.
                        outState.headsetRecenterRequested = outState.recentered;
                        if (outState.touchUp)
                        {
                            _touchUpPos = lastcontrollerState.touchPos;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Resumes the controller.
        /// </summary>
        public override void QwrpResume()
        {
            peripheral.QwrupResume();
            CheckConnectState();

            StartCoroutine(Coroutine_UpdateInteractiveHand());
        }

        /// <summary>
        /// Starts the controller. 
        /// </summary>
        void Start()
        {
            if (peripheral == null)
            {
                OnDisconnected();
                this.gameObject.SetActive(false);
                return;
            }
            CheckConnectState();

            UpdateInteractiveHand();

            onUnbind += OnUnbind;
        }

        /// <summary>
        /// Triggered by OnApplicationPause and OnApplicationFocus.
        /// </summary>
        public override void QwrpPause()
        {
            peripheral.QwrupPause();
        }

        void OnDisconnected()
        {
            _isValid = false;
            SendMessageUpwards("OnPeripheralDisconnected", this);
        }
        void OnConnected()
        {
            _isValid = true;
            SendMessageUpwards("OnPeripheralConnected", this);
            _interactiveHand = GetInteractiveHand();

            if (onConnected != null)
                onConnected(this);
        }

        /// <summary>
        /// Enables the internal objects.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        public override void EnableInternalObjects(bool enabled)
        {
            if (model && model.gameObject)
                model.gameObject.SetActive(enabled);

            if (QwrManager.QWRPointer)
            {
                UpdateInteractiveHand();
            }

            QwrPointerManager.Instance.ray.SetVisible(true);
        }

        /// <summary>
        /// If true,the controller is being used. 
        /// </summary>
        /// <returns></returns>
        public override bool IsActivating()
        {
            return QwrDaydreamController.IsTouching
                || QwrDaydreamController.TriggerButtonDown || QwrDaydreamController.TriggerButtonUp
                || QwrDaydreamController.RecenterBegin || QwrDaydreamController.Recentering
                || QwrDaydreamController.BackButtonDown || QwrDaydreamController.BackButtonUp;
        }

        void CheckConnectState()
        {
            ReadState(controllerState, lastcontrollerState);
            if (controllerState.connectionState != QwrConnectionState.Connected)
            {
                OnDisconnected();
            }
        }

        Vector2 _gestureBeginPos;
        Vector2 _gestureEndPos;

        void UpdateGesturePos()
        {
            if (controllerState.touchDown)
            {
                _gestureBeginPos = controllerState.touchPos;
            }
            else if (controllerState.touchUp)
            {
                _gestureEndPos = lastcontrollerState.touchPos;
            }
        }

        /// <summary>
        /// If true,the user is sliding down on the touchpad.
        /// </summary>
        static public bool TouchGestureDown
        {
            get
            {
                if (instance != null && instance._isValid && instance.controllerState.touchUp)
                {
                    Vector2 delta = instance._gestureEndPos - instance._gestureBeginPos;
                    return (delta.y > 0.3f) && (Mathf.Abs(delta.y) > Mathf.Abs(delta.x));
                }
                return false;
            }
        }

        /// <summary>
        /// If true,the user is sliding up on the touchpad.
        /// </summary>
        static public bool TouchGestureUp
        {
            get
            {
                if (instance != null && instance._isValid && instance.controllerState.touchUp)
                {
                    Vector2 delta = instance._gestureEndPos - instance._gestureBeginPos;
                    return (delta.y < -0.3f) && (Mathf.Abs(delta.y) > Mathf.Abs(delta.x));
                }
                return false;
            }

        }

        /// <summary>
        ///  If true,the user is sliding to the left side on the touchpad.
        /// </summary>
        static public bool TouchGestureLeft
        {
            get
            {
                if (instance != null && instance._isValid && instance.controllerState.touchUp)
                {
                    Vector2 delta = instance._gestureEndPos - instance._gestureBeginPos;
                    return (delta.x < -0.3f) && (Mathf.Abs(delta.x) > Mathf.Abs(delta.y));
                }
                return false;
            }
        }

        /// <summary>
        /// If true,the user is sliding to the right side on the touchpad.
        /// </summary>
        static public bool TouchGestureRight
        {
            get
            {
                if (instance != null && instance._isValid && instance.controllerState.touchUp)
                {
                    Vector2 delta = instance._gestureEndPos - instance._gestureBeginPos;
                    return (delta.x > 0.3f) && (Mathf.Abs(delta.x) > Mathf.Abs(delta.y));
                }
                return false;
            }
        }

        /// <summary>
        /// If true, the back button is currently being pressed. This is not
        /// an event: it represents the button's state (it remains true while the button is being
        /// pressed).
        /// </summary>
        static public bool BackButton
        {
            get
            {
                return instance != null
                    && instance._isValid
                    && instance.controllerState.appButtonState;
            }
        }

        /// <summary>
        /// If true, the back button was just pressed. This is an event flag:
        /// it will be true for only one frame after the event happens.
        /// </summary>
        static public bool BackButtonDown
        {
            get
            {
                return instance != null
                    && instance._isValid
                    && instance.controllerState.appButtonDown;
            }
        }

        /// <summary>
        /// If true, the back button was just released. This is an event flag:
        /// it will be true for only one frame after the event happens.
        /// </summary>
        static public bool BackButtonUp
        {
            get
            {
                return instance != null
                    && instance._isValid
                    && instance.controllerState.appButtonUp;
            }
        }

      
        static public bool HomeButtonDown
        {
            get
            {
                return instance != null
                    && instance._isValid
                    && instance.controllerState.homeButtonDown;
            }
        }


        private int _interactiveHand = 0;

        
        

        void SetInteractiveHand(int interactiveHand)          
        {
            if (peripheral == null)
                return;

            if (interactiveHand != 0 && interactiveHand != 1)
            {
                Debug.LogError("SetInteractiveHand : interactiveHand is invalid value");
                return;
            }

            _interactiveHand = interactiveHand;

            IntPtr buffer = Marshal.AllocHGlobal(sizeof(int));
            int[] values = new int[1];
            values[0] = interactiveHand;
            Marshal.Copy(values, 0, buffer, 1);

            peripheral.QwrupSetDeviceAttribute(QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE_INTERACTIVE_HAND - QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE
                , buffer, sizeof(int));

            Marshal.FreeHGlobal(buffer);

            UpdateArmModel();
        }


        int GetInteractiveHand()
        {
            if (peripheral == null)
                return 0;

            IntPtr buffer = Marshal.AllocHGlobal(sizeof(int));
            int ret = peripheral.QwrupReadDeviceAttribute(QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE_INTERACTIVE_HAND - QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE
            , buffer, sizeof(int));
            int interactiveHand = 0;
            if (ret == 1)
            {
                int[] value = new int[1];
                Marshal.Copy(buffer, value, 0, 1);
                interactiveHand = value[0];
            }
            Marshal.FreeHGlobal(buffer);
            return interactiveHand;
        }

        void UpdateArmModel()
        {
            if (armModel != null)
                armModel.interactiveHand = _interactiveHand;
        }

        /// <summary>
        /// Gets or sets the interactive hand.
        /// </summary>
        /// <value>
        /// The interactive hand.
        /// </value>
        static public int interactiveHand
        {
            set
            {
                if (instance != null)
                    instance.SetInteractiveHand(value);
            }
            get
            {
                return instance != null ? instance.GetInteractiveHand() : 0;
            }
        }

        IEnumerator Coroutine_UpdateInteractiveHand()
        {
            yield return new WaitForSeconds(0.5f);
            UpdateInteractiveHand();
        }

        void UpdateInteractiveHand()
        {
            _interactiveHand = GetInteractiveHand();
            UpdateArmModel();
        }

        /// <summary>
        /// Starts the process of booting pair.
        /// </summary>
        public static void StartBootPair()
        {
            instance.peripheral.QwrupBootPair(true);
        }

        /// <summary>
        /// Stops the process of booting pair.
        /// </summary>
        public static void StopBootPair()
        {
            instance.peripheral.QwrupBootPair(false);
        }

        void OnUnbind(QwrBasePeripheral peripheral)
        {
            if (peripheral != this)
                return;

            _isValid = false;
        }

        /// <summary>
        /// Unbinds this controller.
        /// </summary>
        public static void Unbind()
        {
            instance.peripheral.QwrupSetUnbindDevice();
            onUnbind(instance);
        }


        public delegate void Delegate_OnConnected(QwrBasePeripheral peripheral);


        public delegate void Delegate_OnUnbind(QwrBasePeripheral peripheral);

        /// <summary>
        /// Declare a delegate object. 
        /// Called when the controller is connected.
        /// </summary>
        public static Delegate_OnConnected onConnected = null;

        /// <summary>
        /// Declare a delegate object. 
        /// Called when the controller is unbound.
        /// </summary>
        public static Delegate_OnConnected onUnbind = null;

        /// <summary>
        /// Gets the name of the bond device.
        /// </summary>
        /// <returns></returns>
        public static string GetBondDeviceName()
        {
            return instance.peripheral.QwrupGetBondDeviceName();
        }

        [SerializeField]
        Transform rayOrig;

        override public Ray GetRay()
        {
            return new Ray(rayOrig.position, rayOrig.forward);
        }
    }
}
