using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class WifiManager
        {
            // WifiState
            public const int wifiStateDisabling = 0;
            public const int wifiStateDisabled = 1;
            public const int wifiStateEnabling = 2;
            public const int wifiStateEnabled = 3;
            public const int wifiStateUnknow = 4;

            // WificipherType
            public const int wificipherWep = 0;
            public const int wificipherWpa = 1;
            public const int wificipherNopass = 2;
            public const int wificipherInvalid = 3;

            // NetworkState
            public const int netStateConnecting = 0;
            public const int netStateConnected = 1;
            public const int netStateSuspended = 2;
            public const int netStateDisconnecting = 3;
            public const int netStateDisconnected = 4;
            public const int netStateUnknow = 5;

            public delegate void OnWifiStateChangeHandler(int state);
            public delegate void OnNetworkStateChangeHandler(string ssid, string bssid, int state);
            public delegate void OnRssiChangeHandler(string ssid, int rssi);

            public static void OpenWifiSettings()
            {
                GetDeviceManager().Call("openAndroidWifiSetting");
            }

            /// <summary>
            /// wifi is connected or not
            /// </summary>
            /// <returns></returns>
            public static bool IsNetworkAvailable()
            {
                return GetDeviceManager().Call<bool>("isNetworkAvailable");
            }

            public static int GetWifiStrength()
            {
                return GetDeviceManager().Call<int>("getWifiStrength");
            }

            /// <summary>
            /// register a callback to listen wifi/net info/state
            /// </summary>
            /// <param name="wifiHandler"></param>
            /// <param name="networkStateHandler"></param>
            /// <param name="rssiChangeHandler"></param>
            public static void RegisterWifiReceiver(OnWifiStateChangeHandler wifiHandler, OnNetworkStateChangeHandler networkStateHandler,
                OnRssiChangeHandler rssiChangeHandler)
            {
                GetDeviceManager().Call("registerWifiReceiver", new WifiCallback(wifiHandler, networkStateHandler, rssiChangeHandler));
            }

            /// <summary>
            /// Warning⚠:this method will remove all listeners registered by "RegisterWifiReceiver"
            /// </summary>
            public static void UnRegisterConnectionChangeReceiver()
            {
                GetDeviceManager().Call("unRegisterConnectionChangeReceiver");
            }

            public static bool SetWifiEnabled(bool enable)
            {
                return GetDeviceManager().Call<bool>("setWifiEnabled", enable);
            }

            /// <summary>
            /// return current wifi state
            /// </summary>
            /// <returns ref line 12-16></returns>
            public static int GetWifiState()
            {
                return GetDeviceManager().Call<int>("getWifiState");
            }

            /// <summary>
            /// wifi state is enable, may not connected
            /// </summary>
            /// <returns></returns>
            public static bool IsWifiEnable()
            {
                return GetDeviceManager().Call<bool>("isWifiEnabled");
            }

            /// <summary>
            /// Return a list of all the networks configured for the current foreground user.
            /// the following fields are filled in:
            ///     networkId
            ///     SSID
            ///     BSSID
            ///     priority
            ///     allowedProtocols
            ///     allowedKeyManagement
            ///     allowedAuthAlgorithms
            ///     allowedPairwiseCiphers
            ///     allowedGroupCiphers
            /// 
            /// </summary>
            /// <returns></returns>
            public static string GetConfiguredWifi()
            {
                return GetDeviceManager().Call<string>("getConfiguredWifi");
            }

            /// <summary>
            /// Return ssid  about the current Wi-Fi connection, if any is active
            /// </summary>
            public static string GetConnectedWifiSSID()
            {
                return GetDeviceManager().Call<string>("getConnectedWifiSSID");
            }

            /// <summary>
            /// Request a scan for access points. Returns immediately. The availability
            /// of the results is made known later by means of an asynchronous event sent
            /// on completion of the scan.
            /// </summary>
            /// <returns if the operation succeeded, i.e., the scan was initiated></returns>
            public static bool StartScanWifi()
            {
                return GetDeviceManager().Call<bool>("startScan");
            }

            /// <summary>
            /// Return the results of the latest access point scan, message contains:
            /// SSID - wifi name
            /// BSSID - wifi mac address
            /// capabilities - Authentication, key management, encryption scheme supported by the access point
            /// level - RSSI
            /// timestamp - timestamp
            /// </summary>
            /// <returns the list of access points found in the most recent scan. An app must hold></returns>
            public static string GetScanResult()
            {
                return GetDeviceManager().Call<string>("getScanResults");
            }

            /// <summary>
            /// get mac address if wifi is connected
            /// </summary>
            /// <returns></returns>
            public static string GetMacAddress()
            {
                return GetDeviceManager().Call<string>("getMacAddress");
            }

            /// <summary>
            /// connect Wifi
            /// </summary>
            /// <param name="ssid"></param>
            /// <param name="password"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            public static bool ConnectWifi(string ssid, string password, int wificipherType)
            {
                return GetDeviceManager().Call<bool>("connectWifi", ssid, password, wificipherType);
            }

            /// <summary>
            /// connect static ip wifi
            /// </summary>
            /// <param name="ssid"></param>
            /// <param name="password"></param>
            /// <param name="type"></param>
            /// <param name="prefixLength"></param>
            /// <param name="ipAddress"></param>
            /// <param name="gateway"></param>
            /// <param name="dns"></param>
            /// <returns></returns>
            public static bool ConnectStaticIpWifi(string ssid, string password, int wificipherType,
                                       int prefixLength, string ipAddress, string gateway, string dns)
            {
                return GetDeviceManager().Call<bool>("connectStaticIpWifi", ssid, password, wificipherType, prefixLength, ipAddress, gateway, dns);
            }

            /// <summary>
            /// disconnect current connection
            /// </summary>
            /// <returns></returns>
            public static bool DisconnectWifi()
            {
                return GetDeviceManager().Call<bool>("disconnectWifi");
            }

            /// <summary>
            /// remove network from configuration list
            /// </summary>
            /// <param name="ssid"></param>
            public static void RemoveWifiBySsid(string ssid)
            {
                GetDeviceManager().Call("removeWifiBySsid", ssid);
            }


            private class WifiCallback : AndroidJavaProxy
            {
                private OnWifiStateChangeHandler wifiStateChangeHandler;
                private OnNetworkStateChangeHandler netStateChangeHandler;
                private OnRssiChangeHandler rssiChangeHandler;
                public WifiCallback(OnWifiStateChangeHandler handler,
                    OnNetworkStateChangeHandler netStateHandler,
                    OnRssiChangeHandler rssiChangeHandler) : base("com.qwr.DeviceManager.PluginCallback$WifiCallback")
                {
                    if (handler != null)
                        wifiStateChangeHandler = handler;
                    if (netStateHandler != null)
                        this.netStateChangeHandler = netStateHandler;
                    if (rssiChangeHandler != null)
                        this.rssiChangeHandler = rssiChangeHandler;
                }

                public void onWifiStateChange(int state)
                {
                    if (wifiStateChangeHandler != null)
                        wifiStateChangeHandler(state);
                }

                public void onNetworkStateChange(string ssid, string bssid, int state)
                {
                    if (netStateChangeHandler != null)
                        netStateChangeHandler(ssid,bssid,state);
                }

                public void onRssi(string ssid, int rssi)
                {
                    if (rssiChangeHandler != null)
                        rssiChangeHandler(ssid, rssi);
                }
            }
        }
    }
}