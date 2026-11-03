using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace qwr
{

    public class ControllerFlip : MonoBehaviour
    {
        Button btn;
        Text txt;
        Text TouchPosition;
        Text Gesture;

        // Use this for initialization
        void Start()
        {
            btn = GameObject.Find("Button").GetComponent<Button>(); 
            txt = btn.transform.Find("Text").GetComponent<Text>();
            TouchPosition= GameObject.Find("Touch Position").GetComponent<Text>();
            Gesture = GameObject.Find("Gesture").GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            TouchPosition.text = QwrDaydreamController.TouchPos.ToString();
            if (QwrDaydreamController.ClickButtonDown)
            {
                txt.text = "ClickButtonDown";
            }
            if (QwrDaydreamController.ClickButtonUp)
            {
                txt.text = "ClickButtonUp";
            }
            
            if (QwrDaydreamController.TriggerButtonDown)
            {
                txt.text = "TriggerButtonDown";
            }
            if (QwrDaydreamController.TriggerButtonUp)
            {
                txt.text = "TriggerButtonUp";
            }
            if (QwrDaydreamController.TouchDown)
            {
                txt.text = "TouchDown";
            }
            if (QwrDaydreamController.TouchUp)
            {
                txt.text = "TouchUp";
            }
            if (QwrDaydreamController.TouchGestureDown)

            {
                Gesture.text = "TouchGestureDown";
            }
            if (QwrDaydreamController.TouchGestureUp)

            {
                Gesture.text = "TouchGestureUp";
            }
            if (QwrDaydreamController.TouchGestureLeft)

            {
                Gesture.text = "TouchGestureLeft";
            }
            if (QwrDaydreamController.TouchGestureRight)

            {
                Gesture.text = "TouchGestureRight";
            }





        }
    }
}
