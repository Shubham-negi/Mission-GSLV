


using System;
using System.Collections.Generic;
using UnityEngine;

namespace qwr
{
    public enum NoloDevice
    {
        LeftController = 0,
        RightController,
    }

#if true
    public class QwrNoloController : MonoBehaviour {}
#else
    public class QwrNoloController : QwrMultiControllerPeripheral
    {
        static QwrNoloController instance = null;

        public QwrNoloController()
        {
            instance = this;
            PeripheralType = QWRPeripheral.Nolo;
        }

        NoloController[] noloControllers = new NoloController[2]; 

        static readonly string[] s_controllerNames = {"controller(left)","controller(right)" };

        public void Awake()
        {
            _controllerNames = s_controllerNames;

            _controllers = new QwrBasePeripheral[2];
        }

        void Start()
        {
            {
                noloControllers[0] = GetNoloController(s_controllerNames[0]);
            }
            {
                noloControllers[1] = GetNoloController(s_controllerNames[1]);
            }
        }

        public override QwrBasePeripheral GetController(string controllerName)
        {
            return GetNoloController(controllerName);    
        }

        NoloController GetNoloController(string controllerName)
        {
            NoloController noloCtrller = null;
            Transform noloTransf = gameObject.transform.FindChild(controllerName);
            if (noloTransf != null)
            {
                noloCtrller = noloTransf.GetComponent<NoloController>();
            }
            return noloCtrller;
        }

        override public void OnControllerDisconnected(QwrBasePeripheral controller)
        {
            base.OnControllerDisconnected(controller);

            if (_connectedControllers.Count == 0 && gameObject.transform && gameObject.transform.parent)
            {
                Transform transform = gameObject.transform.parent.FindChild("QwrBoundary(Clone)");
                if (transform)
                    transform.gameObject.SetActive(false);
            }
        }

        public override void OnControllerConnected(QwrBasePeripheral controller)
        {
            base.OnControllerConnected(controller);

            if (_connectedControllers.Count == 1 && gameObject.transform && gameObject.transform.parent)
            {
                Transform transform = gameObject.transform.parent.FindChild("QwrBoundary(Clone)");
                if (transform)
                    transform.gameObject.SetActive(true);
            }
        }

        static bool ButtonDown(NoloDevice device, NoloButtonMask mask)
        {
            if (instance == null)
                return false;

            NoloController controller = instance.noloControllers[(int)device];
            if (controller == null)
                return false;

            return controller.ButtonDown(mask);
        }

        static bool ButtonUp(NoloDevice device, NoloButtonMask mask)
        {
            if (instance == null)
                return false;

            NoloController controller = instance.noloControllers[(int)device];
            if (controller == null)
                return false;

            return controller.ButtonUp(mask);
        }

        static public bool ClickButtonDown(NoloDevice device)
        {
            return ButtonDown(device, NoloButtonMask.TouchClick);
        }

        static public bool ClickButtonUp(NoloDevice device)
        {
            return ButtonUp(device, NoloButtonMask.TouchClick);
        }

        static public bool TouchDown(NoloDevice device)
        {
            return ButtonDown(device, NoloButtonMask.Touch);
        }

        static public bool TouchUp(NoloDevice device)
        {
            return ButtonUp(device, NoloButtonMask.Touch);
        }

        static public bool MenuButtonDown(NoloDevice device)
        {
            return ButtonDown(device, NoloButtonMask.Menu);
        }

        static public bool MenuButtonUp(NoloDevice device)
        {
            return ButtonUp(device, NoloButtonMask.Menu);
        }

        static public bool SystemButtonDown(NoloDevice device)
        {
            return ButtonDown(device, NoloButtonMask.System);
        }

        static public bool SystemButtonUp(NoloDevice device)
        {
            return ButtonUp(device, NoloButtonMask.System);
        }

        static public bool TriggerButtonDown(NoloDevice device)
        {
            return ButtonDown(device, NoloButtonMask.Trigger);
        }

        static public bool TriggerButtonUp(NoloDevice device)
        {
            return ButtonUp(device, NoloButtonMask.Trigger);
        }

        public override void EnableInternalObjects(bool enabled)
        {
            base.EnableInternalObjects(enabled);

            QwrPointerManager.Instance.ray.SetVisible(true);

        }
    }
#endif
}
