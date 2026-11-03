using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.VR;
public class RotateScript : MonoBehaviour
{
    void Start()
    {
        
    }
   
    Vector2 TouchAxis;
    float speed = 1f;
    void Update()
    {

#if UNITY_EDITOR
        TouchAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#else
        //TouchAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
#endif
       

        gameObject.transform.Rotate(new Vector3(0, speed * TouchAxis.x, 0));
       
    }
}
