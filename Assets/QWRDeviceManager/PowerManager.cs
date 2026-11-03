using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class PowerManager
        {
            public const int backgroundWakeLock = 0x01;
            public const int forgroundWakeLock = 0X02;
            public static void AndroidShutDown()
            {
                GetDeviceManager().Call("androidShutDown");
            }

            public static void AndroidReboot()
            {
                GetDeviceManager().Call("androidReboot");
            }

            public static void AcquireWakeLock(int wakeLockLevel = forgroundWakeLock)
            {
                if (wakeLockLevel == backgroundWakeLock)
                {
                    GetDeviceManager().Call("acquireWakeLock");
                }
                else if (wakeLockLevel == forgroundWakeLock)
                {
                    SystemSleepManager.BlockSystemSleep();
                }
                else
                {
                    Debug.Log(tag + ": unknown wakelock level!");
                }
            }

            public static void ReleaseWakeLock(int wakeLockLevel = forgroundWakeLock)
            {
                if (wakeLockLevel == backgroundWakeLock)
                {
                    GetDeviceManager().Call("releaseWakeLock");
                }
                else if (wakeLockLevel == forgroundWakeLock)
                {
                    SystemSleepManager.AllowSystemSleep();
                }
                else
                {
                    Debug.Log(tag + ": unknown wakelock level!");
                }
            }
        }
    }
}
