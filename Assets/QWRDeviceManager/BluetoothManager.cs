using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class BluetoothManager
        {
            public enum DeviceBondState {
                DEVICE_BOND_NONE=10,DEVICE_BONDING,DEVICE_BONDED,DEVICE_BOND_UNSUPPORT
            }
            public const int deviceBondNone = (int)DeviceBondState.DEVICE_BOND_NONE;
            public const int deviceBonding = (int)DeviceBondState.DEVICE_BONDING;
            public const int deviceBonded = (int)DeviceBondState.DEVICE_BONDED;

            public enum BluetoothState{
                STATE_OFF=10, STATE_TURNING_ON, STATE_ON, STATE_TURNING_OFF, STATE_BLE_TURNING_ON,
                STATE_BLE_ON, STATE_BLE_TURNING_OFF
            }
            public const int bluetoothStateOff = (int)BluetoothState.STATE_OFF;
            public const int bluetoothStateTurningOn = (int)BluetoothState.STATE_BLE_TURNING_ON;
            public const int bluetoothStateOn = (int)BluetoothState.STATE_ON;
            public const int bluetoothStateTurningOff = (int)BluetoothState.STATE_TURNING_OFF;

            public enum BTDeviceClass { AUDIO_VIDEO=0x0400, PERIPHERAL= 0x0500 }

            public static bool IsBluetoothEnabled()
            {
                return GetDeviceManager().Call<bool>("isBluetoothEnabled");
            }

            public static void OpenAndroidBluetoothSetting()
            {
                GetDeviceManager().Call("openAndroidBluetoothSetting");
            }

            public static void RegisterBluetoothCallback(OnBtLocalStateCallback btAdapter,
                                                         OnBtLocalScanCallback btDevice,
                                                         OnBtDeviceBondCallback btDeviceBond,
                                                         OnBtDeviceConnectionStateCallback btConDeviceState)
            {
                GetDeviceManager().Call("registerBluetoothReceiver", new BluetoothCallback(btAdapter, btDevice,
                    btDeviceBond, btConDeviceState));
            }

            /// <summary>
            /// Warning⚠:this method will remove all listeners registered by <see cref="RegisterBluetoothCallback"/>
            /// </summary>
            public static void UnregisterBluetoothCallback()
            {
                GetDeviceManager().Call("unregisterReceiver");
            }

            /// <summary>
            /// get connected device name list
            /// </summary>
            /// <returns></returns>
            public static string[] GetConnectedDeviceNameList()
            {
                return GetDeviceManager().Call<string[]>("getConnectedDeviceNameList");
            }

            /// <summary>
            /// get current connected device list
            /// </summary>
            /// <returns></returns>
            public static string GetConnectedDevices()
            {
                return GetDeviceManager().Call<string>("getConnectedDevices");
            }

            /// <summary>
            /// enable/disable local bluetooth
            /// </summary>
            /// <param name="enable"></param>
            /// <returns></returns>
            public static bool SetBluetoothEnabled(bool enable)
            {
                return GetDeviceManager().Call<bool>("setBluetoothEnabled", enable);
            }

            /// <summary>
            /// get bluetooth state
            /// </summary>
            /// <returns><see cref="BluetoothState"/></returns>
            public static int GetBluetoothState()
            {
                return GetDeviceManager().Call<int>("getBluetoothState");
            }

            /// <summary>
            /// Get the friendly Bluetooth name of the local Bluetooth adapter.
            /// This name is visible to remote Bluetooth devices.
            /// </summary>
            /// <returns>the Bluetooth name, or null on error</returns>
            public static string GetLocalName()
            {
                return GetDeviceManager().Call<string>("getLocalName");
            }

            /// <summary>
            /// Set the friendly Bluetooth name of the local Bluetooth adapter.
            /// </summary>
            /// <param name="localName">a valid Bluetooth name</param>
            /// <returns>true if the name was set, false otherwise</returns>
            public static bool SetLocalName(string localName)
            {
                return GetDeviceManager().Call<bool>("setLocalName", localName);
            }

            /// <summary>
            /// Start the remote device discovery process.
            /// The discovery process usually involves an inquiry scan of about 12 seconds.
            /// This is an asynchronous call, it will return immediately. 
            /// </summary>
            /// <returns>return true on success, false on error</returns>
            public static bool StartDiscovery()
            {
                return GetDeviceManager().Call<bool>("startDiscovery");
            }

            /// <summary>
            /// Return true if the local Bluetooth adapter is currently in the device discovery process.
            /// </summary>
            /// <returns>true if discovering</returns>
            public static bool IsDiscovering()
            {
                return GetDeviceManager().Call<bool>("isDiscovering");
            }

            /// <summary>
            /// Cancel the current device discovery process.
            /// </summary>
            /// <returns>true on success, false on error</returns>
            public static bool CancelDiscovery()
            {
                return GetDeviceManager().Call<bool>("cancelDiscovery");
            }

            /// <summary>
            /// Return the json string of objects that are bonded (paired) to the local adapter.
            /// </summary>
            /// <returns>list of BluetoothBondDevice, or null on error</returns>
            public static string GetBluetoothBondedDevices()
            {
                return GetDeviceManager().Call<string>("getBluetoothBondedDevices");
            }

            /// <summary>
            /// Connect bluetooth device by address, only support DeviceClass in <see cref="BTDeviceClass"/>
            /// </summary>
            /// <param name="address">device address</param>
            /// <returns></returns>
            public static bool ConnectBluetoothDevice(string address)
            {
                return GetDeviceManager().Call<bool>("connectBluetoothDevice",address);
            }

            public static void connectA2dp(string address){
                GetDeviceManager().Call("connectA2dp",address);
            }

            /// <summary>
            /// remove bluetooth device
            /// </summary>
            /// <param name="address"></param>
            /// <returns></returns>
            public static bool RemoveBluetoothDevice(string address)
            {
                return GetDeviceManager().Call<bool>("removeBluetoothDevice", address);
            }

            // local bluetooth on/off state
            public delegate void OnBtLocalStateCallback(int state);

            /// <summary>
            /// callback when bluetooth device scan finished
            /// </summary>
            /// <param name="scanState"></param>
            /// <param name="deviceInfo">exp: {"DeviceClass":1024,"address":"30:21:30:45:17:7B","name":"ifkoo Q8","rssi":-39} </param>
            public delegate void OnBtLocalScanCallback(bool scanState, string deviceInfo);

            /// <summary>
            /// device bonding state <see cref="DeviceBondState"/>
            /// </summary>
            /// <param name="deviceName"></param>
            /// <param name="bondState"></param>
            public delegate void OnBtDeviceBondCallback(string deviceName, string address, int bondState);

            public delegate void OnBtDeviceConnectionStateCallback(string name, string address, int preState, int currentState);

            class BluetoothCallback : AndroidJavaProxy
            {
                OnBtLocalStateCallback btLocalStateHandler;
                OnBtLocalScanCallback btLocalScanHandler;
                OnBtDeviceBondCallback btDeviceBondHandler;
                OnBtDeviceConnectionStateCallback btDeviceConnectionStateHandler;
                public BluetoothCallback(OnBtLocalStateCallback btAdapter, OnBtLocalScanCallback btDevice,
                    OnBtDeviceBondCallback btDeviceBond, OnBtDeviceConnectionStateCallback btDeviceConStateCallback)
                    : base("com.qwr.DeviceManager.PluginCallback$BluetoothCallback")
                {
                    this.btLocalStateHandler = btAdapter;
                    this.btLocalScanHandler = btDevice;
                    this.btDeviceBondHandler = btDeviceBond;
                    this.btDeviceConnectionStateHandler = btDeviceConStateCallback;
                }

                void OnBtLocalStateCallback(int state)
                {
                    if (btLocalStateHandler != null)
                        btLocalStateHandler(state);
                }
                void OnBtLocalScanCallback(bool scanState, string deviceInfo)
                {
                    if (btLocalScanHandler != null)
                        btLocalScanHandler(scanState, deviceInfo);
                }
                void OnBtDeviceBondCallback(string name, string address, int bondState)
                {
                    if (btDeviceBondHandler != null)
                        btDeviceBondHandler(name, address, bondState);
                }
                void OnBtDeviceConnectionStateCallback(string name, string address, int preState, int currentState)
                {
                    if (btDeviceConnectionStateHandler != null)
                        btDeviceConnectionStateHandler(name, address, preState, currentState);
                }
            }
        }
    }
}
