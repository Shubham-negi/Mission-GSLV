using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class ControllerKeyManager
        {
            private const int blockControllerHomeKey = 0x1;
            private const int blockControllerBackKey = 0x2;
            private const int blockControllerDpadKey = 0x4;
            private const int blockControllerTriggerKey = 0x8;

            private const int enableDoubleClickBackKey = 0x20;
            private const int enableDoubleClickHomeKey = 0x40;
            private static DeviceKeyEvent keyEvent;

            private static bool IsOld() {
                return GetDeviceManager().Call<bool>("isDeviceManagerOnOldImage");
            }

            public static void BlockControllerHomeKey()
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions |= blockControllerHomeKey;
                    GetDeviceManager().Call("blockController", SystemKeyManager.blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockController", blockControllerHomeKey, "block");
                }
            }

            public static void AllowControllerHomeKey()
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions &= ~blockControllerHomeKey;
                    GetDeviceManager().Call("blockController", SystemKeyManager.blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockController", blockControllerHomeKey, "allow");
                }
            }

            public static void BlockControllerBackKey()
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions |= blockControllerBackKey;
                    GetDeviceManager().Call("blockController", SystemKeyManager.blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockController", blockControllerBackKey, "block");
                }
            }

            public static void AllowControllerBackKey()
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions &= ~blockControllerBackKey;
                    GetDeviceManager().Call("blockController", SystemKeyManager.blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockController", blockControllerBackKey, "allow");
                }
            }

            public static void BlockControllerDPADKey()
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions |= blockControllerDpadKey;
                    GetDeviceManager().Call("blockController", SystemKeyManager.blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockController", blockControllerDpadKey, "block");
                }
            }

            public static void AllowControllerDPADKey()
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions &= ~blockControllerDpadKey;
                    GetDeviceManager().Call("blockController", SystemKeyManager.blockFunctions,null);
                }
                else
                {
                    GetDeviceManager().Call("blockController", blockControllerDpadKey, "allow");
                }
            }

            public static void BlockControllerTriggerKey()
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions |= blockControllerTriggerKey;
                    GetDeviceManager().Call("blockController", SystemKeyManager.blockFunctions,null);
                }
                else
                {
                    GetDeviceManager().Call("blockController", blockControllerTriggerKey, "block");
                }
            }

            public static void AllowControllerTriggerKey()
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions &= ~blockControllerTriggerKey;
                    GetDeviceManager().Call("blockController", SystemKeyManager.blockFunctions,null);
                }
                else
                {
                    GetDeviceManager().Call("blockController", blockControllerTriggerKey, "allow");
                }
            }

            /// <summary>
            /// double click trigger intent to dest pkgname/class
            ///     exp: pkg = com.qwr.DeviceManager   class = MainActivity
            ///     then you should pass 'com.qwr.DeviceManager' as package name
            ///     and 'com.qwr.DeviceManager.MainAcitivity' as class name,
            /// </summary>
            /// <param name="pkgname">package name</param>
            /// <param name="cls">full name of class, contains package name</param>
            public static void EnableDoubleClickBackKey(int behavior, string pkgname, string cls="", int delay=3000)
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions |= enableDoubleClickBackKey;
                    GetDeviceManager().Call("enableDoubleClickBackKey", behavior, SystemKeyManager.blockFunctions, pkgname, cls, delay,null);
                }
                else
                {
                    GetDeviceManager().Call("enableDoubleClickBackKey", behavior, enableDoubleClickBackKey, pkgname, cls, delay, "block");
                }
            }

            public static void DisableDoubleClickBackKey(int behavior)
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions &= ~enableDoubleClickBackKey;
                    GetDeviceManager().Call("enableDoubleClickBackKey", behavior, SystemKeyManager.blockFunctions, "", "", 3000,null);
                }
                else
                {
                    GetDeviceManager().Call("enableDoubleClickBackKey", behavior, enableDoubleClickBackKey, "", "", 3000, "allow");
                }
            }

            /// <summary>
            /// double click trigger intent to dest pkgname/class
            ///     exp: pkg = com.qwr.DeviceManager   class = MainActivity
            ///     then you should pass 'com.qwr.DeviceManager' as package name
            ///     and 'com.qwr.DeviceManager.MainAcitivity' as class name,
            /// </summary>
            /// <param name="pkgname">package name</param>
            /// <param name="cls">full name of class, contains package name</param>
            public static void EnableDoubleClickHomeKey(int behavior, string pkgname, string cls="", int delay=3000)
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions |= enableDoubleClickHomeKey;
                    GetDeviceManager().Call("enableDoubleClickHomeKey", behavior, SystemKeyManager.blockFunctions, pkgname, cls, delay,null);
                }
                else
                {
                    GetDeviceManager().Call("enableDoubleClickHomeKey", behavior, enableDoubleClickHomeKey, pkgname, cls, delay, "block");
                }
            }

            public static void DisableDoubleClickHomeKey(int behavior)
            {
                if (IsOld())
                {
                    SystemKeyManager.blockFunctions &= ~enableDoubleClickHomeKey;
                    GetDeviceManager().Call("enableDoubleClickHomeKey", behavior, SystemKeyManager.blockFunctions, "", "", 3000,null);
                }
                else
                {
                    GetDeviceManager().Call("enableDoubleClickHomeKey", behavior, enableDoubleClickHomeKey, "", "", 3000,"allow");
                }
            }

            /// <summary>
            /// reset all function state, if set true, means to allow all function key
            /// else if set false, means block all function
            /// </summary>
            /// <param name="clearOrSet"></param>
            public static void ClearOrSetAllControllerState(bool clear)
            {
                if (!IsOld())
                {
                    GetDeviceManager().Call("clearOrSetAllControllerState", clear);
                }
            }

            public static void RegisterDoubleClickHomeKeyCallback(OnDoubleClickHomeKeyCallback callback)
            {
                if (keyEvent == null)
                    keyEvent = new DeviceKeyEvent();
                keyEvent.AddDoubleClickHomeListener(callback);
                EnableDoubleClickHomeKey(1, "", "");
                GetDeviceManager().Call("addKeyCodeEventCallback", keyEvent);
            }

            public static void UnregisterDoubleClickHomeKeyCallback()
            {
                GetDeviceManager().Call("unregisterKeyCodeEventCallback");
            }

            public delegate void OnDoubleClickHomeKeyCallback();
            class DeviceKeyEvent : AndroidJavaProxy
            {
                List<OnDoubleClickHomeKeyCallback> doubleClickHomeKeyCallbacks = new List<OnDoubleClickHomeKeyCallback>();
                public DeviceKeyEvent() 
                    : base("com.qwr.DeviceManager.PluginCallback$KeyCodeEvent")
                {
                }

                public void AddDoubleClickHomeListener(OnDoubleClickHomeKeyCallback callback)
                {
                    if (callback != null)
                        this.doubleClickHomeKeyCallbacks.Add(callback);
                }

                public void OnDoubleClickHomeKey()
                {
                    if (doubleClickHomeKeyCallbacks.Count > 0)
                        doubleClickHomeKeyCallbacks.ForEach(item => item());
                }
            }
        }
    }
}
