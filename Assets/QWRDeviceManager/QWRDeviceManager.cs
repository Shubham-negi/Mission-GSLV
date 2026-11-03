using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        private const string tag = "QWRDeviceManager";
        private const string unityPlayerClass = "com.unity3d.player.UnityPlayer";
        private const string versionString = "0.2.5";

        [System.Obsolete("BACKGROUND_WAKE_LOCK is deprecated. Use PowerManager.BackgroundWakeLock instead")]
        public const int BACKGROUND_WAKE_LOCK = PowerManager.backgroundWakeLock;
        [System.Obsolete("FORGROUND_WAKE_LOCK is deprecated. Use PowerManager.ForgroundWakeLock instead")]
        public const int FORGROUND_WAKE_LOCK = PowerManager.forgroundWakeLock;

        private static AndroidJavaObject deviceManager;

        public static AndroidJavaObject GetActivity()
        {
            AndroidJavaClass jc = new AndroidJavaClass(unityPlayerClass);
            if (jc == null)
            {
                Debug.LogErrorFormat("Failed to get Unity Player class, {0}", unityPlayerClass);
                return null;
            }
            AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null)
            {
                Debug.LogError("Failed to obtain Android Activity from Unity Player class.");
                return null;
            }
            return activity;
        }

        private static AndroidJavaObject GetDeviceManager()
        {
            if (deviceManager == null)
            {
                Debug.Log("DeviceManager version: " + versionString + "minSdk = 19, targetSdk = 19");
                Debug.Log("Create device manager android java object.");
                deviceManager = new AndroidJavaObject("com.qwr.DeviceManager.QWRDeviceManager", GetActivity());
            }

            return deviceManager;
        }


        [System.Obsolete("AndroidShutDown() is deprecated. Use PowerManager.AndroidShutDown() instead.")]
        public static void AndroidShutDown()
        {
            PowerManager.AndroidShutDown();
        }

        [System.Obsolete("AndroidReboot() is deprecated. Use PowerManager.AndroidReboot() instead.")]
        public static void AndroidReboot()
        {
            PowerManager.AndroidReboot();
        }

        [System.Obsolete("AcquireWakeLock() is deprecated. Use PowerManager.AcquireWakeLock() instead.")]
        public static void AcquireWakeLock(int wakeLockLevel = PowerManager.forgroundWakeLock)
        {
            PowerManager.AcquireWakeLock(wakeLockLevel);
        }

        [System.Obsolete("ReleaseWakeLock() is deprecated. Use PowerManager.ReleaseWakeLock() instead.")]
        public static void ReleaseWakeLock(int wakeLockLevel = PowerManager.forgroundWakeLock)
        {
            PowerManager.ReleaseWakeLock(wakeLockLevel);
        }

        [System.Obsolete("GoToApp() is deprecated. Use AppManager.GoToApp() instead.")]
        public static void GoToApp(string packageName)
        {
            AppManager.GoToApp(packageName);
        }

        [System.Obsolete("CloseApp() is deprecated. Use AppManager.CloseApp() instead.")]
        public static void CloseApp(string packageName)
        {
            AppManager.CloseApp(packageName);
        }

        [System.Obsolete("InstallApk() is deprecated. Use AppManager.InstallApk() instead.")]
        public static void InstallApk(string apkPath,
                                  OnPackageInstallSuccessHandler successCallback = null,
                                  OnPackageInstallErrorHandler errorCallback = null)
        {
            AppManager.InstallApk(apkPath, new AppManager.OnPackageInstallSuccessHandler(successCallback),
                new AppManager.OnPackageInstallErrorHandler(errorCallback));
        }

        [System.Obsolete("OnPackageInstallSuccessHandler is deprecated. Use AppManager.OnPackageInstallSuccessHandler instead.")]
        public delegate void OnPackageInstallSuccessHandler(string apkPath, string packageName);
        [System.Obsolete("OnPackageInstallErrorHandler is deprecated. Use AppManager.OnPackageInstallErrorHandler instead.")]
        public delegate void OnPackageInstallErrorHandler(string apkPath, int errorCode);

        [System.Obsolete("UninstallApk() is deprecated. Use AppManager.UninstallApk() instead.")]
        public static void UninstallApk(string packageName,
                                        OnPackageUninstallSuccessHandler successCallback = null,
                                        OnPackageUninstallErrorHandler errorCallback = null)
        {
            AppManager.UninstallApk(packageName, new AppManager.OnPackageUninstallSuccessHandler(successCallback),
                new AppManager.OnPackageUninstallErrorHandler(errorCallback));
        }

        [System.Obsolete("OnPackageUninstallSuccessHandler is deprecated. Use AppManager.OnPackageUninstallSuccessHandler instead.")]
        public delegate void OnPackageUninstallSuccessHandler(string packageName);
        [System.Obsolete("OnPackageUninstallErrorHandler is deprecated. Use AppManager.OnPackageUninstallErrorHandler instead.")]
        public delegate void OnPackageUninstallErrorHandler(string packageName, int errorCode);

        [System.Obsolete("SetHomeKeyApp() is deprecated. Use HomeAppManager.SetHomeKeyApp() instead.")]
        public static void SetHomeKeyApp(string packageName, string activityName)
        {
            HomeAppManager.SetHomeKeyApp(packageName, activityName);
        }

        [System.Obsolete("RestoreHomeKeyApp() is deprecated. Use HomeAppManager.RestoreHomeKeyApp() instead.")]
        public static void RestoreHomeKeyApp()
        {
            HomeAppManager.RestoreHomeKeyApp();
        }

        [System.Obsolete("SetNewLauncher() is deprecated. Use HomeAppManager.SetNewLauncher() instead.")]
        public static void SetNewLauncher(string packageName)
        {
            HomeAppManager.SetNewLauncher(packageName);
        }

        [System.Obsolete("RestoreDpnLauncher() is deprecated. Use HomeAppManager.RestoreDpnLauncher() instead.")]
        public static void RestoreDpnLauncher()
        {
            HomeAppManager.RestoreDpnLauncher();
        }

        [System.Obsolete("BlockPowerKey() is deprecated. Use SystemKeyManager.BlockPowerKey() instead.")]
        public static void BlockPowerKey()
        {
            SystemKeyManager.BlockPowerKey();
        }

        [System.Obsolete("AllowPowerKey() is deprecated. Use SystemKeyManager.AllowPowerKey() instead.")]
        public static void AllowPowerKey()
        {
            SystemKeyManager.AllowPowerKey();
        }

        [System.Obsolete("BlockBackKey() is deprecated. Use SystemKeyManager.BlockBackKey() instead.")]
        public static void BlockBackKey()
        {
            SystemKeyManager.BlockBackKey();
        }

        [System.Obsolete("AllowBackKey() is deprecated. Use SystemKeyManager.AllowBackKey() instead.")]
        public static void AllowBackKey()
        {
            SystemKeyManager.AllowBackKey();
        }

        [System.Obsolete("BlockHomeKey() is deprecated. Use SystemKeyManager.BlockHomeKey() instead.")]
        public static void BlockHomeKey()
        {
            SystemKeyManager.BlockHomeKey();
        }

        [System.Obsolete("AllowHomeKey() is deprecated. Use SystemKeyManager.AllowHomeKey() instead.")]
        public static void AllowHomeKey()
        {
            SystemKeyManager.AllowHomeKey();
        }

        [System.Obsolete("BlockScreenOff() is deprecated. Use SystemKeyManager.BlockScreenOff() instead.")]
        public static void BlockScreenOff()
        {
            SystemKeyManager.BlockScreenOff();
        }

        [System.Obsolete("AllowScreenOff() is deprecated. Use SystemKeyManager.AllowScreenOff() instead.")]
        public static void AllowScreenOff()
        {
            SystemKeyManager.AllowScreenOff();
        }

        [System.Obsolete("BlockControllerHomeKey() is deprecated. Use ControlKeyManager.BlockControllerHomeKey() instead.")]
        public static void BlockControllerHomeKey()
        {
            ControllerKeyManager.BlockControllerHomeKey();
        }

        [System.Obsolete("AllowControllerHomeKey() is deprecated. Use ControlKeyManager.AllowControllerHomeKey() instead.")]
        public static void AllowControllerHomeKey()
        {
            ControllerKeyManager.AllowControllerHomeKey();
        }

        [System.Obsolete("BlockControllerBackKey() is deprecated. Use ControlKeyManager.BlockControllerBackKey() instead.")]
        public static void BlockControllerBackKey()
        {
            ControllerKeyManager.BlockControllerBackKey();
        }

        [System.Obsolete("AllowControllerBackKey() is deprecated. Use ControlKeyManager.AllowControllerBackKey() instead.")]
        public static void AllowControllerBackKey()
        {
            ControllerKeyManager.AllowControllerBackKey();
        }

        [System.Obsolete("BlockControllerDPADKey() is deprecated. Use ControlKeyManager.BlockControllerDPADKey() instead.")]
        public static void BlockControllerDPADKey()
        {
            ControllerKeyManager.BlockControllerDPADKey();
        }

        [System.Obsolete("AllowControllerDPADKey() is deprecated. Use ControlKeyManager.AllowControllerDPADKey() instead.")]
        public static void AllowControllerDPADKey()
        {
            ControllerKeyManager.AllowControllerDPADKey();
        }

        [System.Obsolete("BlockControllerTriggerKey() is deprecated. Use ControlKeyManager.BlockControllerTriggerKey() instead.")]
        public static void BlockControllerTriggerKey()
        {
            ControllerKeyManager.BlockControllerTriggerKey();
        }

        [System.Obsolete("AllowControllerTriggerKey() is deprecated. Use ControlKeyManager.AllowControllerTriggerKey() instead.")]
        public static void AllowControllerTriggerKey()
        {
            ControllerKeyManager.AllowControllerTriggerKey();
        }

        [System.Obsolete("BlockSystemSleep() is deprecated. Use SystemSleepManager.BlockSystemSleep() instead.")]
        public static void BlockSystemSleep()
        {
            SystemSleepManager.BlockSystemSleep();
        }

        [System.Obsolete("AllowSystemSleep() is deprecated. Use SystemSleepManager.AllowSystemSleep() instead.")]
        public static void AllowSystemSleep()
        {
            SystemSleepManager.AllowSystemSleep();
        }

        [System.Obsolete("GetProductModel() is deprecated. Use SystemInfoManager.GetProductModel() instead.")]
        public static string GetProductModel()
        {
            return SystemInfoManager.GetProductModel();
        }

        [System.Obsolete("GetLinuxVersion() is deprecated. Use SystemInfoManager.GetLinuxVersion() instead.")]
        public static string GetLinuxVersion()
        {
            return SystemInfoManager.GetLinuxVersion();
        }

        [System.Obsolete("GetAndroidVersion() is deprecated. Use SystemInfoManager.GetAndroidVersion() instead.")]
        public static string GetAndroidVersion()
        {
            return SystemInfoManager.GetAndroidVersion();
        }

        [System.Obsolete("GetBuildNumber() is deprecated. Use SystemInfoManager.GetBuildNumber() instead.")]
        public static string GetBuildNumber()
        {
            return SystemInfoManager.GetBuildNumber();
        }

        [System.Obsolete("GetSerialNo() is deprecated. Use SystemInfoManager.GetSerialNo() instead.")]
        public static string GetSerialNo()
        {
            return SystemInfoManager.GetSerialNo();
        }

        [System.Obsolete("GetHMDBatteryLevel() is deprecated. Use BatteryManager.GetHMDBatteryLevel() instead.")]
        public static int GetHMDBatteryLevel()
        {
            return BatteryManager.GetHMDBatteryLevel();
        }

        [System.Obsolete("OpenAndroidWifiSetting() is deprecated. Use NetworkManager.OpenAndroidWifiSetting() instead.")]
        public static void OpenAndroidWifiSetting()
        {
            NetworkManager.OpenAndroidWifiSetting();
        }

        [System.Obsolete("OpenAndroidBluetoothSetting() is deprecated. Use BluetoothManager.OpenAndroidBluetoothSetting() instead.")]
        public static void OpenAndroidBluetoothSetting()
        {
            BluetoothManager.OpenAndroidBluetoothSetting();
        }
    }
}
