using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR.Rwizen.Demo
{
    public class QWRCameraFOV : MonoBehaviour
    {
        void Reset()
        {
#if UNITY_EDITOR
            var cams = transform.GetComponentsInChildren<Camera>();
            foreach (var cam in cams)
            {
                cam.fieldOfView = 60;
            }
#endif
        }
        // Start is called before the first frame update
        void Start()
        {
#if UNITY_EDITOR
            var cams = transform.GetComponentsInChildren<Camera>();
            foreach (var cam in cams)
            {
                cam.fieldOfView = 60;
            }
#endif
        }


    }
}
