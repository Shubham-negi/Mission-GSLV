using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using qwr;
//using DG.Tweening;

namespace qwr
{
    public class QwrDaydreamControllerRecenterObserver : MonoBehaviour
    {
        MeshRenderer _meshRender = null;

        [Range(0.0f, 1.0f)]
        float _countdownValue = 0.0f;
        GameObject _countDown = null;

        private void Awake()
        {
            _countDown = transform.Find("countDown").gameObject;
            if (_countDown)
            {
                _meshRender = _countDown.GetComponent<MeshRenderer>();
            }

            _countDown.gameObject.SetActive(false);
        }

        void Update()
        {
            transform.rotation = QwrCameraRig._instance._center_transform.rotation;

            if (QwrDaydreamController.RecenterBegin)
            {
                OnRecenterBegin();
            }
            else if (QwrDaydreamController.Recentering)
            {
                OnRecentering();
            }
            else if (QwrDaydreamController.Recentered)
            {
                OnRecentered();
            }
            else if (QwrDaydreamController.RecenterCancel)
            {
                OnRecenterCancel();
            }
            else
            {
                return;
            }

            ApplyMaterial();
        }


        void OnRecentering()
        {
            if (!_countDown || _countDown.activeSelf == false)
                return;

            _countdownValue += Time.deltaTime * 1.05f;
        }

        void OnRecentered()
        {
            if (!_countDown || _countDown.activeSelf == false)
                return;

            _countdownValue = 0.0f;
            SetCountDownActive(false);
        }

        void OnRecenterCancel()
        {
            if (!_countDown || _countDown.activeSelf == false)
                return;

            _countdownValue = 0.0f;
            SetCountDownActive(false);
        }

        void OnRecenterBegin()
        {
            if (!_countDown)
                return;

            _countdownValue = 0.0f;
            SetCountDownActive(true);
        }

        void SetCountDownActive(bool active)
        {
            if (_countDown)
                _countDown.SetActive(active);
        }


        void ApplyMaterial()
        {
            if (_countDown == null)
                return;

            _meshRender.material.SetFloat("_countdownValue", _countdownValue);
        }
    }
}

