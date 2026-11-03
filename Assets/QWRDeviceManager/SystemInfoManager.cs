using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class SystemInfoManager
        {

            private static readonly string Boe4KProductModel = "P1Pro-BOE4K";
            private static readonly string BoeDsc4KProductModel = "P1Pro-BOEDSC4K";

            public static string GetProductModel()
            {
                return GetDeviceManager().Call<string>("getProductModel");
            }

            public static string GetLinuxVersion()
            {
                return GetDeviceManager().Call<string>("getLinuxVersion");
            }

            public static string GetAndroidVersion()
            {
                return GetDeviceManager().Call<string>("getAndroidVersion");
            }

            public static string GetBuildNumber()
            {
                return GetDeviceManager().Call<string>("getBuildNumber");
            }

            public static string GetSerialNo()
            {
                return GetDeviceManager().Call<string>("getSerialNo");
            }

            public static void OpenSystemSettings()
            {
                AppManager.GoToApp("com.android.settings");
            }

            public static string GetCPUModel(){
                return GetDeviceManager().Call<string>("getCPUModel");
            }

            public static string GetResolution(){
                return GetDeviceManager().Call<string>("getResolution");
            }

            public static string GetBrand(){
                return GetDeviceManager().Call<string>("getBrand");
            }

            public static bool BoeDevice()
            {
                string productModel = GetProductModel();

                if (productModel == Boe4KProductModel || productModel == BoeDsc4KProductModel)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
