using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QWR
{
    public partial class QWRDeviceManager
    {
        public class SystemSleepManager
        {
            public static void BlockSystemSleep()
            {
                //TODO
                //blockFunctions |= (BLOCK_SYSTEM_SLEEP | BLOCK_POWERKEY);
                //GetDeviceManager().Call("blockFunctions", blockFunctions);
                SystemKeyManager.BlockScreenOff();  //Workaround P1Pro not support BlockSystemSleep issue.
            }

            public static void AllowSystemSleep()
            {
                //TODO
                //blockFunctions &= ~ (BLOCK_SYSTEM_SLEEP | BLOCK_POWERKEY);
                //GetDeviceManager().Call("blockFunctions", blockFunctions);
                SystemKeyManager.AllowScreenOff();  //Workaround P1Pro not support BlockSystemSleep issue.
            }
        }
    }
}
