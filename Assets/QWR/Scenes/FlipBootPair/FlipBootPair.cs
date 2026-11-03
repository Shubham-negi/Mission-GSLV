using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace qwr
{
    public class FlipBootPair : MonoBehaviour
    {

        bool _paired = false;
        /// <summary>
        /// Starts this instance.Use this for initialization
        /// </summary>
        void Start()
        {
            _btnPair = transform.Find("Panel/Pair").gameObject;
            _btnUnpair = transform.Find("Panel/Unpair").gameObject;

            _handle_bg_1 = transform.Find("Panel/handle_bg_1").gameObject;
            _handle_bg_2 = transform.Find("Panel/handle_bg_2").gameObject;
            _handle_bg_3 = transform.Find("Panel/handle_bg_3").gameObject;
            _handle_bg_4 = transform.Find("Panel/handle_bg_4").gameObject;
            _textDeviceName = transform.Find("Panel/Text").gameObject;

            _btnUnpair.SetActive(false);
            _handle_bg_1.SetActive(false);
            _handle_bg_2.SetActive(false);
            _handle_bg_3.SetActive(false);
            _handle_bg_4.SetActive(false);

            // add message listener for message on flip connected
            QwrDaydreamController.onConnected += OnPeripheralConnected;

            string deviceName = QwrDaydreamController.GetBondDeviceName();
            if(deviceName != "")
            {
                _paired = true;
                _btnPair.SetActive(false);
                _btnUnpair.SetActive(true);
                SetCurrent(_handle_bg_4);
                SetDeviceName(deviceName);
            }

        }

        string _deviceName;

        void SetDeviceName(string name)
        {
            _deviceName = name;

            if (_textDeviceName == null)
                return;

            Text comp = _textDeviceName.GetComponent<Text>();
            if(comp)
                comp.text = "DeviceName:" + _deviceName;
        }

        GameObject _btnPair = null;
        GameObject _btnUnpair = null;

        GameObject _handle_bg_1 = null;
        GameObject _handle_bg_2 = null;
        GameObject _handle_bg_3 = null;
        GameObject _handle_bg_4 = null;
        GameObject _textDeviceName = null;
        GameObject _currentObject = null;

        // Update is called once per frame
        void Update()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.A))
            {
                OnPairSucceed();
            }
#endif
        }
        private IEnumerator _bootPairOvertime = null;

        void OnPairSucceed()
        {
            _paired = true;
            _btnPair.SetActive(false);
            _btnUnpair.SetActive(true);
            SetCurrent(_handle_bg_4);

            if (_bootPairOvertime != null)
            {
                StopCoroutine(_bootPairOvertime);
                _bootPairOvertime = null;
            }

            // Notify the system to stop boot pair
            QwrDaydreamController.StopBootPair();

            // get paired device name
            string name = QwrDaydreamController.GetBondDeviceName();
            SetDeviceName(name);
        }

        void OnPairFailed()
        {
            SetCurrent(_handle_bg_3);
            _btnPair.SetActive(true);
            _btnUnpair.SetActive(false);
            SetDeviceName("");

            // Notify the system to stop boot pair
            QwrDaydreamController.StopBootPair();
        }

        void OnPairStart()
        {
            SetCurrent(_handle_bg_1);
            _btnUnpair.SetActive(false);
            _btnPair.SetActive(false);

            // Notify the system to start boot pair
            QwrDaydreamController.StartBootPair();

            _bootPairOvertime = _BootPairOvertime();
            StartCoroutine(_bootPairOvertime);
        }

        IEnumerator _BootPairOvertime()
        {
            yield return new WaitForSeconds(20.0f);
            //yield return new WaitForSeconds(40.0f);
            if(_paired == false)
            {
                OnPairFailed();
            }
        }

        void SetCurrent(GameObject currentObject)
        {
            if (_currentObject)
                _currentObject.SetActive(false);

            if (currentObject)
                currentObject.SetActive(true);

            _currentObject = currentObject;
        }


        void OnPeripheralConnected(QwrBasePeripheral peripheral)
        {
            // if paired succeed, the flip controller will be connected,
            // and it will send the message on the connection by QwrDaydreamController
            // when app start, if the flip controller is paired, it will send the message also.
            if(!_paired)
                OnPairSucceed();
        }

        #region FlipBootPair implementation

        // called by button click
        public void Pair()
        {
            OnPairStart();
        }

        // called by button click
        public void Unpair()
        {
            OnUnpair();
        }

        #endregion

        void OnUnpair()
        {
            _paired = false;
            _btnUnpair.SetActive(false);
            _btnPair.SetActive(false);
            ResetTips();
            SetDeviceName("");

            // unbind controller
            QwrDaydreamController.Unbind();
        }

        void ResetTips()
        {
            _btnPair.SetActive(true);
            _btnUnpair.SetActive(false);

            if(_currentObject)
                _currentObject.SetActive(false);
        }
    }
}

