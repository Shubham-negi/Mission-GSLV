using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        [System.Obsolete("NetworkManager is fully deprecated. Use WifiManager instead")]
        public class NetworkManager
        {
            private const string errorMsg = "network manager is fully deprecated, use WifiManager instead";

            public static void OpenAndroidWifiSetting()
            {
                WifiManager.OpenWifiSettings();
            }

            [System.Obsolete("NetworkManager is fully deprecated. Use WifiManager instead, this callback will not work")]
            public static void RegisterNetworkCallback(OnNetReachableHandler reachableHandler,
                                                       OnWifiExtrasHandler extrasHandler,
                                                       int levelCount = 5)
            {
                Debug.LogError(errorMsg);
            }

            public static bool IsNetworkAvailable()
            {
                return WifiManager.IsNetworkAvailable();
            }

            public static int GetWifiStrength()
            {
                return WifiManager.GetWifiStrength();
            }

            public static string GetMacAddress()
            {
                return WifiManager.GetMacAddress();
            }

            [System.Obsolete("NetworkManager is fully deprecated. Use WifiManager instead")]
            public delegate void OnNetReachableHandler(bool reachable);
            [System.Obsolete("NetworkManager is fully deprecated. Use WifiManager instead")]
            public delegate void OnWifiExtrasHandler(string ssid, int signalLevel, int speed, string units);
        }
    }
}
