using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class OnEnableLabelCreate : MonoBehaviour {
	public GameObject SecondPoint,EndPoint,LabelText;
	public Vector3 SecondPointPosition,EndPointPosition;

	public bool IsTrue = false;

	void OnEnable(){
		if(IsTrue){
			SecondPoint.transform.DOLocalMove (SecondPointPosition,0.5f).OnComplete(EndPointSetPosition);
		}else{
			SecondPointPosition = SecondPoint.transform.localPosition;
			EndPointPosition = EndPoint.transform.localPosition;
			LabelText.transform.DOScale (Vector3.zero,0.0f);
			IsTrue = true;
		}
	}

	void EndPointSetPosition(){
		EndPoint.transform.DOLocalMove (EndPointPosition,0.5f);
		LabelText.transform.DOScale (Vector3.one,0.5f);
	}

	public void OnDisable(){
		LabelText.transform.DOScale (Vector3.zero,0.01f).OnComplete(WaitForDesolve);
	}

	void WaitForDesolve(){
		SecondPoint.transform.DOLocalMove (Vector3.zero,0.01f);
		EndPoint.transform.DOLocalMove (Vector3.zero,0.01f);
	}

	// Use this for initialization
	void Start () {
		
	}
}