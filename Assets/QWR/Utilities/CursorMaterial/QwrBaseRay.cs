/************************************************************************************

Copyright   :   Copyright 2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/
using UnityEngine;
using System.Collections;

namespace qwr
{
    public class QwrBaseRay : MonoBehaviour
    {
        Vector3 orig = Vector3.zero;
        Vector3 direction = Vector3.forward;

        virtual public void Set(Vector3 start, Vector3 end)
        {
            orig = start;
            direction = (end - start).normalized;

            transform.position = orig;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        IEnumerator _SetVisible()
        {
            yield return new WaitForEndOfFrame();
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = true;
        }

        virtual public void SetVisible(bool value)
        {
            if (value)
            {
                StartCoroutine(_SetVisible());
            }
            else
            {
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                    renderer.enabled = value;
            }
        }
    }
}
