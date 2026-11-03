/************************************************************************************

Copyright   :   Copyright 2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System.Collections;

namespace qwr
{
	[RequireComponent(typeof(Collider))]
	public class Teleport : MonoBehaviour
	{
		private Vector3 startingPosition;

		public Material inactiveMaterial;
		public Material gazedAtMaterial;

		void Start()
		{
			startingPosition = transform.localPosition;
			SetGazedAt(false);
		}

		void LateUpdate()
		{
		}

		public void SetGazedAt(bool gazedAt)
		{
			if (inactiveMaterial != null && gazedAtMaterial != null)
			{
				GetComponent<Renderer>().material = gazedAt ? gazedAtMaterial : inactiveMaterial;
				return;
			}
			GetComponent<Renderer>().material.color = gazedAt ? Color.green : Color.red;
		}

		public void SetGazeTrigger ()
		{
			GetComponent<ParticleSystem>().Play();
		}

		public void Reset()
		{
			transform.localPosition = startingPosition;
		}

		#region IQwrGazeResponder implementation

		/// Called when the user is looking on a GameObject with this script,
		/// as long as it is set to an appropriate layer (see QwrGaze).
		public void OnGazeEnter()
		{
			Debug.Log("Cursor is entered into cube.");
			SetGazedAt(true);
		}

		/// Called when the user stops looking on the GameObject, after OnGazeEnter
		/// was already called.
		public void OnGazeExit()
		{
			Debug.Log("Cursor is exited from cube.");
			SetGazedAt(false);
		}

		/// Called when the viewer's trigger is used, between OnGazeEnter and OnPointerExit.
		public void OnGazeTrigger()
		{
			Debug.Log("Cursor is Triggered above cube.");
			SetGazeTrigger();
		}

		#endregion
	}
}

