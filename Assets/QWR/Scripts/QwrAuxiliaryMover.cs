/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;

namespace qwr
{
    public class QwrAuxiliaryMover : MonoBehaviour
    {
#if UNITY_EDITOR
        Vector3 _prevMousePos;
        Transform _transformCenterEye = null;

        void Start()
        {
            // as script component of QwrCameraRig
            _transformCenterEye = gameObject.transform.Find("TrackingSpace/CenterEyeAnchor");
        }

        private void Update()
        {
            if (transform == null)
                return;

            if (_transformCenterEye == null)
                return;

            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    if (_prevMousePos != Input.mousePosition)
                    {
                        _prevMousePos = Input.mousePosition;
                    }
                }

                if (Input.GetMouseButton(1))
                {
                    Vector3 offsetMousePos = Input.mousePosition - _prevMousePos;

                    if(offsetMousePos != Vector3.zero)
                    {
                        transform.eulerAngles = transform.eulerAngles + new Vector3(-offsetMousePos.y * 0.1f, offsetMousePos.x * 0.1f, 0);
                    }

                    _prevMousePos = Input.mousePosition;
                }
            }
        }
#endif
    }
}