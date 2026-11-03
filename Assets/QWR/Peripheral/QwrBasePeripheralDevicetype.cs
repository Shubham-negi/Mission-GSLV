/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System.Collections;

namespace qwr
{
    public class QwrBasePeripheralDevicetype : QwrBasePeripheral
    {
        public QWRP_DEVICE_TYPE device_type = QWRP_DEVICE_TYPE.QWRP_DEVICE_TYPE_NONE;
        public int device_index = 0;

        public virtual void OnEnable()
        {
            if (device_type != QWRP_DEVICE_TYPE.QWRP_DEVICE_TYPE_NONE)
            {
                OpenPeripheral(device_type, device_index);
            }
        }
    }
}
