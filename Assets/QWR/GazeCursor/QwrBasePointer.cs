/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace qwr
{
    public class QwrBasePointer : MonoBehaviour
	{
        virtual public void OnEnterObject(GameObject targetObject) { }

        virtual public void OnHoverObject(GameObject targetObject) { }

        virtual public void OnExitObject(GameObject targetObject) { }

        virtual public void OnClickDown() { }

        virtual public void OnClickUp() { }

        public Vector2 GetScreenPosition()
		{
			return QwrCameraRig.WorldToScreenPoint(transform.position);
		}

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        virtual public void SetVisible(bool value)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer)
                renderer.enabled = value;
        }
	}
}
