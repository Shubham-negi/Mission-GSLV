/************************************************************************************

Copyright: Copyright(c) 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace qwr
{
    public class PeripheralInfo
    {
        public int pose_count;
        public int position_count;
        public int time_count;
        public int quaternion_count;
        public int vector_count;
        public int axis_count;
        public int button_count;
        public int attribute_count;
        public int feedback_count;
    }
    public class Peripheralstatus
    {
        public ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        
        public int device_status;
        public float[][] pose_state;
        public float[][] position_state;
        public double[][] time_state;
        public float[][] quaternion_state;
        public float[][] vector_state;
        public float[][] axis_state;
        public int[][] button_state;

        public void CopyFrom(Peripheralstatus other)
        {
            if (other == null || this == null)
            {
                return;
            }
            other.cacheLock.EnterReadLock();
            this.cacheLock.EnterWriteLock();
            try
            {
                device_status = other.device_status;
                if (other.pose_state != null)
                {
                    if (pose_state == null)
                    {
                        pose_state = new float[other.pose_state.Length][];
                        for (int i = 0; i < other.pose_state.Length; i++)
                        {
                            pose_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_POSE];
                        }
                    }
                    for (int i = 0; i < other.pose_state.Length; i++)
                    {
                        other.pose_state[i].CopyTo(pose_state[i], 0);
                    }
                }
                else
                {
                    pose_state = null;
                }
                if (other.position_state != null)
                {
                    if (position_state == null)
                    {
                        position_state = new float[other.position_state.Length][];
                        for (int i = 0; i < other.position_state.Length; i++)
                        {
                            position_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_POSITION];
                        }
                    }
                    for (int i = 0; i < other.position_state.Length; i++)
                    {
                        other.position_state[i].CopyTo(position_state[i], 0);
                    }
                }
                else
                {
                    position_state = null;
                }
                if (other.time_state != null)
                {
                    if (time_state == null)
                    {
                        time_state = new double[other.time_state.Length][];
                        for (int i = 0; i < other.time_state.Length; i++)
                        {
                            time_state[i] = new double[QwrpUnity.QWRP_VALUE_TYPE_SIZE_TIME];
                        }
                    }
                    for (int i = 0; i < other.time_state.Length; i++)
                    {
                        other.time_state[i].CopyTo(time_state[i], 0);
                    }
                }
                else
                {
                    time_state = null;
                }
                if (other.quaternion_state != null)
                {
                    if (quaternion_state == null)
                    {
                        quaternion_state = new float[other.quaternion_state.Length][];
                        for (int i = 0; i < other.quaternion_state.Length; i++)
                        {
                            quaternion_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_QUATERNION];
                        }
                    }
                    for (int i = 0; i < other.quaternion_state.Length; i++)
                    {
                        other.quaternion_state[i].CopyTo(quaternion_state[i], 0);
                    }
                }
                else
                {
                    quaternion_state = null;
                }
                if (other.vector_state != null)
                {
                    if (vector_state == null)
                    {
                        vector_state = new float[other.vector_state.Length][];
                        for (int i = 0; i < other.vector_state.Length; i++)
                        {
                            vector_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_VECTOR];
                        }
                    }
                    for (int i = 0; i < other.vector_state.Length; i++)
                    {
                        other.vector_state[i].CopyTo(vector_state[i], 0);
                    }
                }
                else
                {
                    vector_state = null;
                }
                if (other.axis_state != null)
                {
                    if (axis_state == null)
                    {
                        axis_state = new float[other.axis_state.Length][];
                        for (int i = 0; i < other.axis_state.Length; i++)
                        {
                            axis_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_AXIS];
                        }
                    }
                    for (int i = 0; i < other.axis_state.Length; i++)
                    {
                        other.axis_state[i].CopyTo(axis_state[i], 0);
                    }
                }
                else
                {
                    axis_state = null;
                }
                if (other.button_state != null)
                {
                    if (button_state == null)
                    {
                        button_state = new int[other.button_state.Length][];
                        for (int i = 0; i < other.button_state.Length; i++)
                        {
                            button_state[i] = new int[QwrpUnity.QWRP_VALUE_TYPE_SIZE_BUTTON];
                        }
                    }
                    for (int i = 0; i < other.button_state.Length; i++)
                    {
                        other.button_state[i].CopyTo(button_state[i], 0);
                    }

                }
                else
                {
                    button_state = null;
                }
            }
            finally
            {
                other.cacheLock.ExitReadLock();
                this.cacheLock.ExitWriteLock();
            }
        }
    }

    public class QwrPeripheral
    {
        private QwrPeripheral(IntPtr device, string deviceId) { _device = device; _deviceId = deviceId; }

        public IntPtr _device
        {
            get;
            private set;
        }

        public string _deviceId
        {
            get;
            private set;
        }

        public PeripheralInfo peripheralInfo = null;

        public Peripheralstatus prevperipheralstatus = null;

        public Peripheralstatus peripheralstatus = null;
        
        public void QwrupGetDeviceInfo ()
        {
            if (_device == IntPtr.Zero)
                return;
            peripheralInfo = new PeripheralInfo();
            peripheralInfo.pose_count = QwrupGetDevicePoseCount();
            peripheralInfo.position_count = QwrupGetDevicePositionCount();
            peripheralInfo.time_count = QwrupGetDeviceTimeCount();
            peripheralInfo.quaternion_count = QwrupGetDeviceQuaternionCount();
            peripheralInfo.vector_count = QwrupGetDeviceVectorCount();
            peripheralInfo.axis_count = QwrupGetDeviceAxisCount();
            peripheralInfo.button_count = QwrupGetDeviceButtonCount();

            peripheralstatus = new Peripheralstatus();
            peripheralstatus.device_status = 0;
            if (peripheralInfo.pose_count != 0)
            {
                peripheralstatus.pose_state = new float[peripheralInfo.pose_count][];
                for (int i = 0; i < peripheralInfo.pose_count; i++)
                {
                    peripheralstatus.pose_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_POSE];
                }
            }
            else
            {
                peripheralstatus.pose_state = null;
            }

            if (peripheralInfo.position_count != 0)
            {
                peripheralstatus.position_state = new float[peripheralInfo.position_count][];
                for (int i = 0; i < peripheralInfo.position_count; i++)
                {
                    peripheralstatus.position_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_POSITION];
                }
            }
            else
            {
                peripheralstatus.position_state = null;
            }

            if (peripheralInfo.time_count != 0)
            {
                peripheralstatus.time_state = new double[peripheralInfo.time_count][];
                for (int i = 0; i < peripheralInfo.time_count; i++)
                {
                    peripheralstatus.time_state[i] = new double[QwrpUnity.QWRP_VALUE_TYPE_SIZE_TIME];
                }
            }
            else
            {
                peripheralstatus.time_state = null;
            }

            if (peripheralInfo.quaternion_count != 0)
            {
                peripheralstatus.quaternion_state = new float[peripheralInfo.quaternion_count][];
                for (int i = 0; i < peripheralInfo.quaternion_count; i++)
                {
                    peripheralstatus.quaternion_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_QUATERNION];
                }
            }
            else
            {
                peripheralstatus.quaternion_state = null;
            }

            if (peripheralInfo.vector_count != 0)
            {
                peripheralstatus.vector_state = new float[peripheralInfo.vector_count][];
                for (int i = 0; i < peripheralInfo.vector_count; i++)
                {
                    peripheralstatus.vector_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_VECTOR];
                }
            }
            else
            {
                peripheralstatus.vector_state = null;
            }

            if (peripheralInfo.axis_count != 0)
            {
                peripheralstatus.axis_state = new float[peripheralInfo.axis_count][];
                for (int i = 0; i < peripheralInfo.axis_count; i++)
                {
                    peripheralstatus.axis_state[i] = new float[QwrpUnity.QWRP_VALUE_TYPE_SIZE_AXIS];
                }
            }
            else
            {
                peripheralstatus.axis_state = null;
            }

            if (peripheralInfo.button_count != 0)
            {
                Debug.Log("button number " + peripheralInfo.button_count);
                peripheralstatus.button_state = new int[peripheralInfo.button_count][];
                for (int i = 0; i < peripheralInfo.button_count; i++)
                {
                    peripheralstatus.button_state[i] = new int[QwrpUnity.QWRP_VALUE_TYPE_SIZE_BUTTON];
                }
            }
            else
            {
                peripheralstatus.button_state = null;
            }
            prevperipheralstatus = new Peripheralstatus();
            prevperipheralstatus.CopyFrom(peripheralstatus);
        }

        private int FrameNum = -1;
        public void QwrupUpdateDeviceState()
        {
            if (_device == IntPtr.Zero)
            {
                return;
            }
            if (Time.frameCount == FrameNum)
            {
                return;
            }
            else
            {
                FrameNum = Time.frameCount;
            }

            prevperipheralstatus.CopyFrom(peripheralstatus);
            peripheralstatus.cacheLock.EnterWriteLock();
            try
            {
                IntPtr temp = Marshal.AllocHGlobal(sizeof(int));
                QwrupReadDeviceAttribute(QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE_DEVICE_STATUS - QWRP_VALUE_TYPE.QWRP_VALUE_TYPE_ATTRIBUTE
                    , temp, sizeof(int));
                peripheralstatus.device_status = Marshal.ReadInt32(temp);
                Marshal.FreeHGlobal(temp);
                if (peripheralInfo.pose_count > 0)
                {
                    for (int i = 0; i < peripheralInfo.pose_count; i++)
                    {
                        float[] pose = peripheralstatus.pose_state[i];
                        QwrupReadDevicePose(i, pose);
                    }
                }
                if (peripheralInfo.position_count > 0)
                {
                    for (int i = 0; i < peripheralInfo.position_count; i++)
                    {
                        float[] position = peripheralstatus.position_state[i];
                        QwrupReadDevicePosition(i, position);
                    }
                }

                if (peripheralInfo.time_count > 0)
                {
                    for (int i = 0; i < peripheralInfo.time_count; i++)
                    {
                        double[] time = peripheralstatus.time_state[i];
                        time[0] = QwrupReadDeviceTime(i);
                    }
                }

                if (peripheralInfo.quaternion_count > 0)
                {
                    for (int i = 0; i < peripheralInfo.quaternion_count; i++)
                    {
                        float[] quaternion = peripheralstatus.quaternion_state[i];
                        QwrupReadDeviceQuaternion(i, quaternion);
                    }
                }

                if (peripheralInfo.vector_count > 0)
                {
                    for (int i = 0; i < peripheralInfo.vector_count; i++)
                    {
                        float[] vector = peripheralstatus.vector_state[i];
                        QwrupReadDeviceVector(i, vector);
                    }
                }

                if (peripheralInfo.axis_count > 0)
                {
                    for (int i = 0; i < peripheralInfo.axis_count; i++)
                    {
                        float[] axis = peripheralstatus.axis_state[i];
                        axis[0] = QwrupReadDeviceAxis(i);
                    }
                }

                if (peripheralInfo.button_count > 0)
                {
                    for (int i = 0; i < peripheralInfo.button_count; i++)
                    {
                        int[] button = peripheralstatus.button_state[i];
                        button[0] = QwrupReadDeviceButton(i);
                    }
                }
            }
            finally
            {
                peripheralstatus.cacheLock.ExitWriteLock();
            }
        }

        public static void QwrpHandleDeviceEvent(IntPtr device, int event_mask, IntPtr user_data)
        {
            QwrPeripheral dev = null;
            foreach (KeyValuePair<string, PeripheralList> i in QwrDevice.GetPeripherals())
            {
                if(i.Value.peripheral._device == device)
                {
                    dev = i.Value.peripheral;
                    break;
                }
            }

            dev.peripheralstatus.cacheLock.EnterWriteLock();
            try
            {
                if ((event_mask & ((int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_CONNECT | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_DISCONNECT | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_USB_PLUGIN |
                (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_USB_UNPLUG | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_HDMI_PLUGIN | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_HDMI_UNPLUG |
                (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_A_TRACK | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_A_UNTRACK | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_B_TRACK |
                (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_B_UNTRACK)) != 0x0)
                {
                    // set device connection state
                    //
                    if ((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_HDMI_PLUGIN) != 0x0)
                    {
                        dev.peripheralstatus.device_status |= (int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_HDMI_PLUGIN;
                        dev.peripheralstatus.device_status &= ~(int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_HDMI_UNPLUG;
                    }

                    if ((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_USB_PLUGIN) != 0x0)
                    {
                        dev.peripheralstatus.device_status |= (int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_USB_PLUGIN;
                        dev.peripheralstatus.device_status &= ~(int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_USB_UNPLUG;
                    }

                    if ((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_HDMI_UNPLUG) != 0x0)
                    {
                        dev.peripheralstatus.device_status |= (int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_HDMI_UNPLUG;
                        dev.peripheralstatus.device_status &= ~(int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_HDMI_PLUGIN;
                    }

                    if ((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_USB_UNPLUG) != 0x0)
                    {
                        dev.peripheralstatus.device_status |= (int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_USB_UNPLUG;
                        dev.peripheralstatus.device_status &= ~(int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_USB_PLUGIN;
                    }

                    if ((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_A_TRACK) != 0x0)
                    {
                        dev.peripheralstatus.device_status |= (int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_BASE_A_TRACK;
                        dev.peripheralstatus.device_status &= ~(int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_BASE_A_UNTRACK;
                    }

                    if ((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_A_UNTRACK) != 0x0)
                    {
                        dev.peripheralstatus.device_status |= (int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_BASE_A_UNTRACK;
                        dev.peripheralstatus.device_status &= ~(int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_BASE_A_TRACK;
                    }

                    if ((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_B_TRACK) != 0x0)
                    {
                        dev.peripheralstatus.device_status |= (int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_BASE_B_TRACK;
                        dev.peripheralstatus.device_status &= ~(int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_BASE_B_UNTRACK;
                    }

                    if ((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_B_UNTRACK) != 0x0)
                    {
                        dev.peripheralstatus.device_status |= (int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_BASE_B_UNTRACK;
                        dev.peripheralstatus.device_status &= ~(int)QWRP_DEVICE_STATUS.QWRP_DEVICE_STATUS_BASE_B_TRACK;
                    }
                }
                if (((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_POSE_UPDATE) != 0) && (dev.peripheralInfo.pose_count > 0))
                {
                    for (int i = 0; i < dev.peripheralInfo.pose_count; i++)
                    {
                        float[] pose = dev.peripheralstatus.pose_state[i];
                        dev.QwrupReadDevicePose(i, pose);
                    }
                }

                if (((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_POSITION_UPDATE) != 0) && (dev.peripheralInfo.position_count > 0))
                {
                    for (int i = 0; i < dev.peripheralInfo.position_count; i++)
                    {
                        float[] position = dev.peripheralstatus.position_state[i];
                        dev.QwrupReadDevicePosition(i, position);
                    }
                }

                if (((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_TIME_UPDATE) != 0) && (dev.peripheralInfo.time_count > 0))
                {
                    for (int i = 0; i < dev.peripheralInfo.time_count; i++)
                    {
                        double[] time = dev.peripheralstatus.time_state[i];
                        time[0] = dev.QwrupReadDeviceTime(i);
                    }
                }

                if (((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_QUATERNION_UPDATE) != 0) && (dev.peripheralInfo.quaternion_count > 0))
                {
                    for (int i = 0; i < dev.peripheralInfo.quaternion_count; i++)
                    {
                        float[] quaternion = dev.peripheralstatus.quaternion_state[i];
                        dev.QwrupReadDeviceQuaternion(i, quaternion);
                    }
                }

                if (((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_VECTOR_UPDATE) != 0) && (dev.peripheralInfo.vector_count > 0))
                {
                    for (int i = 0; i < dev.peripheralInfo.vector_count; i++)
                    {
                        float[] vector = dev.peripheralstatus.vector_state[i];
                        dev.QwrupReadDeviceVector(i, vector);
                    }
                }

                if (((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_AXIS_UPDATE) != 0) && (dev.peripheralInfo.axis_count > 0))
                {
                    for (int i = 0; i < dev.peripheralInfo.axis_count; i++)
                    {
                        float[] axis = dev.peripheralstatus.axis_state[i];
                        axis[0] = dev.QwrupReadDeviceAxis(i);
                    }
                }

                if (((event_mask & (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BUTTON_UPDATE) != 0) && (dev.peripheralInfo.button_count > 0))
                {
                    for (int i = 0; i < dev.peripheralInfo.button_count; i++)
                    {
                        int[] button = dev.peripheralstatus.button_state[i];
                        button[0] = dev.QwrupReadDeviceButton(i);
                    }
                }
            }
            finally
            {
                dev.peripheralstatus.cacheLock.ExitWriteLock();
            }
        }

        public static int QwrupQueryDeviceCount(QWRP_DEVICE_TYPE type)
        {
            return QwrpUnity.QwrupQueryDeviceCount(type);
        }
        public static string QwrupGetDeviceId(QWRP_DEVICE_TYPE type, int index)
        {
            return Marshal.PtrToStringAnsi(QwrpUnity.QwrupGetDeviceId(type, index));
        }
        public static QwrPeripheral QwrupOpenDevice(string deviceId)
        {
            IntPtr device = IntPtr.Zero;
            if (deviceId == null)
            {
                return null;
            }
            if (deviceId != string.Empty)
            {
                device = QwrpUnity.QwrupOpenDevice(Marshal.StringToHGlobalAnsi(deviceId));
                return device != IntPtr.Zero ? new QwrPeripheral(device, deviceId) : null;
            }
            else
            {
                return new QwrPeripheral(device, deviceId);
            }
        }
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public static QwrpUnity.QwrpHandleEventFunc event_callback = new QwrpUnity.QwrpHandleEventFunc(QwrpHandleDeviceEvent);
        public static QwrPeripheral OpenPeripheralDevice(string deviceId)
        {
            QwrPeripheral peripheral = QwrPeripheral.QwrupOpenDevice(deviceId);
            if (peripheral != null)
            {
                if (deviceId != string.Empty)
                {
                    Debug.Log("OpenDevice " + deviceId + " successes.");
                }
                peripheral.QwrupGetDeviceInfo();
                peripheral.QwrupUpdateDeviceState();
                //int event_mask = (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_POSE_UPDATE | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_POSITION_UPDATE | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_TIME_UPDATE |
   //(int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_AXIS_UPDATE | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BUTTON_UPDATE | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_CONNECT | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_DISCONNECT |
    //(int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_HDMI_PLUGIN | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_HDMI_UNPLUG | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_USB_PLUGIN | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_USB_PLUGIN |
    //(int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_USB_UNPLUG | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_A_TRACK |
    //(int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_A_UNTRACK | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_B_TRACK | (int)QWRP_EVENT_TYPE.QWRP_EVENT_TYPE_BASE_B_UNTRACK;
                //peripheral.QwrupRegisterEventNotificationFunction(null, event_mask, IntPtr.Zero);
                return peripheral;
            }
            else
            {
                Debug.Log("OpenDevice " + deviceId + " fail!");
                return null;
            }
        }
        public static void ClosePeripheralDevice(QwrPeripheral peripheral)
        {
            //peripheral.QwrupRegisterEventNotificationFunction(null, 0, IntPtr.Zero);
            peripheral.QwrupCloseDevice();
        }
        public void QwrupResume()
	    {
            if (_device != IntPtr.Zero)
            {
                QwrpUnity.QwrupResume(_device);
            }
	    }
	    public void QwrupPause()
	    {
            if (_device != IntPtr.Zero)
            {
                QwrpUnity.QwrupPause(_device);
            }
	    }
	    public void QwrupCloseDevice()
	    {
            if (_device != IntPtr.Zero)
            {
                QwrpUnity.QwrupCloseDevice(_device);
                _device = IntPtr.Zero;
            }
        }
        public int QwrupGetDeviceButtonCount()
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupGetDeviceButtonCount(_device);
		    return 0;
	    }
	    public int QwrupGetDeviceAxisCount()
	    {
		    if(_device!=IntPtr.Zero)
			    return QwrpUnity.QwrupGetDeviceAxisCount (_device);
		    return 0;
	    }
	    public int QwrupGetDevicePoseCount()
	    {
		    if(_device!=IntPtr.Zero)
			    return QwrpUnity.QwrupGetDevicePoseCount (_device);
		    return 0;
	    }
	    public int QwrupGetDevicePositionCount()
	    {
		    if(_device!=IntPtr.Zero)
			    return QwrpUnity.QwrupGetDevicePositionCount (_device);
		    return 0;
	    }
	    public int QwrupGetDeviceTimeCount()
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupGetDeviceTimeCount(_device);
		    return 0;
	    }
	    public int QwrupGetDeviceQuaternionCount()
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupGetDeviceQuaternionCount(_device);
		    return 0;
	    }
	    public int QwrupGetDeviceVectorCount()
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupGetDeviceVectorCount(_device);
		    return 0;
	    }
	    public int QwrupGetDeviceAttributeCount()
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupGetDeviceAttributeCount(_device);
		    return 0;
	    }
	    public int QwrupGetDeviceFeedbackCount()
	    {
		    if(_device!=IntPtr.Zero)
			    return QwrpUnity.QwrupGetDeviceFeedbackCount (_device);
		    return 0;
	    }
	    public string QwrupReadDeviceId()
	    {
		    if(_device!=IntPtr.Zero)
			    return Marshal.PtrToStringAnsi(QwrpUnity.QwrupReadDeviceId (_device));
		    return "";
	    }
	    public string QwrupGetAssociatedDevice(QWRP_DEVICE_TYPE type)
	    {
		    if(_device!=IntPtr.Zero)
			    return Marshal.PtrToStringAnsi(QwrpUnity.QwrupGetAssociatedDevice (_device,type));
		    return "";
	    }
	    public bool QwrupReadDevicePose(int index, float[] pose)
	    {
		    if (pose.Length < QwrpUnity.QWRP_VALUE_TYPE_SIZE_POSE) return false;
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupReadDevicePose(_device, index, pose);
		    return false;
	    }
	    public bool QwrupReadDevicePosition(int index, float[] position)
	    {
		    if (position.Length < QwrpUnity.QWRP_VALUE_TYPE_SIZE_POSITION) return false;
		    if (_device != IntPtr.Zero)
		    {
			    return QwrpUnity.QwrupReadDevicePosition(_device, index, position);
		    }
		    return false;
	    }
	    public double QwrupReadDeviceTime(int index)
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupReadDeviceTime(_device, index);
		    return 0;
	    }
	    public bool QwrupReadDeviceQuaternion(int index, float[] quaternion)
	    {
		    if (quaternion.Length < QwrpUnity.QWRP_VALUE_TYPE_SIZE_QUATERNION) return false;
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupReadDeviceQuaternion(_device, index, quaternion);
		    return false;
	    }
	    public bool QwrupReadDeviceVector(int index, float[] vector)
	    {
		    if (vector.Length < QwrpUnity.QWRP_VALUE_TYPE_SIZE_VECTOR) return false;
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupReadDeviceVector(_device, index, vector);
		    return false;
	    }
	    public float QwrupReadDeviceAxis(int index)
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupReadDeviceAxis(_device, index);
		    return 0.0f;
	    }
	    public int QwrupReadDeviceButton(int index)
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupReadDeviceButton (_device, index);
		    return 0;
	    }
	    public int QwrupReadDeviceAttribute(int index, IntPtr buffer, int buffer_length)
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupReadDeviceAttribute(_device, index, buffer, buffer_length);
		    return 0;
	    }
	    public int QwrupSetDeviceAttribute(int index, IntPtr buffer, int buffer_length)
	    {
		    if (_device != IntPtr.Zero)
			    return QwrpUnity.QwrupSetDeviceAttribute(_device, index, buffer, buffer_length);
		    return 0;
	    }
	    public void QwrupResetDevicePose(int index, float[] pose)
	    {
		    if (pose.Length < QwrpUnity.QWRP_VALUE_TYPE_SIZE_POSE) return;
		    if (_device != IntPtr.Zero) 
		    {
			    QwrpUnity.QwrupResetDevicePose(_device, index, pose);
		    }
	    }
	    public void QwrupResetDevicePosition(int index, float[] position)
	    {
		    if (position.Length < QwrpUnity.QWRP_VALUE_TYPE_SIZE_POSITION) return;
		    if (_device != IntPtr.Zero) 
		    {
			    QwrpUnity.QwrupResetDevicePosition(_device, index, position);
		    }
	    }
	    public void QwrupRegisterReferencePoseFunction(int index,IntPtr callback,IntPtr userData)
	    {
		    if (_device != IntPtr.Zero) 
			    QwrpUnity.QwrupRegisterReferencePoseFunction (_device,index,callback,userData);
	    }
	    public void QwrupRegisterRererencePositionFunction(int index,IntPtr callback,IntPtr userData)
	    {
		    if (_device != IntPtr.Zero) 
			    QwrpUnity.QwrupRegisterRererencePositionFunction (_device,index,callback,userData);
	    }
	    public void QwrupRegisterEventNotificationFunction(QwrpUnity.QwrpHandleEventFunc callback, int event_mask, IntPtr userData)
	    {
		    if (_device != IntPtr.Zero)
			    QwrpUnity.QwrupRegisterEventNotificationFunction(_device, callback, event_mask, userData);
	    }
	    public void QwrupWriteDeviceFeedback(int index,float value)
	    {
		    if (_device != IntPtr.Zero) 
			    QwrpUnity.QwrupWriteDeviceFeedback (_device,index,value);
	    }
        public void QwrupHandleControllerData(int hand, ref QwrnControllerStateRecordOriginal src, ref QwrnControllerStateRecord dest)
        {
            if (_device != IntPtr.Zero)
                QwrpUnity.QwrupHandleControllerData(_device, hand, ref src, ref dest);
        }

        public void QwrupBootPair(bool value)
        {
            if (_device != IntPtr.Zero)
                QwrpUnity.QwrupBootPair(_device, value);
        }

        public void QwrupSetUnbindDevice()
        {
            if(_device != IntPtr.Zero)
            {
                QwrpUnity.QwrupSetUnbindDevice(_device);
            }
        }

        public string QwrupGetBondDeviceName()
        {
            if (_device != IntPtr.Zero)
            {
                byte[] name = new byte[1024];
                name[0] = 0;
                QwrpUnity.QwrupGetBondDeviceName(_device, ref name[0]);
                return System.Text.Encoding.Default.GetString(name, 0, name.Length);
            }
            return "";
        }
    }
}
