using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour {
	float x;

	// Use this for initialization
	void Start () {
		x = 0;
		InvokeRepeating ("RotateObj",0f,0.02f);
	}
	

	void RotateObj () {
		x -= 1f;
		gameObject.transform.localEulerAngles = new Vector3 (0,x,0);
	}

    public void ICancle()
    {
        CancelInvoke();
    }
}
