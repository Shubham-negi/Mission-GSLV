using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QWR;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class AppManager
        {
            public static int succeeded = 1;
            public static int installFailedAlreadyExists = -1;
            public static int installFailedInvalidApk = -2;
            public static int installFailedOlderSdk = -12;
            public static int installFailedCpuAbiIncompatible = -16;
            public static int installParseFailedInconsistentCertificates = -104;
            public static int uninstallFailedInternalError = -1;
            public static int uninstallFailedDevicePolicyManager = -2;
            public static int uninstallFailedUnknownPkg = -10;

            private static InstallApkCallback installApkCallback;
            private static UninstallApkCallback uninstallApkCallback;

            public static void GoToApp(string packageName)
            {
                bool success = GetDeviceManager().Call<bool>("goToApp", packageName);
                Debug.Log("DEBUG goto app : " + success);
            }

            public static void CloseApp(string packageName)
            {
                GetDeviceManager().Call("closeApp", packageName);
            }

            public static bool StartAppWithExtra(string packageName, string className, string[] key, string[] value)
            {
                return GetDeviceManager().Call<bool>("startAppWithExtra", packageName, className, key, value);
            }

            public static bool StartAppWithAction(string action)
            {
                return GetDeviceManager().Call<bool>("startAppWithAction", action);
            }

            public static bool StartAppWithAction(string packageName, string action, string url)
            {
                return GetDeviceManager().Call<bool>("startAppWithAction", packageName, action, url);
            }

            public static void InstallApk(string apkPath,
                                      OnPackageInstallSuccessHandler successCallback = null,
                                      OnPackageInstallErrorHandler errorCallback = null)
            {
                installApkCallback = new InstallApkCallback(successCallback, errorCallback);
                GetDeviceManager().Call("installApk", apkPath, installApkCallback);
            }

            public delegate void OnPackageInstallSuccessHandler(string apkPath, string packageName);
            public delegate void OnPackageInstallErrorHandler(string apkPath, int errorCode);
            class InstallApkCallback : AndroidJavaProxy
            {
                OnPackageInstallSuccessHandler successHandler;
                OnPackageInstallErrorHandler errorHandler;
                public InstallApkCallback(OnPackageInstallSuccessHandler successCallback,
                                          OnPackageInstallErrorHandler errorCallback)
                                        : base("com.qwr.DeviceManager.PluginCallback$PackageInstallCallback")
                {
                    successHandler = successCallback;
                    errorHandler = errorCallback;
                }

                public void OnSuccess(string apkPath, string packageName)
                {
                    if (successHandler != null)
                        successHandler(apkPath, packageName);
                }

                public void OnError(string apkPath, int errorCode)
                {
                    if (errorHandler != null)
                        errorHandler(apkPath, errorCode);
                }
            }

            public static void UninstallApk(string packageName,
                                        OnPackageUninstallSuccessHandler successCallback = null,
                                        OnPackageUninstallErrorHandler errorCallback = null)
            {
                uninstallApkCallback = new UninstallApkCallback(successCallback, errorCallback);
                GetDeviceManager().Call("uninstallApk", packageName, uninstallApkCallback);
            }

            public delegate void OnPackageUninstallSuccessHandler(string packageName);
            public delegate void OnPackageUninstallErrorHandler(string packageName, int errorCode);
            class UninstallApkCallback : AndroidJavaProxy
            {
                OnPackageUninstallSuccessHandler successHandler;
                OnPackageUninstallErrorHandler errorHandler;
                public UninstallApkCallback(OnPackageUninstallSuccessHandler successCallback,
                                            OnPackageUninstallErrorHandler errorCallback)
                                            : base("com.qwr.DeviceManager.PluginCallback$PackageUninstallCallback")
                {
                    successHandler = successCallback;
                    errorHandler = errorCallback;
                }

                public void OnSuccess(string packageName)
                {
                    Debug.Log(packageName + " packageUninstall!");
                    if (successHandler != null)
                        successHandler(packageName);
                }

                public void OnError(string packageName, int errorCode)
                {
                    if (errorHandler != null)
                        errorHandler(packageName, errorCode);
                }
            }

            public static string[] GetInstalledAppList(bool onlyLauncher = true)
            {
                return GetDeviceManager().Call<string[]>("getInstalledPkgNameList", onlyLauncher);
            }

            public static string GetAppNameByPkgname(string pkgname)
            {
                return GetDeviceManager().Call<string>("getAppNameByPkgname", pkgname);
            }

            public static byte[] GetAppIconByPkgname(string pkgname)
            {
                return GetDeviceManager().Call<byte[]>("getApkIconByPkgname", pkgname);
            }

            public static void ShowControllerBindPage()
            {
                GetDeviceManager().Call("showControllerBindPage");
            }

            public static void SendOpenApkBroadCast(string packageName)
            {
                GetDeviceManager().Call("SendOpenApkBroadCast", packageName);
            }
        }
    }
}