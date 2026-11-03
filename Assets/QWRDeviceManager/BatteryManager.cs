using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class BatteryManager
        {
            public const int batteryStateUnknown = 1;
            public const int batteryStateCharging = 2;
            public const int batteryStateDischarging = 3;
            public const int batteryStateNotCharging = 4;
            public const int batteryStateFull = 5;

            private static BatteryCallback batteryCallback;

            public static int GetHMDBatteryLevel()
            {
                return GetDeviceManager().Call<int>("getBatteryLevel");
            }

            public static void RegisterBatteryCallback(OnBatteryLevelHandler batteryHandler)
            {
                batteryCallback = new BatteryCallback(batteryHandler);
                GetDeviceManager().Call("registerBatteryReceiver", batteryCallback);
            }

            public delegate void OnBatteryLevelHandler(int level, int batteryState);
            class BatteryCallback : AndroidJavaProxy
            {
                OnBatteryLevelHandler batteryLevelHandler;
                public BatteryCallback(OnBatteryLevelHandler handler) : base("com.qwr.DeviceManager.PluginCallback$BatteryCallback")
                {
                    this.batteryLevelHandler = handler;
                }

                public void OnBatteryLevel(int level, int batteryState)
                {
                    if (batteryLevelHandler != null)
                        batteryLevelHandler(level, batteryState);
                }
            }
        }
    }
}
