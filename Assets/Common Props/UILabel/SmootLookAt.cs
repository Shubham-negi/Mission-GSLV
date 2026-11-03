using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmootLookAt : MonoBehaviour {
	public Transform target;

	void Update () {
		transform.LookAt(target);
	}
}
