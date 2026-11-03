using UnityEngine;
using System.Collections;
using UnityEngine.VR;
using UnityEngine.UI;
using System.Collections.Generic;
using qwr;
using QWR;

namespace QWR.Rwizen.Demo
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        public static MovementController Instance;
        public float rotateSpeed = 2.0F;
        public float rotateAngle = 10;


        public float speed = 4.0F;

        Vector3 forward;
        CharacterController controllor;
        public Transform vrHead;

        Vector2 tp;

        float curSpeed;

        void Start()
        {
            Instance = this;
            controllor = GetComponent<CharacterController>();

        }

        void Update()
        {
            if (Time.timeScale == 0)
                return;

            OculusController();
        }

        Vector2 TouchAxis, TouchRawAxis;
        bool isHorizontalDown;
        void OculusController()
        {

#if UNITY_EDITOR
            TouchAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            isHorizontalDown = isHorizontalDown = Input.GetButtonDown("Horizontal");

#else
       TouchAxis = GetTouchPoint();
        isHorizontalDown = QwrDaydreamController.TouchDown;
#endif



            forward = vrHead.TransformDirection(Camera.main.transform.forward);

            if (TouchAxis.magnitude > 0.3f)
            {
                if (Mathf.Abs(TouchAxis.x) <= Mathf.Abs(TouchAxis.y))
                {
                    forward = Camera.main.transform.forward;
                    curSpeed = speed * TouchAxis.y;
                    controllor.SimpleMove(forward * curSpeed);
                }


            }

            if (isHorizontalDown)
            {
                if (Mathf.Abs(TouchAxis.y) <= Mathf.Abs(TouchAxis.x))
                {
                    transform.Rotate(0, rotateAngle * TouchAxis.x, 0);
                }
            }



            if (TouchAxis.y == 0)
            {
                curSpeed = 0;
            }

        }

        public Vector2 GetTouchPoint()
        {
            if (QwrDaydreamController.IsTouching)
            {
                Vector2 position = QwrDaydreamController.TouchPos;
                switch (QwrManager.touchPosOrig)
                {
                    case TouchPosOrig.CENTER:
                        break;
                    case TouchPosOrig.TOP_LEFT:
                        position = position * 2.0f - Vector2.one;
                        break;
                }

                return new Vector2(position.x, -position.y);
            }

            return new Vector2(0, 0);
        }

        void NormalController()
        {
            forward = vrHead.TransformDirection(Vector3.forward);
            curSpeed = -speed * Input.GetAxis("Vertical");
            controllor.SimpleMove(forward * curSpeed * Time.deltaTime);

            if (Input.GetButtonDown("Horizontal"))
                transform.Rotate(0, rotateAngle * Input.GetAxisRaw("Horizontal"), 0);
        }
    }
}