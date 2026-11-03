/************************************************************************************

Copyright   :   Copyright 2015 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System.Collections;

public class TestMotion : MonoBehaviour
{
	private Vector3 _scale;

	private void Start()
	{
		_scale = transform.localScale;
	}

	// Update is called once per frame
	private void Update ()
	{
		transform.localScale = Mul( _scale , new Vector3
			( 1 + 0.2f * Mathf.Sin( Time.realtimeSinceStartup )
			 , 1
			 , 1 + 0.2f * Mathf.Cos( Time.realtimeSinceStartup )
			 ) );
	}

	
	private Vector3 Mul( Vector3 a , Vector3 b )
	{
		return new Vector3( a.x * b.x , a.y * b.y , a.z * b.z );
	}
}
