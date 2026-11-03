using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using qwr;

public class DragDropUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    bool isDown;
    bool isDropped;
    public float scale;

    private Transform originalParent;
    Vector3 originalScale, posHolder, rotHolder;

    public Vector3 screenSpace;
    public Vector3 offset;

    public GameObject _trueTxt;
    
    void Start()
    {
        posHolder = transform.position;
        rotHolder = transform.localEulerAngles;
        originalScale = transform.localScale;
        originalParent = transform.parent;

       }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (isDropped)
            return;

        isDown = true;
        screenSpace = Camera.main.WorldToScreenPoint(transform.position);
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(QwrPointerManager.Instance.pointer.GetScreenPosition().x, QwrPointerManager.Instance.pointer.GetScreenPosition().y, screenSpace.z));

       GetComponent<BoxCollider>().enabled = false;
       GetComponentInChildren<Text>().material = GameManagerScript.instance.overlayMat;
    }

    void Update()
    {
        if (isDown)
        {
            // transform.position = laserPointer.cursorVisual.transform.position;
            transform.localEulerAngles = Vector3.zero;
            var curScreenSpace = new Vector3(QwrPointerManager.Instance.pointer.GetScreenPosition().x, QwrPointerManager.Instance.pointer.GetScreenPosition().y, screenSpace.z);

            //convert the screen mouse position to world point and adjust with offset
            var curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;

            //update the position of the object in the world
            transform.position = curPosition;
        }
    }
    Ray ray;
    RaycastHit hit;

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        isDown = false;
        transform.parent = originalParent;
        GetComponentInChildren<Text>().material = null;

        ray = Camera.main.ScreenPointToRay(QwrPointerManager.Instance.pointer.GetScreenPosition());

        if (Physics.Raycast(ray, out hit, 1000))
        {

            if (hit.collider.tag == transform.tag)
            {
                SoundManager.sManager.PlayRightSound();
                hit.transform.GetComponent<BoxCollider>().enabled = false;
                hit.transform.GetComponent<Animator>().enabled = false;
                hit.transform.GetComponent<Image>().enabled = false;

              //  print(Pvr_ControllerRay.Instance.currentHit.gameObject.name);
                isDropped = true;
                GameManagerScript.instance.dragCounter++;
                _trueTxt.SetActive(true);
                print("count : " + GameManagerScript.instance.dragCounter);
                if (GameManagerScript.instance.dragCounter == 5)
                {
                    GameManagerScript.instance._uObjects[0].SetActive(true);                    
                    GameManagerScript.instance._uObjects[1].SetActive(false);                    
                }
               
                gameObject.SetActive(false);
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

    void AfterDrag()
	{
        //CircleGameManager.instance._instructionsA[11].SetActive(false);
        //CircleGameManager.instance._endMsg.SetActive(true);
        //CircleGameManager.instance.dragCounter = 0;
    }
}