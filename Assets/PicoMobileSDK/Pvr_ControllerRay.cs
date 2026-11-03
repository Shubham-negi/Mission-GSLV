// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;

public class Pvr_ControllerRay : MonoBehaviour
{
    public GameObject HeadSetController;

    float dotZPos;
    private Ray ray;
    private RaycastHit hit;
    private PointerEventData ped;
    public Pvr_Controller pvr_Controller;
    [HideInInspector]
    public bool lockDistance;
    [HideInInspector]
    public Transform lastHit, currentHit;
    [HideInInspector]
    public float rayDefaultLength = 4;
    internal GameObject referenceObj,currentController, dot, rayLine;
    private bool isHasController = false, headcontrolmode = false, noClick;

    public static void GetInterfaces<T>(out List<T> resultList, GameObject objectToSearch) where T : class
    {
        MonoBehaviour[] list = objectToSearch.GetComponents<MonoBehaviour>();
        resultList = new List<T>();
        foreach (MonoBehaviour mb in list)
        {
            if (mb is T)
            {
                //found one
                resultList.Add((T)((System.Object)mb));
            }
        }
    }

    /// Const to use for clarity when no event mask is set
    /// </summary>
    protected const int kNoEventMaskSet = -1;

    protected Camera m_EventCamera;

    /// <summary>
    /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
    /// </summary>
    [SerializeField]
    protected LayerMask m_EventMask = kNoEventMaskSet;

    /// <summary>
    /// Event mask used to determine which objects will receive events.
    /// </summary>
    public int finalEventMask
    {
        get { return (Camera.main != null) ? Camera.main.cullingMask & m_EventMask : kNoEventMaskSet; }
    }

    /// <summary>
    /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
    /// </summary>
    public LayerMask eventMask
    {
        get { return m_EventMask; }
        set { m_EventMask = value; }
    }

    public static Pvr_ControllerRay Instance;

    private float depth;
    private Vector3 _startPoint,_addDiffrence = new Vector3(0.1f,0,0.5f);
    public float depthScaleMultiplier = 0.05f;
    float scale;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ray = new Ray();
        hit = new RaycastHit();
        if (Pvr_UnitySDKManager.SDK.isHasController)
        {
            Pvr_ControllerManager.PvrServiceStartSuccessEvent += ServiceStartSuccess;
            Pvr_ControllerManager.SetControllerStateChangedEvent += ControllerStateListener;
            Pvr_ControllerManager.ControllerStatusChangeEvent += CheckControllerStateForGoblin;
            isHasController = true;
#if UNITY_EDITOR
            HeadSetController.SetActive(false);
            currentController = pvr_Controller.controller1;
            dot = pvr_Controller.controller1.transform.Find("dot").gameObject;
            dot.SetActive(true);
            rayLine = pvr_Controller.controller1.transform.Find("ray_LengthAdaptive").gameObject;
            rayLine.SetActive(true);
#endif
        }
        referenceObj = new GameObject("ReferenceObj");
    }

    void OnDestroy()
    {
        if (isHasController)
        {
            Pvr_ControllerManager.PvrServiceStartSuccessEvent -= ServiceStartSuccess;
            Pvr_ControllerManager.SetControllerStateChangedEvent -= ControllerStateListener;
            Pvr_ControllerManager.ControllerStatusChangeEvent -= CheckControllerStateForGoblin;
        }
    }
    private bool canEnter = true;
    private bool canExit = true;
    private bool isDown = false;

    Transform currentPickedObject;

    void Update()
    {
        if (HeadSetController.activeSelf)
        {
            HeadSetController.transform.parent.localRotation = Quaternion.Euler(Pvr_UnitySDKSensor.Instance.HeadPose.Orientation.eulerAngles.x, Pvr_UnitySDKSensor.Instance.HeadPose.Orientation.eulerAngles.y, 0);
            HeadSetController.transform.parent.localPosition = Pvr_UnitySDKSensor.Instance.HeadPose.Position;

            ray.direction = HeadSetController.transform.position - HeadSetController.transform.parent.parent.Find("Head").position;
            ray.origin = HeadSetController.transform.parent.parent.Find("Head").position;

            if (Physics.Raycast(ray, out hit, finalEventMask))
            {
                if (HeadSetController.name == "SightPointer")
                {
                    HeadSetController.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
                }

                currentHit = hit.transform;

                if (currentHit != null && lastHit != null && currentHit != lastHit)
                {
                    if (lastHit.GetComponent<Pvr_UIGraphicRaycaster>() && lastHit.transform.gameObject.activeInHierarchy && lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                    {
                        lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = false;
                    }
                }
                if (currentHit != null && lastHit != null && currentHit == lastHit)
                {
                    if (currentHit.GetComponent<Pvr_UIGraphicRaycaster>() && !currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                    {
                        currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = true;
                    }
                }
                lastHit = hit.transform;
#if UNITY_EDITOR
                Debug.DrawLine(ray.origin, hit.point, Color.red);
#endif
                if (Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                {
                    HeadSetController.transform.position = hit.point;
                    HeadSetController.transform.position -= (hit.point - HeadSetController.transform.parent.parent.Find("Head").position).normalized * 0.02f;
                    float scale = 0.008f * hit.distance / 4f;
                    Mathf.Clamp(scale, 0.002f, 0.008f);
                    HeadSetController.transform.localScale = new Vector3(scale, scale, 1);
                }
            }
            else
            {
                if (HeadSetController.name == "SightPointer")
                {
                    HeadSetController.transform.localScale = Vector3.zero;
                }

                currentHit = null;
                noClick = false;

                if (Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                {
                    HeadSetController.transform.position = HeadSetController.transform.parent.parent.Find("Head").position + ray.direction.normalized * (0.5f + rayDefaultLength);
                    HeadSetController.transform.localScale = new Vector3(0.008f, 0.008f, 1);
                }
            }

            if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetMouseButtonDown(0))
            {
                if (lastHit != null && currentHit != null)
                {
                    noClick = true;
                }
            }
        }
        else
        {
            if (currentController != null)
            {
                ray.direction = currentController.transform.forward - currentController.transform.up * 0.25f;
                ray.origin = currentController.transform.Find("start").position;

                if (Physics.Raycast(ray, out hit))
                {
                    currentHit = hit.transform;
                    if (currentHit != null && lastHit != null && currentHit != lastHit)
                    {
                        if (lastHit.GetComponent<Pvr_UIGraphicRaycaster>() && lastHit.transform.gameObject.activeInHierarchy && lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                        {
                            lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = false;
                        }

                        List<IPointerEnterHandler> interfaceList;
                        GetInterfaces<IPointerEnterHandler>(out interfaceList, currentHit.gameObject);
                        foreach (IPointerEnterHandler pointers in interfaceList)
                        {
                            pointers.OnPointerEnter(ped);
                        }
                        canEnter = false;
                        List<IPointerExitHandler> interfaceList1;
                        GetInterfaces<IPointerExitHandler>(out interfaceList1, lastHit.gameObject);
                        foreach (IPointerExitHandler pointers in interfaceList1)
                        {
                            pointers.OnPointerExit(ped);
                        }
                        canExit = false;
                    }
                    if (currentHit != null && lastHit != null && currentHit == lastHit)
                    {
                        if (currentHit.GetComponent<Pvr_UIGraphicRaycaster>() && !currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                        {
                            currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = true;
                        }
                    }


                    if (!noClick && canEnter)
                    {
                        List<IPointerEnterHandler> interfaceList;
                        GetInterfaces<IPointerEnterHandler>(out interfaceList, currentHit.gameObject);
                        foreach (IPointerEnterHandler pointers in interfaceList)
                        {
                            pointers.OnPointerEnter(ped);
                        }
                        canEnter = false;
                        canExit = true;
                    }

                    if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TOUCHPAD)
                      || Input.GetMouseButtonDown(0) || Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TRIGGER) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TRIGGER))
                    {

                        currentPickedObject = currentHit;
                        referenceObj.transform.position = hit.point;
                        disX = hit.transform.position.x - referenceObj.transform.position.x;
                        disY = hit.transform.position.y - referenceObj.transform.position.y;
                        dragObj = hit.transform;
                        List<IPointerDownHandler> interfaceList;
                        GetInterfaces<IPointerDownHandler>(out interfaceList, currentHit.gameObject);
                        foreach (IPointerDownHandler pointers in interfaceList)
                        {
                            pointers.OnPointerDown(ped);
                        }
                        isDown = true;
                    }
                    if (Controller.UPvr_GetKey(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKey(1, Pvr_KeyCode.TOUCHPAD)
                    || Input.GetMouseButton(0) || Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TRIGGER) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TRIGGER))
                    {

                    }

                    if (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.TOUCHPAD)
                        || Input.GetMouseButtonUp(0) || Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.TRIGGER) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.TRIGGER))
                    {
                        if (currentPickedObject == currentHit)
                        {
                            List<IPointerUpHandler> interfaceList;
                            GetInterfaces<IPointerUpHandler>(out interfaceList, currentPickedObject.gameObject);
                            foreach (IPointerUpHandler pointers in interfaceList)
                            {
                                pointers.OnPointerUp(ped);
                            }
                            currentPickedObject = null;
                        }

                        else
                        {
                            if (currentPickedObject != null)
                            {
                                List<IPointerUpHandler> interfaceList;
                                GetInterfaces<IPointerUpHandler>(out interfaceList, currentPickedObject.gameObject);
                                foreach (IPointerUpHandler pointers in interfaceList)
                                {
                                    pointers.OnPointerUp(ped);
                                }
                                currentPickedObject = null;
                            }
                        }
                    }


                    lastHit = hit.transform;

#if UNITY_EDITOR
                    Debug.DrawLine(ray.origin, hit.point, Color.red);
#endif
                    currentController.transform.Find("dot").position = hit.point;
                    if (!lockDistance)
                        dotZPos = currentController.transform.Find("dot").position.z;

                    if (lockDistance)
                    {
                        currentController.transform.Find("dot").position =
                        new Vector3(currentController.transform.Find("dot").position.x, currentController.transform.Find("dot").position.y, dotZPos);
                    }

                   // if (Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                  //  {
                        _startPoint = new Vector3(0, 0, 0);
                        depth = (_startPoint - (currentController.transform.Find("dot").localPosition+ _addDiffrence)).magnitude;
                        scale = depth * depthScaleMultiplier;
                        currentController.transform.Find("dot").localScale = new Vector3(scale, scale, scale);
                   // }
                }
                else
                {
                    if (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.TOUCHPAD)
                        || Input.GetMouseButtonUp(0) || Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.TRIGGER) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.TRIGGER))
                    {
                        if (currentPickedObject != null)
                        {
                            List<IPointerUpHandler> interfaceList;
                            GetInterfaces<IPointerUpHandler>(out interfaceList, currentPickedObject.gameObject);
                            foreach (IPointerUpHandler pointers in interfaceList)
                            {
                                pointers.OnPointerUp(ped);
                            }
                            currentPickedObject = null;
                        }
                    }

                    if (lastHit != null && canExit)
                    {
                        List<IPointerExitHandler> interfaceList;
                        GetInterfaces<IPointerExitHandler>(out interfaceList, lastHit.gameObject);
                        foreach (IPointerExitHandler pointers in interfaceList)
                        {
                            pointers.OnPointerExit(ped);
                        }
                        canExit = false;
                    }

                    currentHit = null;
                    noClick = false;
                    canEnter = true;

                    if (Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                    {
                        _startPoint = new Vector3(0, 0, 0f);
                        depth = (_startPoint - (currentController.transform.Find("dot").localPosition + _addDiffrence)).magnitude;
                        scale = depth * depthScaleMultiplier;
                        currentController.transform.Find("dot").localScale = new Vector3(scale, scale, scale);
                    }
                }
#if UNITY_EDITOR
                rayLine.GetComponent<LineRenderer>().SetPosition(0, currentController.transform.TransformPoint(0, 0, 0.072f));
                rayLine.GetComponent<LineRenderer>().SetPosition(1, dot.transform.position);

                _startPoint = new Vector3(0, 0, 0f);
                depth = (_startPoint - (currentController.transform.Find("dot").localPosition + _addDiffrence)).magnitude;
                scale = depth * depthScaleMultiplier;
                currentController.transform.Find("dot").localScale = new Vector3(scale, scale, scale);
                
#endif
            }

            if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TOUCHPAD)
                || Input.GetMouseButtonDown(0) || Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TRIGGER) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TRIGGER))
            {
                if (lastHit != null && currentHit != null)
                {
                    List<IPointerClickHandler> interfaceList;
                    GetInterfaces<IPointerClickHandler>(out interfaceList, currentHit.gameObject);
                    foreach (IPointerClickHandler pointers in interfaceList)
                    {
                        pointers.OnPointerClick(ped);
                    }

                    isDown = false;
                    noClick = true;
                }
            }
        }
    }
    private Transform dragObj;
    float disX, disY, disZ;

    private void ServiceStartSuccess()
    {
        if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
            Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
        {
            HeadSetController.SetActive(false);
        }
        else
        {
            HeadSetController.SetActive(true);
        }
        if (Controller.UPvr_GetMainHandNess() == 0)
        {
            currentController = pvr_Controller.controller0;
        }
        if (Controller.UPvr_GetMainHandNess() == 1)
        {
            currentController = pvr_Controller.controller1;
        }
    }

    private void ControllerStateListener(string data)
    {

        if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
            Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
        {
            HeadSetController.SetActive(false);
        }
        else
        {
            HeadSetController.SetActive(true);
        }

        if (Controller.UPvr_GetMainHandNess() == 0)
        {
            currentController = pvr_Controller.controller0;
        }
        if (Controller.UPvr_GetMainHandNess() == 1)
        {
            currentController = pvr_Controller.controller1;
        }
    }

    private void CheckControllerStateForGoblin(string state)
    {
        HeadSetController.SetActive(Convert.ToInt16(state) != 1);
    }

    public void SwitchControlMode()
    {
#if UNITY_EDITOR
        if (headcontrolmode)
        {
            headcontrolmode = false;
            HeadSetController.SetActive(false);
            pvr_Controller.controller0.SetActive(true);
            pvr_Controller.controller1.SetActive(true);
        }
        else
        {
            headcontrolmode = true;
            HeadSetController.SetActive(true);
            pvr_Controller.controller0.SetActive(false);
            pvr_Controller.controller1.SetActive(false);
        }
#endif
    }
}
