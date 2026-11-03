using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    Ray ray;
    bool isDown;
    bool isDropped;
    RaycastHit hit;
    private Transform dot;
    float disX, disY, disZ;
    GameObject referenceObj;
    Pvr_Controller controller;
    public UnityEvent AfterDrag;
    private Transform dragObj, originalParent;
    Vector3 originalScale, posHolder, rotHolder;

    public GameObject _trueTxt;
    public List<GameObject> _falseObj, _trueObj;

    void Start()
    {
        posHolder = transform.position;
        rotHolder = transform.localEulerAngles;
        originalScale = transform.localScale;
        originalParent = transform.parent;

        referenceObj = new GameObject("ReferenceObj");
        referenceObj.transform.parent = transform.parent;
        ray = new Ray();

        controller = Pvr_ControllerManager.Instance.GetComponent<Pvr_Controller>();
        dot = controller.controller1.transform.Find("dot");
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        // transform.localScale = Vector3.one * 0.3f;
        // AfterDrag1();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // transform.localScale = Vector3.one * 0.25f;
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (isDropped)
            return;

        referenceObj.transform.position = dot.position;
        disX = transform.position.x - referenceObj.transform.position.x;
        disY = transform.position.y - referenceObj.transform.position.y;
        dragObj = transform;

        isDown = true;
        GetComponent<BoxCollider>().enabled = false;
        gameObject.layer = 10;
    }

    void Update()
    {
        if (isDown)
        {
            Pvr_ControllerRay.Instance.lockDistance = true;
            referenceObj.transform.position = new Vector3(dot.position.x, dot.position.y, dot.position.z);
            dragObj.position = new Vector3(referenceObj.transform.position.x + disX, referenceObj.transform.position.y + disY, dot.position.z);
        }
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        isDown = false;
        gameObject.layer = 5;
        transform.parent = originalParent;
        Pvr_ControllerRay.Instance.lockDistance = false;
        if (Pvr_ControllerRay.Instance.currentHit != null)
        {
            if (Pvr_ControllerRay.Instance.currentHit.tag == transform.tag)
            {
                SoundManager.sManager.PlayRightSound();
                transform.parent = Pvr_ControllerRay.Instance.currentHit.transform;
                transform.DOLocalMove(Pvr_ControllerRay.Instance.currentHit.transform.GetChild(0).localPosition, 0.2f).SetEase(Ease.Linear).OnComplete(AfterHitCall);
                Pvr_ControllerRay.Instance.currentHit.transform.GetComponent<BoxCollider>().enabled = false;
                gameObject.layer = 2;
                isDropped = true;
            }
            else
            {
                OnWrongHit();
            }
        }
        else
        {
            OnWrongHit();
        }
    }

    void AfterHitCall()
    {
        transform.localEulerAngles = Vector3.zero;
        Invoke("LastMsg", 1f);
        this.enabled = false;
    }

    void LastMsg()
    {
        AfterDrag.Invoke();
    }

    void OnWrongHit()
    {
        SoundManager.sManager.PlayWrongSound();
        transform.DOMove(posHolder, 0.1f).SetEase(Ease.Linear).OnComplete(AfterTweenCall);
        transform.localEulerAngles = rotHolder;
        transform.localScale = originalScale;
    }

    void AfterTweenCall()
    {
        GetComponent<Collider>().enabled = true;
    }
}