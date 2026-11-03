using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class QWRSettingsManager
        {
            public static void OpenApplicationDetail(string pkgname)
            {
                GetDeviceManager().Call("openApplicationDetail", pkgname);
            }

            public static void OpenDateSettings()
            {
                GetDeviceManager().Call("openDateSettings");
            }

            public static void OpenDeviceInfoSettings()
            {
                GetDeviceManager().Call("openDeviceInfoSettings");
            }

            public static void OpenLocaleSettings()
            {
                GetDeviceManager().Call("openLocaleSettings");
            }

            public static void OpenSoundSettings()
            {
                GetDeviceManager().Call("openSoundSettings");
            }

            public static void OpenVPNSettings()
            {
                GetDeviceManager().Call("openVPNSettings");
            }

            /* *
             * display setting include brightness、night light、adaptive brightness、
             * wallpaper、advanced settings, etc.
             * */
            public static void OpenDisplaySettings()
            {
                GetDeviceManager().Call("openDisplaySettings");
            }
        }
    }
}
