/************************************************************************************

Copyright: Copyright(c) 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace qwr
{
    public class QwrpUnity
    {
        public const string LibQwr = "QwrUnity";
        public const int QWRP_VALUE_TYPE_SIZE_POSE = 10;
        public const int QWRP_VALUE_TYPE_SIZE_POSITION = 9;
        public const int QWRP_VALUE_TYPE_SIZE_TIME = 1;
        public const int QWRP_VALUE_TYPE_SIZE_QUATERNION = 4;
        public const int QWRP_VALUE_TYPE_SIZE_VECTOR = 3;
        public const int QWRP_VALUE_TYPE_SIZE_AXIS = 1;
        public const int QWRP_VALUE_TYPE_SIZE_BUTTON = 1;
        public const int QWRP_VALUE_TYPE_SIZE_ATTRIBUTE = 1;
        public const int QWRP_VALUE_TYPE_SIZE_FEEDBACK = 1;
        public const int QWRP_VALUE_TYPE_SIZE_SENSOR = 11;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void QwrpReadPoseFunc(IntPtr device, IntPtr userData, float[] pose);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void QwrpReadPositionFunc(IntPtr device, IntPtr userData, float[] position);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void QwrpHandleEventFunc(IntPtr device, int event_mask, IntPtr userData);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupResume(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupPause(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupQueryDeviceCount(QWRP_DEVICE_TYPE type);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr QwrupGetDeviceId(QWRP_DEVICE_TYPE type, int index);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr QwrupOpenDevice(IntPtr deviceId);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupCloseDevice(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupGetDeviceButtonCount(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupGetDeviceAxisCount(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupGetDevicePoseCount(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupGetDevicePositionCount(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupGetDeviceTimeCount(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupGetDeviceQuaternionCount(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupGetDeviceVectorCount(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupGetDeviceAttributeCount(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupGetDeviceFeedbackCount(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr QwrupReadDeviceId(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr QwrupGetAssociatedDevice(IntPtr device, QWRP_DEVICE_TYPE type);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwrupReadDevicePose(IntPtr device, int index, float[] pose);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwrupReadDevicePosition(IntPtr device, int index, float[] position);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static double QwrupReadDeviceTime(IntPtr device, int index);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwrupReadDeviceQuaternion(IntPtr device, int index, float[] quaternion);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwrupReadDeviceVector(IntPtr device, int index, float[] vector);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static float QwrupReadDeviceAxis(IntPtr device, int index);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupReadDeviceButton(IntPtr device, int index);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupReadDeviceAttribute(IntPtr device, int index, IntPtr buffer, int buffer_length);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwrupSetDeviceAttribute(IntPtr device, int index, IntPtr buffer, int buffer_length);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupResetDevicePose(IntPtr device, int index, float[] pose);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupResetDevicePosition(IntPtr device, int index, float[] position);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupRegisterReferencePoseFunction(IntPtr device, int index, IntPtr callback, IntPtr userData);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupRegisterRererencePositionFunction(IntPtr device, int index, IntPtr callback, IntPtr userData);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupRegisterEventNotificationFunction(IntPtr device, QwrpHandleEventFunc callback, int event_mask, IntPtr userData);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupWriteDeviceFeedback(IntPtr device, int index, float value);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupHandleControllerData(IntPtr device, int hand, ref QwrnControllerStateRecordOriginal src, ref QwrnControllerStateRecord dest);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupBootPair(IntPtr device, bool value);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupSetUnbindDevice(IntPtr device);

        [DllImport(LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static void QwrupGetBondDeviceName(IntPtr device, ref byte name);
        
    }

    public enum QWRP_DEVICE_TYPE
    {
        QWRP_DEVICE_TYPE_NONE = -1,
        QWRP_DEVICE_TYPE_JOYSTICK = 0,
        QWRP_DEVICE_TYPE_LEFT_HAND = 1,
        QWRP_DEVICE_TYPE_RIGHT_HAND = 2,
        QWRP_DEVICE_TYPE_HEAD_TRACKER = 3,
        QWRP_DEVICE_TYPE_LEFT_CONTROLLER = 4,
        QWRP_DEVICE_TYPE_RIGHT_CONTROLLER = 5,
        QWRP_DEVICE_TYPE_LEFT_JOYSTICK = 6,
        QWRP_DEVICE_TYPE_RIGHT_JOYSTICK = 7,
        QWRP_DEVICE_TYPE_MOUSE_KEYBOARD = 8,
        QWRP_DEVICE_TYPE_TRACKER = 9,
        QWRP_DEVICE_TYPE_DEBUG_CONTROL = 10,
        QWRP_DEVICE_TYPE_BASE_MASTER = 11,
        QWRP_DEVICE_TYPE_BASE_SLAVE = 12,
        QWRP_DEVICE_TYPE_MR_CONTROLLER = 13,

        // This should always be the last one.
        QWRP_DEVICE_TYPE_NUM,
    };

    public enum QWRP_EVENT_TYPE
    {
        QWRP_EVENT_TYPE_CONNECT = 0x0001,
        QWRP_EVENT_TYPE_DISCONNECT = 0x0002,
        QWRP_EVENT_TYPE_USB_PLUGIN = 0x0004,
        QWRP_EVENT_TYPE_USB_UNPLUG = 0x0008,
        QWRP_EVENT_TYPE_HDMI_PLUGIN = 0x0010,
        QWRP_EVENT_TYPE_HDMI_UNPLUG = 0x0020,
        QWRP_EVENT_TYPE_POSE_UPDATE = 0x0040,
        QWRP_EVENT_TYPE_POSITION_UPDATE = 0x0080,
        QWRP_EVENT_TYPE_TIME_UPDATE = 0x0100,
        QWRP_EVENT_TYPE_QUATERNION_UPDATE = 0x0200,
        QWRP_EVENT_TYPE_VECTOR_UPDATE = 0x0400,
        QWRP_EVENT_TYPE_AXIS_UPDATE = 0x0800,
        QWRP_EVENT_TYPE_BUTTON_UPDATE = 0x1000,
        QWRP_EVENT_TYPE_ATTRIBUTE_UPDATE = 0x2000,
        QWRP_EVENT_TYPE_PERFORMANCE_LOW = 0x8000,// 0x4000 used by p-sensor
        QWRP_EVENT_TYPE_BASE_A_TRACK = 0x10000,
        QWRP_EVENT_TYPE_BASE_A_UNTRACK = 0x20000,
        QWRP_EVENT_TYPE_BASE_B_TRACK = 0x40000,
        QWRP_EVENT_TYPE_BASE_B_UNTRACK = 0x80000,
        QWRP_EVENT_TYPE_FILE_STREAM_END = 0x100000,
        QWRP_EVENT_TYPE_VSYNC = 0x200000
    };

    public enum QWRP_DEVICE_STATUS
    {
        QWRP_DEVICE_STATUS_CONNECT = 0x0001,
        QWRP_DEVICE_STATUS_DISCONNECT = 0x0002,
        QWRP_DEVICE_STATUS_USB_PLUGIN = 0x0004,
        QWRP_DEVICE_STATUS_USB_UNPLUG = 0x0008,
        QWRP_DEVICE_STATUS_HDMI_PLUGIN = 0x0010,
        QWRP_DEVICE_STATUS_HDMI_UNPLUG = 0x0020,
        QWRP_DEVICE_STATUS_BASE_A_TRACK = 0x0040,
        QWRP_DEVICE_STATUS_BASE_A_UNTRACK = 0x0080,
        QWRP_DEVICE_STATUS_BASE_B_TRACK = 0x0100,
        QWRP_DEVICE_STATUS_BASE_B_UNTRACK = 0x0200,
    };

    public enum QWRP_VALUE_TYPE
    {
        QWRP_VALUE_TYPE_POSE_COUNT = 0,
        QWRP_VALUE_TYPE_POSITION_COUNT,
        QWRP_VALUE_TYPE_TIME_COUNT,
        QWRP_VALUE_TYPE_QUATERNION_COUNT,
        QWRP_VALUE_TYPE_VECTOR_COUNT,
        QWRP_VALUE_TYPE_AXIS_COUNT,
        QWRP_VALUE_TYPE_BUTTON_COUNT,
        QWRP_VALUE_TYPE_ATTRIBUTE_COUNT,
        QWRP_VALUE_TYPE_FEEDBACK_COUNT,

        QWRP_VALUE_TYPE_POSE = 0x100,                // Size = 10, type: float
                                                        // POSE0
        QWRP_VALUE_TYPE_POSE_W = QWRP_VALUE_TYPE_POSE,
        QWRP_VALUE_TYPE_POSE_I,
        QWRP_VALUE_TYPE_POSE_J,
        QWRP_VALUE_TYPE_POSE_K,
        QWRP_VALUE_TYPE_ANGULAR_VELOCITY_X,
        QWRP_VALUE_TYPE_ANGULAR_VELOCITY_Y,
        QWRP_VALUE_TYPE_ANGULAR_VELOCITY_Z,
        QWRP_VALUE_TYPE_ANGULAR_ACCELERATION_X,
        QWRP_VALUE_TYPE_ANGULAR_ACCELERATION_Y,
        QWRP_VALUE_TYPE_ANGULAR_ACCELERATION_Z,
        // POSE1

        QWRP_VALUE_TYPE_POSITION = 0x200,           // Size = 9, type: float
                                                    // POSITION0
        QWRP_VALUE_TYPE_POSITION_X = QWRP_VALUE_TYPE_POSITION,
        QWRP_VALUE_TYPE_POSITION_Y,
        QWRP_VALUE_TYPE_POSITION_Z,
        QWRP_VALUE_TYPE_POSITION_VELOCITY_X,
        QWRP_VALUE_TYPE_POSITION_VELOCITY_Y,
        QWRP_VALUE_TYPE_POSITION_VELOCITY_Z,
        QWRP_VALUE_TYPE_POSITION_ACCELERATION_X,
        QWRP_VALUE_TYPE_POSITION_ACCELERATION_Y,
        QWRP_VALUE_TYPE_POSITION_ACCELERATION_Z,
        // POSITION1

        QWRP_VALUE_TYPE_TIME = 0x300,               //Size = 1, type: double
                                                    // TIME_CURRENT
        QWRP_VALUE_TYPE_TIME_CURRENT = QWRP_VALUE_TYPE_TIME,
        /*    TIME_POSE_0
                        .
                        .
                        .
                TIME_POSE_N
                TIME_POSITION_0
                            .
                            .
                            .
                TIME_POSITION_N
                    others         */


        QWRP_VALUE_TYPE_QUATERNION = 0x400,         //Size = 4, type: float
                                                    // QUATERNION0
        QWRP_VALUE_TYPE_QUATERNION_W = QWRP_VALUE_TYPE_QUATERNION,
        QWRP_VALUE_TYPE_QUATERNION_I,
        QWRP_VALUE_TYPE_QUATERNION_J,
        QWRP_VALUE_TYPE_QUATERNION_K,
        // QUATERNION1

        QWRP_VALUE_TYPE_VECTOR = 0x500,             //Size = 3, type: float
                                                    // VECTOR0
        QWRP_VALUE_TYPE_VECTOR_X = QWRP_VALUE_TYPE_VECTOR,
        QWRP_VALUE_TYPE_VECTOR_Y,
        QWRP_VALUE_TYPE_VECTOR_Z,
        // VECTOR1

        QWRP_VALUE_TYPE_AXIS = 0x600,               //Size = 1, type: float
        QWRP_VALUE_TYPE_AXIS_X = QWRP_VALUE_TYPE_AXIS,
        QWRP_VALUE_TYPE_AXIS_Y,

        QWRP_VALUE_TYPE_BUTTON = 0x700,             //Size = 1, type: int
        QWRP_VALUE_TYPE_KEYBOARD,

        QWRP_VALUE_TYPE_ATTRIBUTE = 0x800,          //Size = 1, type: void *
        QWRP_VALUE_TYPE_ATTRIBUTE_DEVICE_STATUS = QWRP_VALUE_TYPE_ATTRIBUTE,
        QWRP_VALUE_TYPE_ATTRIBUTE_DEVICE_NAME,
        QWRP_VALUE_TYPE_ATTRIBUTE_DEVICE_SERIAL_NUMBER,
        QWRP_VALUE_TYPE_ATTRIBUTE_DEVICE_VID,
        QWRP_VALUE_TYPE_ATTRIBUTE_DEVICE_PID,
        QWRP_VALUE_TYPE_ATTRIBUTE_FIRMWARE_VERSION,
        QWRP_VALUE_TYPE_ATTRIBUTE_HARDWARE_VERSION,
        QWRP_VALUE_TYPE_ATTRIBUTE_BACK_LIGHT,
        QWRP_VALUE_TYPE_ATTRIBUTE_P_SENSOR,
        QWRP_VALUE_TYPE_ATTRIBUTE_BATTERY_POWER,
        QWRP_VALUE_TYPE_ATTRIBUTE_RF_VERSION,
        QWRP_VALUE_TYPE_ATTRIBUTE_FPGA_VERSION,
        QWRP_VALUE_TYPE_ATTRIBUTE_RECENTER_GLOBAL,
        QWRP_VALUE_TYPE_ATTRIBUTE_FIX_POSE,     // always makes hmd pose at (0,0,0,1.0)
        QWRP_VALUE_TYPE_ATTRIBUTE_SCREEN, // On/Off
        QWRP_VALUE_TYPE_ATTRIBUTE_RESET_SENSOR_FUSION,
        QWRP_VALUE_TYPE_ATTRIBUTE_DDK_LOG_PATH_NAME, // void type, passing char * strings
        QWRP_VALUE_TYPE_ATTRIBUTE_DDK_LOG_MASK, // int type
        QWRP_VALUE_TYPE_ATTRIBUTE_DDK_UDP_PORT, // int type
        QWRP_VALUE_TYPE_ATTRIBUTE_DDK_UDP_INJECT, // int type 0 disable, 1 enable
        QWRP_VALUE_TYPE_ATTRIBUTE_DDK_UDP_LOG, // int type 0 disable, 1 enable
        QWRP_VALUE_TYPE_ATTRIBUTE_DDK_SRC_FILENAME, // void type, passing char * strings
        QWRP_VALUE_TYPE_ATTRIBUTE_DDK_FILE_SRC_EN, // int type 0 disable, 1 enable
        QWRP_VALUE_TYPE_ATTRIBUTE_WRONG_BASE_USE,
        QWRP_VALUE_TYPE_ATTRIBUTE_HMD_EEPROM_ERROR,
        QWRP_VALUE_TYPE_ATTRIBUTE_REBOOT,
        QWRP_VALUE_TYPE_ATTRIBUTE_HMD_ONLY,// int type 0 disable, 1 enable
        QWRP_VALUE_TYPE_ATTRIBUTE_INVERT_YAW,// int type false disable, true enable
        QWRP_VALUE_TYPE_ATTRIBUTE_DEVICE_MODE,
        QWRP_VALUE_TYPE_ATTRIBUTE_EYE_OFFSET,
        QWRP_VALUE_TYPE_ATTRIBUTE_AUTOCALIB_STATUS,//polaris1.1 autoCalibration status
        QWRP_VALUE_TYPE_ATTRIBUTE_HMD_REAL_YAW,// nolo
        QWRP_VALUE_TYPE_ATTRIBUTE_RECENTER_WITH_YAW,// nolo
        QWRP_VALUE_TYPE_ATTRIBUTE_PLUGIN_NAME,// nolo / Ximmerse / Polaris
        QWRP_VALUE_TYPE_ATTRIBUTE_MOVEMENT,// movement
        QWRP_VALUE_TYPE_ATTRIBUTE_INTERACTIVE_TYPE, // 0-mobile touchpad 1-controller
        QWRP_VALUE_TYPE_ATTRIBUTE_INTERACTIVE_HAND, // 0-right, 1-left

        QWRP_VALUE_TYPE_FEEDBACK = 0x900,           //Size = 1, type: void *
        QWRP_VALUE_TYPE_FEEDBACK_BREATH_LIGHT = QWRP_VALUE_TYPE_FEEDBACK,
        QWRP_VALUE_TYPE_FEEDBACK_MOTOR,

        QWRP_VALUE_TYPE_SENSOR = 0xa00,           //Size = 1, type: void *
        QWRP_VALUE_TYPE_SENSOR_ACC_X = QWRP_VALUE_TYPE_SENSOR,
        QWRP_VALUE_TYPE_SENSOR_ACC_Y,
        QWRP_VALUE_TYPE_SENSOR_ACC_Z,
        QWRP_VALUE_TYPE_SENSOR_GYRO_X,
        QWRP_VALUE_TYPE_SENSOR_GYRO_Y,
        QWRP_VALUE_TYPE_SENSOR_GYRO_Z,
        QWRP_VALUE_TYPE_SENSOR_MAG_X,
        QWRP_VALUE_TYPE_SENSOR_MAG_Y,
        QWRP_VALUE_TYPE_SENSOR_MAG_Z,
        QWRP_VALUE_TYPE_SENSOR_TEMPERATURE,
        QWRP_VALUE_TYPE_SENSOR_TIMEINSECONDS,
    };
    public enum QWRDDK_BUTTONMASK
    {
        QWRDDK_BUTTONMASK_B = 0x01,
        QWRDDK_BUTTONMASK_A = 0x02,
        QWRDDK_BUTTONMASK_GRIP = 0x04,
        QWRDDK_BUTTONMASK_HOME = 0x08,
        QWRDDK_BUTTONMASK_TRIGER = 0x10,
        QWRDDK_BUTTONMASK_A_II = 0x20, // key A of another controller

    };
    public enum QWRP_POLARIS_BUTTONS
    {
        QWRP_POLARIS_BUTTONS_ST, // stick button
        QWRP_POLARIS_BUTTONS_XA,
        QWRP_POLARIS_BUTTONS_YB,
        QWRP_POLARIS_BUTTONS_HOME_MENU,

        QWRP_POLARIS_BUTTONS_NUM,
    };
    public enum QWRP_POLARIS_BUTTONMASK
    {
        QWRP_POLARIS_BUTTONMASK_B = 0x01,
        QWRP_POLARIS_BUTTONMASK_A = 0x02,
        QWRP_POLARIS_BUTTONMASK_GRIP = 0x04,
        QWRP_POLARIS_BUTTONMASK_HOME = 0x08,
        QWRP_POLARIS_BUTTONMASK_TRIGER = 0x10,
        QWRP_POLARIS_BUTTONMASK_A_II = 0x20, // key A of another controller
        QWRP_POLARIS_BUTTONMASK_POWER = 0x40,
    };
    public enum QWRP_POLARIS_AXES
    {
        QWRP_POLARIS_AXIS_T,
        QWRP_POLARIS_AXIS_B,
        QWRP_POLARIS_AXIS_X, // stick x
        QWRP_POLARIS_AXIS_Y, // stick y

        QWRP_POLARIS_AXES_NUM,
    };
    public enum QWRP_DAYDREAM_BUTTONS
    {
        QWRP_DAYDREAM_BUTTON_APP,
        QWRP_DAYDREAM_BUTTON_HOME,
        QWRP_DAYDREAM_BUTTON_CLICK,
        QWRP_DAYDREAM_BUTTON_VOLUMEDOWN,
        QWRP_DAYDREAM_BUTTON_VOLUMEUP,
        QWRP_DAYDREAM_BUTTON_NUM,
    };
    public enum QWRP_FLIP_BUTTONS
    {
        QWRP_FLIP_BUTTON_APP,
        QWRP_FLIP_BUTTON_HOME,
        QWRP_FLIP_BUTTON_CLICK,
        QWRP_FLIP_BUTTON_TOUCH,
        QWRP_FLIP_BUTTON_TRIGGGER,
        QWRP_FLIP_BUTTON_RECENTERING,
        QWRP_FLIP_BUTTON_RECENTERED,
        QWRP_FLIP_BUTTON_NUM,
    };
    public enum QWRP_DAYDREAM_AXES
    {
        QWRP_DAYDREAM_AXIS_X,
        QWRP_DAYDREAM_AXIS_Y,
    };
    public enum QWRP_POLARIS_BASE_STATION
    {
        QWRP_POLARIS_BASE_A,
        QWRP_POLARIS_BASE_B,

        QWRP_POLARIS_BASE_NUM,
    };
    public enum QWRP_DAYDREAM_ATTRIBUTE
    {
        QWRP_DAYDREAM_ATTRIBUTE_DEVICE_STATUS = 0,
        QWRP_DAYDREAM_ATTRIBUTE_DEVICE_NAME,
        QWRP_DAYDREAM_ATTRIBUTE_DEVICE_SERIAL_NUMBER,
        QWRP_DAYDREAM_ATTRIBUTE_DEVICE_VID,
        QWRP_DAYDREAM_ATTRIBUTE_DEVICE_PID,
        QWRP_DAYDREAM_ATTRIBUTE_FIRMWARE_VERSION,
        QWRP_DAYDREAM_ATTRIBUTE_SOFTWARE_VERSION,
        QWRP_DAYDREAM_ATTRIBUTE_BACK_LIGHT,
        QWRP_DAYDREAM_ATTRIBUTE_UPDATE,
    }
    public enum QWRP_MOUSE_AXIS
    {
        QWRP_MOUSE_X,
        QWRP_MOUSE_Y,
        QWRP_MOUSE_WHEEL
    };
    public enum QWRP_JOYSTICK_AXIS
    {
        QWRP_JOYSTICK_X,
        QWRP_JOYSTICK_Y,
        QWRP_JOYSTICK_Z,

        QWRP_JOYSTICK_RX,
        QWRP_JOYSTICK_RY,
        QWRP_JOYSTICK_RZ,

        QWRP_JOYSTICK_POV
    };

    enum QWRP_COMMON_BUTTONS
    {
        QWRP_KEYBOARD_BUTTON_0,
        QWRP_KEYBOARD_BUTTON_1,
        QWRP_KEYBOARD_BUTTON_2,
        QWRP_KEYBOARD_BUTTON_3,
        QWRP_KEYBOARD_BUTTON_4,
        QWRP_KEYBOARD_BUTTON_5,
        QWRP_KEYBOARD_BUTTON_6,
        QWRP_KEYBOARD_BUTTON_7,
        QWRP_KEYBOARD_BUTTON_8,
        QWRP_KEYBOARD_BUTTON_9,

        QWRP_KEYBOARD_BUTTON_A,
        QWRP_KEYBOARD_BUTTON_B,
        QWRP_KEYBOARD_BUTTON_C,
        QWRP_KEYBOARD_BUTTON_D,
        QWRP_KEYBOARD_BUTTON_E,
        QWRP_KEYBOARD_BUTTON_F,
        QWRP_KEYBOARD_BUTTON_G,
        QWRP_KEYBOARD_BUTTON_H,
        QWRP_KEYBOARD_BUTTON_I,
        QWRP_KEYBOARD_BUTTON_J,
        QWRP_KEYBOARD_BUTTON_K,
        QWRP_KEYBOARD_BUTTON_L,
        QWRP_KEYBOARD_BUTTON_M,
        QWRP_KEYBOARD_BUTTON_N,
        QWRP_KEYBOARD_BUTTON_O,
        QWRP_KEYBOARD_BUTTON_P,
        QWRP_KEYBOARD_BUTTON_Q,
        QWRP_KEYBOARD_BUTTON_R,
        QWRP_KEYBOARD_BUTTON_S,
        QWRP_KEYBOARD_BUTTON_T,
        QWRP_KEYBOARD_BUTTON_U,
        QWRP_KEYBOARD_BUTTON_V,
        QWRP_KEYBOARD_BUTTON_W,
        QWRP_KEYBOARD_BUTTON_X,
        QWRP_KEYBOARD_BUTTON_Y,
        QWRP_KEYBOARD_BUTTON_Z,

        QWRP_KEYBOARD_BUTTON_F1,
        QWRP_KEYBOARD_BUTTON_F2,
        QWRP_KEYBOARD_BUTTON_F3,
        QWRP_KEYBOARD_BUTTON_F4,
        QWRP_KEYBOARD_BUTTON_F5,
        QWRP_KEYBOARD_BUTTON_F6,
        QWRP_KEYBOARD_BUTTON_F7,
        QWRP_KEYBOARD_BUTTON_F8,
        QWRP_KEYBOARD_BUTTON_F9,
        QWRP_KEYBOARD_BUTTON_F10,
        QWRP_KEYBOARD_BUTTON_F11,
        QWRP_KEYBOARD_BUTTON_F12,
        QWRP_KEYBOARD_BUTTON_F13,
        QWRP_KEYBOARD_BUTTON_F14,
        QWRP_KEYBOARD_BUTTON_F15,

        QWRP_KEYBOARD_BUTTON_BACKSPACE,
        QWRP_KEYBOARD_BUTTON_TAB,
        QWRP_KEYBOARD_BUTTON_CLEAR,
        QWRP_KEYBOARD_BUTTON_RETURN,
        QWRP_KEYBOARD_BUTTON_PAUSE,
        QWRP_KEYBOARD_BUTTON_ESCAPE,
        QWRP_KEYBOARD_BUTTON_SPACE,

        QWRP_KEYBOARD_BUTTON_KEYPAD0,
        QWRP_KEYBOARD_BUTTON_KEYPAD1,
        QWRP_KEYBOARD_BUTTON_KEYPAD2,
        QWRP_KEYBOARD_BUTTON_KEYPAD3,
        QWRP_KEYBOARD_BUTTON_KEYPAD4,
        QWRP_KEYBOARD_BUTTON_KEYPAD5,
        QWRP_KEYBOARD_BUTTON_KEYPAD6,
        QWRP_KEYBOARD_BUTTON_KEYPAD7,
        QWRP_KEYBOARD_BUTTON_KEYPAD8,
        QWRP_KEYBOARD_BUTTON_KEYPAD9,

        QWRP_KEYBOARD_BUTTON_KEYPAD_PERIOD, // del
        QWRP_KEYBOARD_BUTTON_KEYPAD_DIVIDE, // /
        QWRP_KEYBOARD_BUTTON_KEYPAD_MULTIPLY, // *
        QWRP_KEYBOARD_BUTTON_KEYPAD_MINUS, // -
        QWRP_KEYBOARD_BUTTON_KEYPAD_PLUS, // +
        QWRP_KEYBOARD_BUTTON_KEYPAD_ENTER,
        QWRP_KEYBOARD_BUTTON_KEYPAD_EQUALS, // =

        QWRP_KEYBOARD_BUTTON_UP,
        QWRP_KEYBOARD_BUTTON_DOWN,
        QWRP_KEYBOARD_BUTTON_RIGHT,
        QWRP_KEYBOARD_BUTTON_LEFT,

        QWRP_KEYBOARD_BUTTON_INSERT,
        QWRP_KEYBOARD_BUTTON_DELETE,
        QWRP_KEYBOARD_BUTTON_HOME,
        QWRP_KEYBOARD_BUTTON_END,
        QWRP_KEYBOARD_BUTTON_PAGE_UP,
        QWRP_KEYBOARD_BUTTON_PAGE_DOWN,

        QWRP_KEYBOARD_BUTTON_PRINT,
        QWRP_KEYBOARD_BUTTON_SYS_REQ,
        QWRP_KEYBOARD_BUTTON_BREAK,

        QWRP_KEYBOARD_BUTTON_EXCLAIM, // !
        QWRP_KEYBOARD_BUTTON_DOUBLE_QUOTE, // "
        QWRP_KEYBOARD_BUTTON_HASH, // #
        QWRP_KEYBOARD_BUTTON_DOLLAR, // $
        QWRP_KEYBOARD_BUTTON_AMPERSAND, // &
        QWRP_KEYBOARD_BUTTON_QUOTE, //'
        QWRP_KEYBOARD_BUTTON_LEFT_PAREN, // (
        QWRP_KEYBOARD_BUTTON_RIGHT_PAREN, // )
        QWRP_KEYBOARD_BUTTON_ASTERISK, // *
        QWRP_KEYBOARD_BUTTON_PLUS, // +
        QWRP_KEYBOARD_BUTTON_COMMA, // ,
        QWRP_KEYBOARD_BUTTON_MINUS, // -
        QWRP_KEYBOARD_BUTTON_PERIOD, // .
        QWRP_KEYBOARD_BUTTON_SLASH, // /
        QWRP_KEYBOARD_BUTTON_COLON, // :
        QWRP_KEYBOARD_BUTTON_SEMICOLON, // ;
        QWRP_KEYBOARD_BUTTON_LESS, // <
        QWRP_KEYBOARD_BUTTON_EQUALS, // =
        QWRP_KEYBOARD_BUTTON_GREATER, // >
        QWRP_KEYBOARD_BUTTON_QUESTION, // ?
        QWRP_KEYBOARD_BUTTON_AT, // @
        QWRP_KEYBOARD_BUTTON_LEFT_BRACKET, // [
        QWRP_KEYBOARD_BUTTON_RIGHT_BRACKET, // ]
        QWRP_KEYBOARD_BUTTON_CARET, // ^
        QWRP_KEYBOARD_BUTTON_UNDERSCORE, // _
        QWRP_KEYBOARD_BUTTON_BACKQUOTE, // `

        QWRP_KEYBOARD_BUTTON_NUMLOCK,
        QWRP_KEYBOARD_BUTTON_CAPSLOCK,
        QWRP_KEYBOARD_BUTTON_SCROLLLOCK,
        QWRP_KEYBOARD_BUTTON_RIGHT_SHIFT,
        QWRP_KEYBOARD_BUTTON_LEFT_SHIFT,
        QWRP_KEYBOARD_BUTTON_RIGHT_CONTROL,
        QWRP_KEYBOARD_BUTTON_LEFT_CONTROL,
        QWRP_KEYBOARD_BUTTON_RIGHT_ALT,
        QWRP_KEYBOARD_BUTTON_LEFT_ALT,

        QWRP_KEYBOARD_BUTTON_LEFT_COMMAND,
        QWRP_KEYBOARD_BUTTON_LEFT_APPLE,
        QWRP_KEYBOARD_BUTTON_LEFT_WINDOWS,
        QWRP_KEYBOARD_BUTTON_RIGHT_COMMAND,
        QWRP_KEYBOARD_BUTTON_RIGHT_APPLE,
        QWRP_KEYBOARD_BUTTON_RIGHT_WINDOWS,

        QWRP_KEYBOARD_BUTTON_ALT_GR,
        QWRP_KEYBOARD_BUTTON_HELP,
        QWRP_KEYBOARD_BUTTON_MENU,

        QWRP_MOUSE_LEFT_BUTTON = 0x100,
        QWRP_MOUSE_RIGHT_BUTTON,
        QWRP_MOUSE_MIDDLE_BUTTON
    };
    /*
    public enum QWRP_AXIS_TYPE {
        QWRP_AXIS_TYPE_POSE = 0,
        QWRP_AXIS_TYPE_POSE_S,
        QWRP_AXIS_TYPE_POSE_I,
        QWRP_AXIS_TYPE_POSE_J,
        QWRP_AXIS_TYPE_POSE_K,

        QWRP_AXIS_TYPE_POSE_S0 = QWRP_AXIS_TYPE_POSE_S,
        QWRP_AXIS_TYPE_POSE_I0 = QWRP_AXIS_TYPE_POSE_I,
        QWRP_AXIS_TYPE_POSE_J0 = QWRP_AXIS_TYPE_POSE_J,
        QWRP_AXIS_TYPE_POSE_K0 = QWRP_AXIS_TYPE_POSE_K,

        QWRP_AXIS_TYPE_POSE_S1,
        QWRP_AXIS_TYPE_POSE_I1,
        QWRP_AXIS_TYPE_POSE_J1,
        QWRP_AXIS_TYPE_POSE_K1,

        QWRP_AXIS_TYPE_POSE_S2,
        QWRP_AXIS_TYPE_POSE_I2,
        QWRP_AXIS_TYPE_POSE_J2,
        QWRP_AXIS_TYPE_POSE_K2,

        QWRP_AXIS_TYPE_POSE_S3,
        QWRP_AXIS_TYPE_POSE_I3,
        QWRP_AXIS_TYPE_POSE_J3,
        QWRP_AXIS_TYPE_POSE_K3,

        QWRP_AXIS_TYPE_POSE_S4,
        QWRP_AXIS_TYPE_POSE_I4,
        QWRP_AXIS_TYPE_POSE_J4,
        QWRP_AXIS_TYPE_POSE_K4,

        QWRP_AXIS_TYPE_POSE_S5,
        QWRP_AXIS_TYPE_POSE_I5,
        QWRP_AXIS_TYPE_POSE_J5,
        QWRP_AXIS_TYPE_POSE_K5,

        QWRP_AXIS_TYPE_POSE_S6,
        QWRP_AXIS_TYPE_POSE_I6,
        QWRP_AXIS_TYPE_POSE_J6,
        QWRP_AXIS_TYPE_POSE_K6,

        QWRP_AXIS_TYPE_POSE_S7,
        QWRP_AXIS_TYPE_POSE_I7,
        QWRP_AXIS_TYPE_POSE_J7,
        QWRP_AXIS_TYPE_POSE_K7,

        QWRP_AXIS_TYPE_POSE_S8,
        QWRP_AXIS_TYPE_POSE_I8,
        QWRP_AXIS_TYPE_POSE_J8,
        QWRP_AXIS_TYPE_POSE_K8,

        QWRP_AXIS_TYPE_POSE_S9,
        QWRP_AXIS_TYPE_POSE_I9,
        QWRP_AXIS_TYPE_POSE_J9,
        QWRP_AXIS_TYPE_POSE_K9,

        QWRP_AXIS_TYPE_POSE_S10,
        QWRP_AXIS_TYPE_POSE_I10,
        QWRP_AXIS_TYPE_POSE_J10,
        QWRP_AXIS_TYPE_POSE_K10,

        QWRP_AXIS_TYPE_POSE_S11,
        QWRP_AXIS_TYPE_POSE_I11,
        QWRP_AXIS_TYPE_POSE_J11,
        QWRP_AXIS_TYPE_POSE_K11,

        QWRP_AXIS_TYPE_POSE_S12,
        QWRP_AXIS_TYPE_POSE_I12,
        QWRP_AXIS_TYPE_POSE_J12,
        QWRP_AXIS_TYPE_POSE_K12,

        QWRP_AXIS_TYPE_POSE_S13,
        QWRP_AXIS_TYPE_POSE_I13,
        QWRP_AXIS_TYPE_POSE_J13,
        QWRP_AXIS_TYPE_POSE_K13,

        QWRP_AXIS_TYPE_POSE_S14,
        QWRP_AXIS_TYPE_POSE_I14,
        QWRP_AXIS_TYPE_POSE_J14,
        QWRP_AXIS_TYPE_POSE_K14,

        QWRP_AXIS_TYPE_POSE_S15,
        QWRP_AXIS_TYPE_POSE_I15,
        QWRP_AXIS_TYPE_POSE_J15,
        QWRP_AXIS_TYPE_POSE_K15,

        QWRP_AXIS_TYPE_POSE_S16,
        QWRP_AXIS_TYPE_POSE_I16,
        QWRP_AXIS_TYPE_POSE_J16,
        QWRP_AXIS_TYPE_POSE_K16,

        QWRP_AXIS_TYPE_POSE_S17,
        QWRP_AXIS_TYPE_POSE_I17,
        QWRP_AXIS_TYPE_POSE_J17,
        QWRP_AXIS_TYPE_POSE_K17,

        QWRP_AXIS_TYPE_POSE_S18,
        QWRP_AXIS_TYPE_POSE_I18,
        QWRP_AXIS_TYPE_POSE_J18,
        QWRP_AXIS_TYPE_POSE_K18,

        QWRP_AXIS_TYPE_POSE_S19,
        QWRP_AXIS_TYPE_POSE_I19,
        QWRP_AXIS_TYPE_POSE_J19,
        QWRP_AXIS_TYPE_POSE_K19,

        QWRP_AXIS_TYPE_POSITION = 0x100,
        QWRP_AXIS_TYPE_POSITION_X,
        QWRP_AXIS_TYPE_POSITION_Y,
        QWRP_AXIS_TYPE_POSITION_Z,

        QWRP_AXIS_TYPE_POSITION_X0 = QWRP_AXIS_TYPE_POSITION_X,
        QWRP_AXIS_TYPE_POSITION_Y0 = QWRP_AXIS_TYPE_POSITION_Y,
        QWRP_AXIS_TYPE_POSITION_Z0 = QWRP_AXIS_TYPE_POSITION_Z,

        QWRP_AXIS_TYPE_POSITION_X1,
        QWRP_AXIS_TYPE_POSITION_Y1,
        QWRP_AXIS_TYPE_POSITION_Z1,

        QWRP_AXIS_TYPE_POSITION_X2,
        QWRP_AXIS_TYPE_POSITION_Y2,
        QWRP_AXIS_TYPE_POSITION_Z2,

        QWRP_AXIS_TYPE_POSITION_X3,
        QWRP_AXIS_TYPE_POSITION_Y3,
        QWRP_AXIS_TYPE_POSITION_Z3,

        QWRP_AXIS_TYPE_POSITION_X4,
        QWRP_AXIS_TYPE_POSITION_Y4,
        QWRP_AXIS_TYPE_POSITION_Z4,

        QWRP_AXIS_TYPE_POSITION_X5,
        QWRP_AXIS_TYPE_POSITION_Y5,
        QWRP_AXIS_TYPE_POSITION_Z5,

        QWRP_AXIS_TYPE_POSITION_X6,
        QWRP_AXIS_TYPE_POSITION_Y6,
        QWRP_AXIS_TYPE_POSITION_Z6,

        QWRP_AXIS_TYPE_POSITION_X7,
        QWRP_AXIS_TYPE_POSITION_Y7,
        QWRP_AXIS_TYPE_POSITION_Z7,

        QWRP_AXIS_TYPE_POSITION_X8,
        QWRP_AXIS_TYPE_POSITION_Y8,
        QWRP_AXIS_TYPE_POSITION_Z8,

        QWRP_AXIS_TYPE_POSITION_X9,
        QWRP_AXIS_TYPE_POSITION_Y9,
        QWRP_AXIS_TYPE_POSITION_Z9,

        QWRP_AXIS_TYPE_POSITION_X10,
        QWRP_AXIS_TYPE_POSITION_Y10,
        QWRP_AXIS_TYPE_POSITION_Z10,

        QWRP_AXIS_TYPE_POSITION_X11,
        QWRP_AXIS_TYPE_POSITION_Y11,
        QWRP_AXIS_TYPE_POSITION_Z11,

        QWRP_AXIS_TYPE_POSITION_X12,
        QWRP_AXIS_TYPE_POSITION_Y12,
        QWRP_AXIS_TYPE_POSITION_Z12,

        QWRP_AXIS_TYPE_POSITION_X13,
        QWRP_AXIS_TYPE_POSITION_Y13,
        QWRP_AXIS_TYPE_POSITION_Z13,

        QWRP_AXIS_TYPE_POSITION_X14,
        QWRP_AXIS_TYPE_POSITION_Y14,
        QWRP_AXIS_TYPE_POSITION_Z14,

        QWRP_AXIS_TYPE_POSITION_X15,
        QWRP_AXIS_TYPE_POSITION_Y15,
        QWRP_AXIS_TYPE_POSITION_Z15,

        QWRP_AXIS_TYPE_POSITION_X16,
        QWRP_AXIS_TYPE_POSITION_Y16,
        QWRP_AXIS_TYPE_POSITION_Z16,

        QWRP_AXIS_TYPE_POSITION_X17,
        QWRP_AXIS_TYPE_POSITION_Y17,
        QWRP_AXIS_TYPE_POSITION_Z17,

        QWRP_AXIS_TYPE_POSITION_X18,
        QWRP_AXIS_TYPE_POSITION_Y18,
        QWRP_AXIS_TYPE_POSITION_Z18,

        QWRP_AXIS_TYPE_POSITION_X19,
        QWRP_AXIS_TYPE_POSITION_Y19,
        QWRP_AXIS_TYPE_POSITION_Z19,

        QWRP_AXIS_TYPE_AXIS = 0x200,
        QWRP_AXIS_TYPE_AXIS_X,
        QWRP_AXIS_TYPE_AXIS_Y,
        QWRP_AXIS_TYPE_AXIS_Z,
        QWRP_AXIS_TYPE_AXIS_W,

        QWRP_AXIS_TYPE_AXIS_X0 = QWRP_AXIS_TYPE_AXIS_X,
        QWRP_AXIS_TYPE_AXIS_Y0 = QWRP_AXIS_TYPE_AXIS_Y,
        QWRP_AXIS_TYPE_AXIS_Z0 = QWRP_AXIS_TYPE_AXIS_Z,
        QWRP_AXIS_TYPE_AXIS_W0 = QWRP_AXIS_TYPE_AXIS_W,

        QWRP_AXIS_TYPE_AXIS_X1,
        QWRP_AXIS_TYPE_AXIS_Y1,
        QWRP_AXIS_TYPE_AXIS_Z1,
        QWRP_AXIS_TYPE_AXIS_W1,

        QWRP_AXIS_TYPE_AXIS_X2,
        QWRP_AXIS_TYPE_AXIS_Y2,
        QWRP_AXIS_TYPE_AXIS_Z2,
        QWRP_AXIS_TYPE_AXIS_W2,

        QWRP_AXIS_TYPE_AXIS_X3,
        QWRP_AXIS_TYPE_AXIS_Y3,
        QWRP_AXIS_TYPE_AXIS_Z3,
        QWRP_AXIS_TYPE_AXIS_W3,

        QWRP_AXIS_TYPE_AXIS_X4,
        QWRP_AXIS_TYPE_AXIS_Y4,
        QWRP_AXIS_TYPE_AXIS_Z4,
        QWRP_AXIS_TYPE_AXIS_W4,

        QWRP_AXIS_TYPE_AXIS_X5,
        QWRP_AXIS_TYPE_AXIS_Y5,
        QWRP_AXIS_TYPE_AXIS_Z5,
        QWRP_AXIS_TYPE_AXIS_W5,

        QWRP_AXIS_TYPE_AXIS_X6,
        QWRP_AXIS_TYPE_AXIS_Y6,
        QWRP_AXIS_TYPE_AXIS_Z6,
        QWRP_AXIS_TYPE_AXIS_W6,

        QWRP_AXIS_TYPE_AXIS_X7,
        QWRP_AXIS_TYPE_AXIS_Y7,
        QWRP_AXIS_TYPE_AXIS_Z7,
        QWRP_AXIS_TYPE_AXIS_W7,

        QWRP_AXIS_TYPE_AXIS_X8,
        QWRP_AXIS_TYPE_AXIS_Y8,
        QWRP_AXIS_TYPE_AXIS_Z8,
        QWRP_AXIS_TYPE_AXIS_W8,

        QWRP_AXIS_TYPE_AXIS_X9,
        QWRP_AXIS_TYPE_AXIS_Y9,
        QWRP_AXIS_TYPE_AXIS_Z9,
        QWRP_AXIS_TYPE_AXIS_W9,

        QWRP_AXIS_TYPE_AXIS_X10,
        QWRP_AXIS_TYPE_AXIS_Y10,
        QWRP_AXIS_TYPE_AXIS_Z10,
        QWRP_AXIS_TYPE_AXIS_W10,

        QWRP_AXIS_TYPE_AXIS_X11,
        QWRP_AXIS_TYPE_AXIS_Y11,
        QWRP_AXIS_TYPE_AXIS_Z11,
        QWRP_AXIS_TYPE_AXIS_W11,

        QWRP_AXIS_TYPE_AXIS_X12,
        QWRP_AXIS_TYPE_AXIS_Y12,
        QWRP_AXIS_TYPE_AXIS_Z12,
        QWRP_AXIS_TYPE_AXIS_W12,

        QWRP_AXIS_TYPE_AXIS_X13,
        QWRP_AXIS_TYPE_AXIS_Y13,
        QWRP_AXIS_TYPE_AXIS_Z13,
        QWRP_AXIS_TYPE_AXIS_W13,

        QWRP_AXIS_TYPE_AXIS_X14,
        QWRP_AXIS_TYPE_AXIS_Y14,
        QWRP_AXIS_TYPE_AXIS_Z14,
        QWRP_AXIS_TYPE_AXIS_W14,

        QWRP_AXIS_TYPE_AXIS_X15,
        QWRP_AXIS_TYPE_AXIS_Y15,
        QWRP_AXIS_TYPE_AXIS_Z15,
        QWRP_AXIS_TYPE_AXIS_W15,

        QWRP_AXIS_TYPE_AXIS_X16,
        QWRP_AXIS_TYPE_AXIS_Y16,
        QWRP_AXIS_TYPE_AXIS_Z16,
        QWRP_AXIS_TYPE_AXIS_W16,

        QWRP_AXIS_TYPE_AXIS_X17,
        QWRP_AXIS_TYPE_AXIS_Y17,
        QWRP_AXIS_TYPE_AXIS_Z17,
        QWRP_AXIS_TYPE_AXIS_W17,

        QWRP_AXIS_TYPE_AXIS_X18,
        QWRP_AXIS_TYPE_AXIS_Y18,
        QWRP_AXIS_TYPE_AXIS_Z18,
        QWRP_AXIS_TYPE_AXIS_W18,

        QWRP_AXIS_TYPE_AXIS_X19,
        QWRP_AXIS_TYPE_AXIS_Y19,
        QWRP_AXIS_TYPE_AXIS_Z19,
        QWRP_AXIS_TYPE_AXIS_W19,

        QWRP_AXIS_TYPE_HAT_X,
        QWRP_AXIS_TYPE_HAT_Y,
        QWRP_AXIS_TYPE_HAT_Z,
        QWRP_AXIS_TYPE_HAT_W,

        QWRP_AXIS_TYPE_HAT_X0 = QWRP_AXIS_TYPE_HAT_X,
        QWRP_AXIS_TYPE_HAT_Y0 = QWRP_AXIS_TYPE_HAT_Y,
        QWRP_AXIS_TYPE_HAT_Z0 = QWRP_AXIS_TYPE_HAT_Z,
        QWRP_AXIS_TYPE_HAT_W0 = QWRP_AXIS_TYPE_HAT_W,

        QWRP_AXIS_TYPE_HAT_X1,
        QWRP_AXIS_TYPE_HAT_Y1,
        QWRP_AXIS_TYPE_HAT_Z1,
        QWRP_AXIS_TYPE_HAT_W1,

        QWRP_AXIS_TYPE_HAT_X2,
        QWRP_AXIS_TYPE_HAT_Y2,
        QWRP_AXIS_TYPE_HAT_Z2,
        QWRP_AXIS_TYPE_HAT_W2,

        QWRP_AXIS_TYPE_HAT_X3,
        QWRP_AXIS_TYPE_HAT_Y3,
        QWRP_AXIS_TYPE_HAT_Z3,
        QWRP_AXIS_TYPE_HAT_W3,

        QWRP_AXIS_TYPE_HAT_X4,
        QWRP_AXIS_TYPE_HAT_Y4,
        QWRP_AXIS_TYPE_HAT_Z4,
        QWRP_AXIS_TYPE_HAT_W4,

        QWRP_AXIS_TYPE_HAT_X5,
        QWRP_AXIS_TYPE_HAT_Y5,
        QWRP_AXIS_TYPE_HAT_Z5,
        QWRP_AXIS_TYPE_HAT_W5,

        QWRP_AXIS_TYPE_HAT_X6,
        QWRP_AXIS_TYPE_HAT_Y6,
        QWRP_AXIS_TYPE_HAT_Z6,
        QWRP_AXIS_TYPE_HAT_W6,

        QWRP_AXIS_TYPE_HAT_X7,
        QWRP_AXIS_TYPE_HAT_Y7,
        QWRP_AXIS_TYPE_HAT_Z7,
        QWRP_AXIS_TYPE_HAT_W7,

        QWRP_AXIS_TYPE_HAT_X8,
        QWRP_AXIS_TYPE_HAT_Y8,
        QWRP_AXIS_TYPE_HAT_Z8,
        QWRP_AXIS_TYPE_HAT_W8,

        QWRP_AXIS_TYPE_HAT_X9,
        QWRP_AXIS_TYPE_HAT_Y9,
        QWRP_AXIS_TYPE_HAT_Z9,
        QWRP_AXIS_TYPE_HAT_W9,

        QWRP_AXIS_TYPE_HAT_X10,
        QWRP_AXIS_TYPE_HAT_Y10,
        QWRP_AXIS_TYPE_HAT_Z10,
        QWRP_AXIS_TYPE_HAT_W10,

        QWRP_AXIS_TYPE_HAT_X11,
        QWRP_AXIS_TYPE_HAT_Y11,
        QWRP_AXIS_TYPE_HAT_Z11,
        QWRP_AXIS_TYPE_HAT_W11,

        QWRP_AXIS_TYPE_HAT_X12,
        QWRP_AXIS_TYPE_HAT_Y12,
        QWRP_AXIS_TYPE_HAT_Z12,
        QWRP_AXIS_TYPE_HAT_W12,

        QWRP_AXIS_TYPE_HAT_X13,
        QWRP_AXIS_TYPE_HAT_Y13,
        QWRP_AXIS_TYPE_HAT_Z13,
        QWRP_AXIS_TYPE_HAT_W13,

        QWRP_AXIS_TYPE_HAT_X14,
        QWRP_AXIS_TYPE_HAT_Y14,
        QWRP_AXIS_TYPE_HAT_Z14,
        QWRP_AXIS_TYPE_HAT_W14,

        QWRP_AXIS_TYPE_HAT_X15,
        QWRP_AXIS_TYPE_HAT_Y15,
        QWRP_AXIS_TYPE_HAT_Z15,
        QWRP_AXIS_TYPE_HAT_W15,

        QWRP_AXIS_TYPE_HAT_X16,
        QWRP_AXIS_TYPE_HAT_Y16,
        QWRP_AXIS_TYPE_HAT_Z16,
        QWRP_AXIS_TYPE_HAT_W16,

        QWRP_AXIS_TYPE_HAT_X17,
        QWRP_AXIS_TYPE_HAT_Y17,
        QWRP_AXIS_TYPE_HAT_Z17,
        QWRP_AXIS_TYPE_HAT_W17,

        QWRP_AXIS_TYPE_HAT_X18,
        QWRP_AXIS_TYPE_HAT_Y18,
        QWRP_AXIS_TYPE_HAT_Z18,
        QWRP_AXIS_TYPE_HAT_W18,

        QWRP_AXIS_TYPE_HAT_X19,
        QWRP_AXIS_TYPE_HAT_Y19,
        QWRP_AXIS_TYPE_HAT_Z19,
        QWRP_AXIS_TYPE_HAT_W19,

        QWRP_AXIS_TYPE_BUTTON = 0x300,
        QWRP_AXIS_TYPE_BUTTON_0,
        QWRP_AXIS_TYPE_BUTTON_1,
        QWRP_AXIS_TYPE_BUTTON_2,
        QWRP_AXIS_TYPE_BUTTON_3,
        QWRP_AXIS_TYPE_BUTTON_4,
        QWRP_AXIS_TYPE_BUTTON_5,
        QWRP_AXIS_TYPE_BUTTON_6,
        QWRP_AXIS_TYPE_BUTTON_7,
        QWRP_AXIS_TYPE_BUTTON_8,
        QWRP_AXIS_TYPE_BUTTON_9,

        QWRP_AXIS_TYPE_BUTTON_A,
        QWRP_AXIS_TYPE_BUTTON_B,
        QWRP_AXIS_TYPE_BUTTON_C,
        QWRP_AXIS_TYPE_BUTTON_D,
        QWRP_AXIS_TYPE_BUTTON_E,
        QWRP_AXIS_TYPE_BUTTON_F,
        QWRP_AXIS_TYPE_BUTTON_G,
        QWRP_AXIS_TYPE_BUTTON_H,
        QWRP_AXIS_TYPE_BUTTON_I,
        QWRP_AXIS_TYPE_BUTTON_J,
        QWRP_AXIS_TYPE_BUTTON_K,
        QWRP_AXIS_TYPE_BUTTON_L,
        QWRP_AXIS_TYPE_BUTTON_M,
        QWRP_AXIS_TYPE_BUTTON_N,
        QWRP_AXIS_TYPE_BUTTON_O,
        QWRP_AXIS_TYPE_BUTTON_P,
        QWRP_AXIS_TYPE_BUTTON_Q,
        QWRP_AXIS_TYPE_BUTTON_R,
        QWRP_AXIS_TYPE_BUTTON_S,
        QWRP_AXIS_TYPE_BUTTON_T,
        QWRP_AXIS_TYPE_BUTTON_U,
        QWRP_AXIS_TYPE_BUTTON_V,
        QWRP_AXIS_TYPE_BUTTON_W,
        QWRP_AXIS_TYPE_BUTTON_X,
        QWRP_AXIS_TYPE_BUTTON_Y,
        QWRP_AXIS_TYPE_BUTTON_Z,

        QWRP_AXIS_TYPE_BUTTON_F1,
        QWRP_AXIS_TYPE_BUTTON_F2,
        QWRP_AXIS_TYPE_BUTTON_F3,
        QWRP_AXIS_TYPE_BUTTON_F4,
        QWRP_AXIS_TYPE_BUTTON_F5,
        QWRP_AXIS_TYPE_BUTTON_F6,
        QWRP_AXIS_TYPE_BUTTON_F7,
        QWRP_AXIS_TYPE_BUTTON_F8,
        QWRP_AXIS_TYPE_BUTTON_F9,
        QWRP_AXIS_TYPE_BUTTON_F10,
        QWRP_AXIS_TYPE_BUTTON_F11,
        QWRP_AXIS_TYPE_BUTTON_F12,
        QWRP_AXIS_TYPE_BUTTON_F13,
        QWRP_AXIS_TYPE_BUTTON_F14,
        QWRP_AXIS_TYPE_BUTTON_F15,

        QWRP_AXIS_TYPE_BUTTON_BACKSPACE,
        QWRP_AXIS_TYPE_BUTTON_TAB,
        QWRP_AXIS_TYPE_BUTTON_CLEAR,
        QWRP_AXIS_TYPE_BUTTON_RETURN,
        QWRP_AXIS_TYPE_BUTTON_PAUSE,
        QWRP_AXIS_TYPE_BUTTON_ESCAPE,
        QWRP_AXIS_TYPE_BUTTON_SPACE,

        QWRP_AXIS_TYPE_BUTTON_KEYPAD0,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD1,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD2,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD3,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD4,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD5,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD6,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD7,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD8,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD9,

        QWRP_AXIS_TYPE_BUTTON_KEYPAD_PERIOD, // del
        QWRP_AXIS_TYPE_BUTTON_KEYPAD_DIVIDE, // /
        QWRP_AXIS_TYPE_BUTTON_KEYPAD_MULTIPLY, // *
        QWRP_AXIS_TYPE_BUTTON_KEYPAD_MINUS, // -
        QWRP_AXIS_TYPE_BUTTON_KEYPAD_PLUS, // +
        QWRP_AXIS_TYPE_BUTTON_KEYPAD_ENTER,
        QWRP_AXIS_TYPE_BUTTON_KEYPAD_EQUALS, // =

        QWRP_AXIS_TYPE_BUTTON_UP,
        QWRP_AXIS_TYPE_BUTTON_DOWN,
        QWRP_AXIS_TYPE_BUTTON_RIGHT,
        QWRP_AXIS_TYPE_BUTTON_LEFT,

        QWRP_AXIS_TYPE_BUTTON_INSERT,
        QWRP_AXIS_TYPE_BUTTON_DELETE,
        QWRP_AXIS_TYPE_BUTTON_HOME,
        QWRP_AXIS_TYPE_BUTTON_END,
        QWRP_AXIS_TYPE_BUTTON_PAGE_UP,
        QWRP_AXIS_TYPE_BUTTON_PAGE_DOWN,

        QWRP_AXIS_TYPE_BUTTON_PRINT,
        QWRP_AXIS_TYPE_BUTTON_SYS_REQ,
        QWRP_AXIS_TYPE_BUTTON_BREAK,

        QWRP_AXIS_TYPE_BUTTON_EXCLAIM, // !
        QWRP_AXIS_TYPE_BUTTON_DOUBLE_QUOTE, // "
        QWRP_AXIS_TYPE_BUTTON_HASH, // #
        QWRP_AXIS_TYPE_BUTTON_DOLLAR, // $
        QWRP_AXIS_TYPE_BUTTON_AMPERSAND, // &
        QWRP_AXIS_TYPE_BUTTON_QUOTE, //'
        QWRP_AXIS_TYPE_BUTTON_LEFT_PAREN, // (
        QWRP_AXIS_TYPE_BUTTON_RIGHT_PAREN, // )
        QWRP_AXIS_TYPE_BUTTON_ASTERISK, // *
        QWRP_AXIS_TYPE_BUTTON_PLUS, // +
        QWRP_AXIS_TYPE_BUTTON_COMMA, // ,
        QWRP_AXIS_TYPE_BUTTON_MINUS, // -
        QWRP_AXIS_TYPE_BUTTON_PERIOD, // .
        QWRP_AXIS_TYPE_BUTTON_SLASH, // /
        QWRP_AXIS_TYPE_BUTTON_COLON, // :
        QWRP_AXIS_TYPE_BUTTON_SEMICOLON, // ;
        QWRP_AXIS_TYPE_BUTTON_LESS, // <
        QWRP_AXIS_TYPE_BUTTON_EQUALS, // =
        QWRP_AXIS_TYPE_BUTTON_GREATER, // >
        QWRP_AXIS_TYPE_BUTTON_QUESTION, // ?
        QWRP_AXIS_TYPE_BUTTON_AT, // @
        QWRP_AXIS_TYPE_BUTTON_LEFT_BRACKET, // [
        QWRP_AXIS_TYPE_BUTTON_RIGHT_BRACKET, // [
        QWRP_AXIS_TYPE_BUTTON_CARET, // ^
        QWRP_AXIS_TYPE_BUTTON_UNDERSCORE, // _
        QWRP_AXIS_TYPE_BUTTON_BACKQUOTE, // `

        QWRP_AXIS_TYPE_BUTTON_NUMLOCK,
        QWRP_AXIS_TYPE_BUTTON_CAPSLOCK,
        QWRP_AXIS_TYPE_BUTTON_SCROLLLOCK,
        QWRP_AXIS_TYPE_BUTTON_RIGHT_SHIFT,
        QWRP_AXIS_TYPE_BUTTON_LEFT_SHIFT,
        QWRP_AXIS_TYPE_BUTTON_RIGHT_CONTROL,
        QWRP_AXIS_TYPE_BUTTON_LEFT_CONTROL,
        QWRP_AXIS_TYPE_BUTTON_RIGHT_ALT,
        QWRP_AXIS_TYPE_BUTTON_LEFT_ALT,

        QWRP_AXIS_TYPE_BUTTON_LEFT_COMMAND,
        QWRP_AXIS_TYPE_BUTTON_LEFT_APPLE,
        QWRP_AXIS_TYPE_BUTTON_LEFT_WINDOWS,
        QWRP_AXIS_TYPE_BUTTON_RIGHT_COMMAND,
        QWRP_AXIS_TYPE_BUTTON_RIGHT_APPLE,
        QWRP_AXIS_TYPE_BUTTON_RIGHT_WINDOWS,

        QWRP_AXIS_TYPE_BUTTON_ALT_GR,
        QWRP_AXIS_TYPE_BUTTON_HELP,
        QWRP_AXIS_TYPE_BUTTON_MENU,

        QWRP_AXIS_TYPE_BUTTON_MOUSE0,
        QWRP_AXIS_TYPE_BUTTON_MOUSE1,
        QWRP_AXIS_TYPE_BUTTON_MOUSE2,
        QWRP_AXIS_TYPE_BUTTON_MOUSE3,
        QWRP_AXIS_TYPE_BUTTON_MOUSE4,
        QWRP_AXIS_TYPE_BUTTON_MOUSE5,
        QWRP_AXIS_TYPE_BUTTON_MOUSE6,
        QWRP_AXIS_TYPE_BUTTON_MOUSE7,

        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_0,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_1,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_2,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_3,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_4,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_5,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_6,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_7,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_8,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_9,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_10,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_11,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_12,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_13,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_14,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_15,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_16,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_17,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_18,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_19,

        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A0 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_0,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A1 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_1,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A2 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_2,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A3 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_3,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A4 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_4,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A5 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_5,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A6 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_6,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A7 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_7,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A8 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_8,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A9 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_9,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A10 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_10,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A11 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_11,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A12 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_12,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A13 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_13,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A14 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_14,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A15 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_15,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A16 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_16,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A17 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_17,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A18 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_18,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_A19 = QWRP_AXIS_TYPE_BUTTON_JOYSTICK_19,

        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B0,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B1,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B2,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B3,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B4,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B5,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B6,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B7,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B8,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B9,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B10,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B11,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B12,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B13,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B14,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B15,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B16,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B17,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B18,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_B19,

        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C0,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C1,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C2,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C3,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C4,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C5,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C6,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C7,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C8,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C9,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C10,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C11,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C12,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C13,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C14,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C15,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C16,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C17,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C18,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_C19,

        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D0,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D1,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D2,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D3,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D4,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D5,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D6,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D7,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D8,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D9,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D10,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D11,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D12,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D13,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D14,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D15,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D16,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D17,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D18,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_D19,

        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E0,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E1,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E2,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E3,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E4,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E5,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E6,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E7,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E8,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E9,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E10,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E11,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E12,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E13,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E14,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E15,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E16,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E17,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E18,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_E19,

        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F0,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F1,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F2,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F3,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F4,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F5,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F6,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F7,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F8,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F9,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F10,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F11,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F12,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F13,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F14,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F15,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F16,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F17,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F18,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_F19,

        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G0,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G1,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G2,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G3,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G4,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G5,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G6,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G7,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G8,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G9,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G10,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G11,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G12,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G13,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G14,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G15,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G16,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G17,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G18,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_G19,

        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H0,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H1,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H2,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H3,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H4,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H5,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H6,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H7,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H8,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H9,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H10,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H11,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H12,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H13,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H14,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H15,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H16,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H17,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H18,
        QWRP_AXIS_TYPE_BUTTON_JOYSTICK_H19,
        //QWRP_AXIS_TYPE_BUTTON_10,
        //QWRP_AXIS_TYPE_BUTTON_11,
        //QWRP_AXIS_TYPE_BUTTON_12,
        //QWRP_AXIS_TYPE_BUTTON_13,
        //QWRP_AXIS_TYPE_BUTTON_14,
        //QWRP_AXIS_TYPE_BUTTON_15,

        QWRP_AXIS_TYPE_FEEDBACK = 0x600,

        QWRP_AXIS_TYPE_FEEDBACK_ = 0x600,
        QWRP_AXIS_TYPE_FEEDBACK_32 = 0x620,

        QWRP_AXIS_TYPE_BUTTON_PPGUN_1 = QWRP_AXIS_TYPE_BUTTON_U,//U
            QWRP_AXIS_TYPE_BUTTON_PPGUN_2 = QWRP_AXIS_TYPE_BUTTON_I,//I
            QWRP_AXIS_TYPE_BUTTON_PPGUN_3 = QWRP_AXIS_TYPE_BUTTON_O,//O
            QWRP_AXIS_TYPE_BUTTON_PPGUN_4 = QWRP_AXIS_TYPE_BUTTON_H,//H
            QWRP_AXIS_TYPE_BUTTON_PPGUN_5 = QWRP_AXIS_TYPE_BUTTON_J,//J
            QWRP_AXIS_TYPE_BUTTON_PPGUN_6 = QWRP_AXIS_TYPE_BUTTON_M,//M
            QWRP_AXIS_TYPE_BUTTON_PPGUN_7 = QWRP_AXIS_TYPE_BUTTON_MOUSE1,//right mouse down
            QWRP_AXIS_TYPE_BUTTON_PPGUN_8 = QWRP_AXIS_TYPE_BUTTON_N,//N
            QWRP_AXIS_TYPE_BUTTON_PPGUN_Fire = QWRP_AXIS_TYPE_BUTTON_MOUSE0,//left mouse down
            QWRP_AXIS_TYPE_BUTTON_PPGUN_EN = QWRP_AXIS_TYPE_BUTTON_KEYPAD_ENTER,//Enter
            QWRP_AXIS_TYPE_BUTTON_PPGUN_W = QWRP_AXIS_TYPE_BUTTON_W,//W
            QWRP_AXIS_TYPE_BUTTON_PPGUN_S = QWRP_AXIS_TYPE_BUTTON_S,//S
            QWRP_AXIS_TYPE_BUTTON_PPGUN_A = QWRP_AXIS_TYPE_BUTTON_A,//A
            QWRP_AXIS_TYPE_BUTTON_PPGUN_D = QWRP_AXIS_TYPE_BUTTON_D,//D
            QWRP_AXIS_TYPE_BUTTON_PPGUN_Space = QWRP_AXIS_TYPE_BUTTON_SPACE,//Space


        QWRP_AXIS_TYPE_JOYSTICK_X,
        QWRP_AXIS_TYPE_JOYSTICK_Y,
        QWRP_AXIS_TYPE_JOYSTICK_Z,

        QWRP_AXIS_TYPE_JOYSTICK_RX,
        QWRP_AXIS_TYPE_JOYSTICK_RY,
        QWRP_AXIS_TYPE_JOYSTICK_RZ,

        QWRP_AXIS_TYPE_JOYSTICK_POV
    };*/

    [StructLayout(LayoutKind.Sequential)]
    public struct QwrnDevicePose
    {
        public qwrQuarterion qRotation;
        public qwrVector3 vecAngularVelocity;
        public qwrVector3 vecAngularAcceleration;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct QwrnDevicePosition
    {
        public qwrVector3 vecPosition;
        public qwrVector3 vecVelocity;
        public qwrVector3 vecAcceleration;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct QwrnDeviceAxis
    {
        public float T;
        public float B;
        public float X;
        public float Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct QwrnDevicePosePosition
    {
        public QwrnDevicePose pose;      //pose.Length = 10
        public QwrnDevicePosition position;     //position.Length = 9
        public int is_valid;
        public int is_connected;
        public double update_time;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct QwrnControllerState
    {
        public uint packet_number;
        public float touch_pad_analog_x;
        public float touch_pad_analog_y;
        public float trigger_analog;
        public ulong button_pressed_flags;
        public ulong button_touched_flags;
        public uint power_button_mode;
        public int is_valid;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct QwrnControllerStateRecord
    {
        public QwrnDevicePosePosition controllerPosePosition;
        public QwrnControllerState controllerState;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct QwrnControllerStateRecordOriginal
    {
        public double time;
        public int connect_state;
        public QwrnDevicePose pose;      //pose.Length = 10
        public QwrnDevicePosition position;     //position.Length = 9
        public int button_mask;
        public QwrnDeviceAxis axis;

        public QwrnControllerStateRecordOriginal(Peripheralstatus status)
        {
            time = status.time_state[1][0];
            connect_state = status.device_status;
            pose.qRotation = new qwrQuarterion(status.pose_state[0][0], status.pose_state[0][1], status.pose_state[0][2], status.pose_state[0][3]);
            pose.vecAngularVelocity = new qwrVector3(status.pose_state[0][4], status.pose_state[0][5], status.pose_state[0][6]);
            pose.vecAngularAcceleration = new qwrVector3(status.pose_state[0][7], status.pose_state[0][8], status.pose_state[0][9]);
            position.vecPosition = new qwrVector3(status.position_state[0][0], status.position_state[0][1], status.position_state[0][2]);
            position.vecVelocity = new qwrVector3(status.position_state[0][3], status.position_state[0][4], status.position_state[0][5]);
            position.vecAcceleration = new qwrVector3(status.position_state[0][6], status.position_state[0][7], status.position_state[0][8]);
            button_mask = status.button_state[(int)QWRP_POLARIS_BUTTONS.QWRP_POLARIS_BUTTONS_YB][0];
            axis.T = status.axis_state[(int)QWRP_POLARIS_AXES.QWRP_POLARIS_AXIS_T][0];
            axis.B = status.axis_state[(int)QWRP_POLARIS_AXES.QWRP_POLARIS_AXIS_B][0];
            axis.X = status.axis_state[(int)QWRP_POLARIS_AXES.QWRP_POLARIS_AXIS_X][0];
            axis.Y = status.axis_state[(int)QWRP_POLARIS_AXES.QWRP_POLARIS_AXIS_Y][0];
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct VRControllerAxis_t
    {
        public float x;
        public float y;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct VRControllerState_t
    {
        public uint unPacketNum;
        public ulong ulButtonPressed;
        public ulong ulButtonTouched;
        public VRControllerAxis_t rAxis0; //VRControllerAxis_t[5]
        public VRControllerAxis_t rAxis1;
        public VRControllerAxis_t rAxis2;
        public VRControllerAxis_t rAxis3;
        public VRControllerAxis_t rAxis4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TrackedDevicePose_t
    {
        public qwrVector3 vVelocity;
        public qwrVector3 vAngularVelocity;
        [MarshalAs(UnmanagedType.I1)]
        public bool bPoseIsValid;
        [MarshalAs(UnmanagedType.I1)]
        public bool bDeviceIsConnected;
    }
}

