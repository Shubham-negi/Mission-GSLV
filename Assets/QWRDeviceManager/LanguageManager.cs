using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class LanguageManager
        {
            public static void SetLanguage(string language, string country = "")
            {
                GetDeviceManager().Call("setLanguage", language, country);
            }

            public static string GetCurrentSystemLanguage()
            {
                return GetDeviceManager().Call<string>("getCurrentLanguage");
            }

            public static string GetCurrentSystemCountry()
            {
                return GetDeviceManager().Call<string>("getCurrentCountry");
            }

            public static string[] GetSupportLanguageList()
            {
                return GetDeviceManager().Call<string[]>("getSupportLanguageList");
            }

            public static string[] GetCountriesByLanguage(string language = "zh")
            {
                return GetDeviceManager().Call<string[]>("getCountriesByLanguage", language);
            }
        }
    }
}
