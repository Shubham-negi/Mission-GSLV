/************************************************************************************

Copyright   :   Copyright 2015 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System.Collections;
#if UNITY_5_3_OR_NEWER || UNITY_5_3
using UnityEngine.SceneManagement;
#endif

namespace qwr
{
    public class QuitGame : MonoBehaviour
    {

        public KeyCode quit = KeyCode.Escape;

        public KeyCode loadNextScene = KeyCode.N;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(quit))
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                QuitAndroidActivity();
#endif
                Application.Quit();
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        void QuitAndroidActivity()
        {
            var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.CallStatic("Exit");
        }
#endif
    }
}
