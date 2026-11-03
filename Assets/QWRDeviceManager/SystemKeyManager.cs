using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class SystemKeyManager
        {
            private const int blockPowerKey = 0x1;
            private const int blockBackKey = 0x2;
            private const int blockHomeKey = 0x4;
            private const int blockScreenOff = 0x10;
            private const int blockVolumeKey = 0x80;

            public static int blockFunctions = 0;

            public static void BlockPowerKey()
            {
                if (IsOld())
                {
                    blockFunctions |= blockPowerKey;
                    GetDeviceManager().Call("blockFunctions", blockFunctions,null);
                }
                else
                {
                    GetDeviceManager().Call("blockFunctions", blockPowerKey,"block");
                }
            }

            public static void AllowPowerKey()
            {
                if (IsOld())
                {
                    blockFunctions &= ~blockPowerKey;
                    GetDeviceManager().Call("blockFunctions", blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockFunctions", blockPowerKey, "allow");
                }
            }

            public static void BlockBackKey()
            {
                if (IsOld())
                {
                    blockFunctions |= blockBackKey;
                    GetDeviceManager().Call("blockFunctions", blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockFunctions", blockBackKey, "block");
                }
            }

            public static void AllowBackKey()
            {
                if (IsOld())
                {
                    blockFunctions &= ~blockBackKey;
                    GetDeviceManager().Call("blockFunctions", blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockFunctions", blockBackKey, "allow");
                }
            }

            public static void BlockHomeKey()
            {
                if (IsOld())
                {
                    blockFunctions |= blockHomeKey;
                    GetDeviceManager().Call("blockFunctions", blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockFunctions", blockHomeKey,"block");
                }

                ControllerKeyManager.BlockControllerHomeKey();
            }

            public static void AllowHomeKey()
            {
                if (IsOld())
                {
                    blockFunctions &= ~blockHomeKey;
                    GetDeviceManager().Call("blockFunctions", blockFunctions,null);
                }
                else
                {
                    GetDeviceManager().Call("blockFunctions", blockHomeKey, "allow");
                }

                ControllerKeyManager.AllowControllerHomeKey();
            }

            public static void BlockScreenOff()
            {
                if (IsOld())
                {
                    blockFunctions |= (blockScreenOff | blockPowerKey);
                    GetDeviceManager().Call("blockFunctions", blockFunctions,null);
                }
                else
                {
                    GetDeviceManager().Call("blockFunctions", (blockScreenOff | blockPowerKey),"block");
                }
            }

            public static void SetScreenOn()
            {
                Debug.Log("Screen:::SetScreenOn in manager");
                GetDeviceManager().Call("setScreenOn");
            }

            public static void SetScreenOff()
            {
                Debug.Log("Screen:::SetScreenOff in manager");
                GetDeviceManager().Call("setScreenOff");
            }

            public static void AllowScreenOff()
            {
                if (IsOld())
                {
                    blockFunctions &= ~(blockScreenOff | blockPowerKey);
                    GetDeviceManager().Call("blockFunctions", blockFunctions, null);
                }
                else
                {
                    GetDeviceManager().Call("blockFunctions", (blockScreenOff | blockPowerKey), "allow");
                }
            }

            public static void BlockVolumeKey()
            {
                GetDeviceManager().Call("blockFunctions", blockVolumeKey, "block");
            }

            public static void AllowVolumeKey()
            {
                GetDeviceManager().Call("blockFunctions", blockVolumeKey, "allow");
            }

            /// <summary>
            /// reset all function state, if set true, means to allow all function key
            /// else if set false, means block all function
            /// </summary>
            /// <param name="clearOrSet"></param>
            public static void ClearOrSetAllBlockState(bool clearOrSet)
            {
                if (!IsOld())
                {
                    GetDeviceManager().Call("clearOrSetAllBlockState", clearOrSet);
                }
            }

            private static bool IsOld()
            {
                return GetDeviceManager().Call<bool>("isDeviceManagerOnOldImage");
            }
        }
    }
}
