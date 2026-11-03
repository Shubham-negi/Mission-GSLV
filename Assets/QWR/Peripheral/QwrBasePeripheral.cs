/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System.Collections;
using System.Threading;

namespace qwr
{
    /// <summary>
    /// Main entry point for the QwrBasePeripheral API.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class QwrBasePeripheral : MonoBehaviour
    {
        public QwrPeripheral peripheral = null;

        public QwrBasePeripheral()
        {
            PeripheralType = QWRPeripheral.None;
        }

        public QWRPeripheral PeripheralType
        {
            protected set;
            get;
        }

        void Awake()
        {
            if (rayParent == null)
                rayParent = this.transform;

            rayLocalMatrix = Matrix4x4.TRS(rayOrigin, Quaternion.Euler(rayTiltedAngle, 0, 0), Vector3.one);
        }
        /// <summary>
        /// Opens the peripheral.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public virtual bool OpenPeripheral(QWRP_DEVICE_TYPE type, int index)
        {
            if (QwrDevice._instance == null)
            {
                QwrDevice.create();
            }
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (type == QWRP_DEVICE_TYPE.QWRP_DEVICE_TYPE_HEAD_TRACKER)
            {
                peripheral = QwrDevice.OpenPeripheral(string.Empty, this);
                return true;
            }
            #endif

            int count = QwrPeripheral.QwrupQueryDeviceCount(type);
            if (count < 0 || index >= count)
            {
                return false;
            }
            string devicename = QwrPeripheral.QwrupGetDeviceId(type, index);
            if (devicename != null)
            {
                return OpenPeripheral(devicename);
            }
            return false;
        }

        /// <summary>
        /// Opens the peripheral.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        public virtual bool OpenPeripheral(string deviceId)
        {
            if (QwrDevice._instance == null)
            {
                QwrDevice.create();
            }
#if UNITY_ANDROID && UNITY_EDITOR
            if (QwrManager.androidEditorUseHmd)
            {
                QwrPeripheral peripheral = QwrDevice.OpenPeripheral(deviceId, this);
                return peripheral != null;
            }
            else
            {
                peripheral = null;
                return true;
            }
#else
            QwrPeripheral peripheral = QwrDevice.OpenPeripheral(deviceId, this);
            return peripheral != null;
#endif

        }

        /// <summary>
        /// Disables the base peripheral.
        /// </summary>
        public virtual void OnDisable()
        {
            ClosePeripheral();
        }

        /// <summary>
        /// Closes the base peripheral.
        /// </summary>
        public void ClosePeripheral()
        {
            if (peripheral == null)
            {
                return;
            }
            QwrDevice.ClosePeripheral(this);
        }

        /// <summary>
        /// Gets the information of the device.
        /// </summary>
        /// <returns></returns>
        public PeripheralInfo QwrpGetDeviceInfo ()
        {
            if (peripheral == null)
            {
                return null;
            }
            return peripheral.peripheralInfo;
        }

        /// <summary>
        /// Gets the current status of the device. 
        /// </summary>
        /// <returns></returns>
        public Peripheralstatus QwrpGetDeviceCurrentStatus()
        {
            if (peripheral == null)
            {
                return null;
            }
            return peripheral.peripheralstatus;
        }

        /// <summary>
        /// Gets the previous status of the device.
        /// </summary>
        /// <returns></returns>
        public Peripheralstatus QwrpGetDevicePrevStatus()
        {
            if (peripheral == null)
            {
                return null;
            }
            return peripheral.prevperipheralstatus;
        }
        
        /// <summary>
        /// Updates the state of the device.
        /// </summary>
        public virtual void QwrpUpdate()
        {
            if (peripheral == null)
            {
                return;
            }
            peripheral.QwrupUpdateDeviceState();
        }

        /// <summary>
        /// Suspends the running process of the device.
        /// </summary>
        public virtual void QwrpPause()
        {
            if (peripheral == null)
            {
                return;
            }
            peripheral.QwrupPause();
        }

        /// <summary>
        /// Resumes the peripheral.
        /// </summary>
        public virtual void QwrpResume()
        {
            if (peripheral == null)
            {
                return;
            }
            peripheral.QwrupResume();
        }

        public override string ToString()
        {
            return peripheral._deviceId;
        }

        virtual public void EnableInternalObjects(bool enabled)
        {

        }

        virtual public bool IsActivating()
        {
            return false;
        }

        virtual public void EnableModel(bool enabled)
        {

        }
        
        protected bool _isValid = true;

        public bool isValid
        {
            get
            {
                return _isValid;
            }
        }

        /// <summary>
        /// ray parent
        /// </summary>
        [SerializeField]
        Transform rayParent;

        /// <summary>
        /// ray origin
        /// </summary>
        [SerializeField]
        Vector3 rayOrigin;

        /// <summary>
        /// ray tilted angle, rotate around x axis
        /// </summary>
        [SerializeField]
        float rayTiltedAngle = 0.0f;

        Matrix4x4 rayLocalMatrix;

        virtual public Ray GetRay()
        {
            if(rayParent != null)
                return new Ray(rayParent.position, rayParent.forward);
            else
                return new Ray(transform.position, transform.forward);
        }
    }
}
