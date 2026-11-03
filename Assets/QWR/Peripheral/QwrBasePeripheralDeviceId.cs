using UnityEngine;
using System.Collections;

namespace qwr
{
    public class QwrBasePeripheralDeviceId : QwrBasePeripheral
    {
        public string deviceId = null;
        public virtual void OnEnable()
        {
            if (deviceId != null)
            {
                OpenPeripheral(deviceId);
            }
        }
    }
}
