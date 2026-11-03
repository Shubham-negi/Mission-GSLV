using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class DelayCreate1 : MonoBehaviour {
    public float _height, _time,_delayTime;
    public List<GameObject> _next;

    private void Start()
    {
        transform.DOScaleY(_height, _time).SetEase(Ease.Linear).OnComplete(() => {
            foreach (var i in _next)
            {
                i.SetActive(true);
            }
        });
    }
    //void  () {
    //       transform.DOScaleY(_height, _time).SetEase(Ease.Linear).OnComplete(()=> {
    //           foreach(var i in _next)
    //           {
    //               i.SetActive(true);
    //           }
    //       });
    //}
    }