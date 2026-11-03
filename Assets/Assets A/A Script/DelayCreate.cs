using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;


public class DelayCreate : MonoBehaviour {
    public float _height, _time,_delayTime;
    public List<GameObject> _next;
	void OnEnable () {
        transform.DOScaleY(_height, _time).SetEase(Ease.Linear).OnComplete(()=> {
            foreach(var i in _next)
            {
                i.SetActive(true);
            }
        });
	}
}