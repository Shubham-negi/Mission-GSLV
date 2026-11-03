using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using qwr;

namespace QWR.Rwizen.Demo
{
    public class DragDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {


        Ray ray;
        RaycastHit hit;
        Vector3 originalScale;
        Vector3 posHolder, rotHolder;
        Vector3 placingPos;
        Transform originalParent;

        bool isDropped;
        bool isDown;

        public UnityEvent AfterDrag;


        public static int dragCount;

        public Vector3 screenSpace;
        public Vector3 offset;

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
            transform.localEulerAngles = Vector3.zero;
            gameObject.layer = 10;

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

        public void OnPointerUp(PointerEventData pointerEventData)
        {
            isDown = false;

            gameObject.layer = 5;
            transform.parent = originalParent;


            ray = Camera.main.ScreenPointToRay(QwrPointerManager.Instance.pointer.GetScreenPosition());

            if (Physics.Raycast(ray, out hit, 1000))
            {

                if (hit.collider.tag == transform.tag && hit.collider.transform != transform)
                {

                    Debug.Log("Hitted!!");
                    SoundManager.sManager.PlayRightSound();
                    transform.parent = hit.transform;
                    transform.localScale = originalScale;
                    transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.Linear).OnComplete(AfterHitCall);
                    hit.transform.GetComponent<BoxCollider>().enabled = false;
                    SoundManager.sManager.PlayRightSound();
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.localScale = originalScale * 1.25f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = originalScale;
        }
    }
}