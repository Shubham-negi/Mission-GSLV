using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class FotaUpdateManager
        {
            private static FotaUpdateCallback fotaUpdateCallback;
            public static bool QueryFotaUpdate()
            {
                return GetDeviceManager().Call<bool>("queryFotaUpdate");
            }

            public static bool RegisterFotaUpdate(OnFotaUpdateHandler handler)
            {
                fotaUpdateCallback = new FotaUpdateCallback(handler);
                return GetDeviceManager().Call<bool>("registerUpdateReceiver", fotaUpdateCallback);
            }

            public delegate void OnFotaUpdateHandler(bool newVersion);
            class FotaUpdateCallback : AndroidJavaProxy
            {
                OnFotaUpdateHandler updateHandler;
                public FotaUpdateCallback(OnFotaUpdateHandler handler) : base("com.qwr.DeviceManager.PluginCallback$FotaUpdateCallback")
                {
                    updateHandler = handler;
                }

                public void OnQueryUpdate(bool newVersion)
                {
                    if (updateHandler != null)
                    {
                        updateHandler(newVersion);
                    }
                }
            }
        }
    }
}
