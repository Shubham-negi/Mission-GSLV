using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QwrEditorController : MonoBehaviour
{

    private float mouseX = 0;
    private float mouseY = 0;
    private float mouseZ = 0;


    public static QwrEditorController instance;

    Vector3 _prevMousePos;
    


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (!Application.isEditor)
        {
            this.enabled = false;

        }
    }

  
        private Quaternion UpdateSimulatedFrameParams()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                mouseX += Input.GetAxis("Mouse X") * 5;
                if (mouseX <= -180)
                {
                    mouseX += 360;
                }
                else if (mouseX > 180)
                {
                    mouseX -= 360;
                }
                mouseY -= Input.GetAxis("Mouse Y") * 2.4f;
                mouseY = Mathf.Clamp(mouseY, -80, 80);
            }
            else if (Input.GetKey(KeyCode.RightShift))
            {
                mouseZ += Input.GetAxis("Mouse X") * 5;
                mouseZ = Mathf.Clamp(mouseZ, -80, 80);
            }

            return Quaternion.Euler(mouseY, mouseX, mouseZ);
        }
    




    private void Update()
    {



#if UNITY_EDITOR

        transform.localRotation = UpdateSimulatedFrameParams();
#endif

    }
}
