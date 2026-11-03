/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace qwr
{
    public class QwrPointerManager : MonoBehaviour
    {
        [SerializeField]
        public QwrBasePointer pointer;

        [SerializeField]
        public QwrBaseRay ray;

        static public QwrPointerManager Instance
        {
            private set;
            get;
        }

        public QwrPointerManager()
        {
            // if (Instance == null)
            {
                Instance = this;
            }
        }

        void OnDestroy()
        {
            if(Instance == this)
                Instance = null;
        }

        void Awake()
        {
            IsRaycastEnabled = true;
        }

        void OnEnable()
        {
            QwrCameraRig.onPeripheralChanged += OnPeripheralChanged;
            QwrCameraRig.onRecenter += OnRencenter;
        }

        void OnDisable()
        {
            QwrCameraRig.onPeripheralChanged -= OnPeripheralChanged;
            QwrCameraRig.onRecenter -= OnRencenter;
        }

        void Start()
        {

        }

        IEnumerator _SetRaycastEnable()
        {
            yield return new WaitForEndOfFrame();
            pointer.gameObject.SetActive(true);
            ray.gameObject.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRaycastEnabled
        {
            get;set;
        }

        GameObject _lastGameObejct = null;

        void OnNoInteraction(Ray castRay, Quaternion pointerRotation)
        {
            float z = (QwrCameraRig._instance._center_eye.nearClipPlane + QwrCameraRig._instance._center_eye.farClipPlane) * 0.5f;
            Vector3 worldPos = castRay.origin + castRay.direction * z;

            SetPointerMatrix(worldPos, pointerRotation);

            if (_lastGameObejct != null)
            {
                _lastGameObejct = null;
                pointer.transform.localScale = Vector3.one;
                pointer.OnExitObject(_lastGameObejct);
            }
        }

        public void ProcessMove(PointerEventData pointerEvent)
        {
            if (!QwrManager.QWRPointer || QwrCameraRig._instance == null)
                return;

            GameObject castObject = pointerEvent.pointerCurrentRaycast.gameObject;

            Ray castRay = QwrCameraRig._instance.CurrentPeripheral.GetRay();
            // Transform rayOrig = QwrCameraRig._instance.CurrentPeripheral.RayOrig;

            Quaternion pointerRotation = Quaternion.LookRotation(castRay.direction);

            if (castObject && IsRaycastEnabled)
            {
                BaseRaycaster raycaster = pointerEvent.pointerCurrentRaycast.module;
                Ray intersectionRay = new Ray();
                if (raycaster != null)
                {
                    intersectionRay = castRay;
                }
                else if (pointerEvent.enterEventCamera != null)
                {
                    Camera cam = pointerEvent.enterEventCamera;
                    intersectionRay = new Ray(cam.transform.position, cam.transform.forward);
                }

                Vector3 intersectionPos = pointerEvent.pointerCurrentRaycast.worldPosition;
                if (intersectionPos == Vector3.zero)
                {
                    Camera camera = pointerEvent.enterEventCamera;
                    if (camera != null)
                    {
                        float intersectionDistance = pointerEvent.pointerCurrentRaycast.distance + camera.nearClipPlane;
                        intersectionPos = camera.transform.position + intersectionRay.direction * intersectionDistance;
                    }
                }

                SetPointerMatrix(intersectionPos, pointerRotation);

                if (_lastGameObejct == null || _lastGameObejct != castObject)
                {
                    _lastGameObejct = castObject;

                    pointer.OnEnterObject(_lastGameObejct);
                    pointer.transform.localScale = Vector3.zero;
                }
                else
                {
                    pointer.transform.localScale = Vector3.one;
                    pointer.OnHoverObject(_lastGameObejct);
                }

            }
            else
            {
                OnNoInteraction(castRay, pointerRotation);
            }

        }

        void SetPointerMatrix(Vector3 position, Quaternion rotaion)
        {
            if (pointer == null)
                return;

            if (QwrUICamera.instance != null && pointer.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                pointer.transform.position = new Vector3(0, 0, (position - QwrCameraRig.Instance.transform.position).magnitude);
                pointer.transform.rotation = Quaternion.identity;
            }
            else
            {
                pointer.transform.position = position;
                pointer.transform.rotation = rotaion;
            }

        }

        void LateUpdate()
        {
            if (QwrCameraRig._instance.CurrentPeripheral == null)
                return;
            if(QwrStandaloneInputModule.Instance == null)
            {
                Ray castRay = QwrCameraRig._instance.CurrentPeripheral.GetRay();
                Quaternion pointerRotation = Quaternion.LookRotation(castRay.direction);
                OnNoInteraction(castRay, pointerRotation);
            }

            Ray r = QwrCameraRig._instance.CurrentPeripheral.GetRay();
            ray.Set(r.origin, pointer.transform.position);
        }

        IEnumerator _SetRayVisible()
        {
            yield return new WaitForEndOfFrame();
            ray.SetVisible(QwrCameraRig._instance.CurrentPeripheral.PeripheralType != QWRPeripheral.None);
        }

        void OnPeripheralChanged(QwrBasePeripheral peripheral)
        {
            StartCoroutine(_SetRayVisible());
        }

        void OnRencenter()
        {
            pointer.SetVisible(false);
            ray.SetVisible(false);

            StartCoroutine(_OnRecenter());
        }

        IEnumerator _OnRecenter()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            pointer.SetVisible(true);
            ray.SetVisible(true);
        }

        bool isPointerEnabled = true;

        public bool IsPointerEnabled
        {
            get
            {
                return isPointerEnabled;
            }
            set
            {
                pointer.gameObject.SetActive(value);
                isPointerEnabled = value;
            }
            
        }

        bool isRayEnabled = true;

        public bool IsRayEnabled
        {
            get
            {
                return IsRayEnabled;
            }
            set
            {
                ray.gameObject.SetActive(value);
                isRayEnabled = value;
            }
        }
    }
}
