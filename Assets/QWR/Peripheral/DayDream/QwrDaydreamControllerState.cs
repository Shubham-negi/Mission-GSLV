/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System;

namespace qwr
{
    /// <summary>
    /// Internal representation of the controller's current state.
	/// This representation is used by controller providers to represent the controller's state.
	///
	/// The fields in this class have identical meanings to their correspondents in the GVR C API,
	/// so they are not redundantly documented here.
    /// </summary>
    class QwrDaydreamControllerState
	{
		internal QwrConnectionState connectionState = QwrConnectionState.Disconnected;
		internal QwrControllerApiStatus apiStatus = QwrControllerApiStatus.Unavailable;
		internal Quaternion orientation = Quaternion.identity;
		internal Vector3 gyro = Vector3.zero;
		internal Vector3 accel = Vector3.zero;
		internal bool isTouching = false;
		internal Vector2 touchPos = Vector2.zero;
		internal bool touchDown = false;
		internal bool touchUp = false;
		internal bool recentering = false;
		internal bool recentered = false;
		internal bool recenterBegin = false;
		internal bool recenterCancel = false;

		internal bool clickButtonState = false;
		internal bool clickButtonDown = false;
		internal bool clickButtonUp = false;

		internal bool appButtonState = false;
		internal bool appButtonDown = false;
		internal bool appButtonUp = false;

        internal bool triggerButtonState = false;
        internal bool triggerButtonDown = false;
        internal bool triggerButtonUp = false;

		internal bool homeButtonState = false;
		internal bool homeButtonDown = false;
		internal bool homeButtonUp = false;

		//internal bool volumeUpButtonState = false;
		//internal bool volumeUpButtonDown = false;
		//internal bool volumeUpButtonUp = false;

		//internal bool volumeDownButtonState = false;
		//internal bool volumeDownButtonDown = false;
		//internal bool volumeDownButtonUp = false;

		internal string errorDetails = "";

        /// <summary>
        ///Indicates whether or not a headset recenter was requested.
		// This is up to the ControllerProvider implementation to decide.
        /// </summary>
        internal bool headsetRecenterRequested = false;
		internal int batteryPower = 100;


        public void CopyFrom(QwrDaydreamControllerState other)
		{
			connectionState = other.connectionState;
			apiStatus = other.apiStatus;
			orientation = other.orientation;
			gyro = other.gyro;
			accel = other.accel;
			isTouching = other.isTouching;
			touchPos = other.touchPos;
			touchDown = other.touchDown;
			touchUp = other.touchUp;
			recentering = other.recentering;
			recentered = other.recentered;
			clickButtonState = other.clickButtonState;
			clickButtonDown = other.clickButtonDown;
			clickButtonUp = other.clickButtonUp;
			appButtonState = other.appButtonState;
			appButtonDown = other.appButtonDown;
			appButtonUp = other.appButtonUp;
            triggerButtonState = other.triggerButtonState;
            triggerButtonDown = other.triggerButtonDown;
            triggerButtonUp = other.triggerButtonUp;
			//volumeUpButtonState = other.volumeUpButtonState;
			//volumeUpButtonDown = other.volumeUpButtonDown;
			//volumeUpButtonUp = other.volumeUpButtonUp;
			//volumeDownButtonState = other.volumeDownButtonState;
			//volumeDownButtonDown = other.volumeDownButtonDown;
			//volumeDownButtonUp = other.volumeDownButtonUp;
			errorDetails = other.errorDetails;
			headsetRecenterRequested = other.headsetRecenterRequested;
		}

        /// <summary>
        /// Resets the transient state (the state variables that represent events, and which are true
        /// for only one frame).
        /// </summary>
        public void ClearTransientState()
		{
			touchDown = false;
			touchUp = false;
			recentered = false;
			clickButtonDown = false;
			clickButtonUp = false;
			appButtonDown = false;
			appButtonUp = false;
            triggerButtonState = false;
            triggerButtonDown = false;
            triggerButtonUp = false;
			//volumeUpButtonState = false;
			//volumeUpButtonDown = false;
			//volumeUpButtonUp = false;
			//volumeDownButtonState = false;
			//volumeDownButtonDown = false;
			//volumeDownButtonUp = false;
			headsetRecenterRequested = false;
		}
	}
}

