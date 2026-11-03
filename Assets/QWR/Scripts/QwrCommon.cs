/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace qwr
{
	/// <summary>
	/// Contains the valid range of texture depth values usable with Unity render textures.
	/// </summary>
	public enum TEXTURE_DEPTH
	{
		_0  =  0,
		_16 = 16,
		_24 = 24,
	}
	
	/// <summary>
	/// Matches the events in the native plugin.
	/// </summary>
	public enum PLUGIN_EVENT_TYPE
	{
		// PC
		BeginFrame = 0,
		EndFrame = 1,
		Initialize = 2,
		Destroy = 3,
		
		// Android
		InitRenderThread = 0,
		Pause = 1,
		Resume = 2,
		LeftEyeEndFrame = 3,
		RightEyeEndFrame = 4,
		TimeWarp = 5,
		PlatformUI = 6,
		PlatformUIConfirmQuit = 7,
		ResetVrModeParms = 8,
		PlatformUITutorial = 9,
		ShutdownRenderThread = 10,
	}

	public enum qwrcEYE
	{
		LEFT = 0 ,
		RIGHT = 1 ,
		BOTH_LEFT = 2 ,
		BOTH_RIGHT = 3 ,
		NUM = 2 ,
	}

	public enum qwrcOutputMode
	{
		NONE = -1,
		LEFT_EYE = 0,
		RIGHT_EYE = 1,
		BOTH_NORMAL = 2,
		BOTH_DISTORTION = 3,
	}

	public enum qwrcTwType
	{
		NONE = 0,
		DISTORTION = 1,
		// It is not recommanded to use the following two options for multilayer.
		// If you want to use these, please leave transparent blank at the edge.
		TW= 2,
		TW_DISTORTION= 3,
	}

	/// <summary>
	/// An affine transformation built from a Unity position and orientation.
	/// </summary>
	public struct Pose
	{
		/// <summary>
		/// The position.
		/// </summary>
		public Vector3 position;
		
		/// <summary>
		/// The orientation.
		/// </summary>
		public Quaternion orientation;
	}
	
	public enum qwrpGENDER
	{
		SECRET ,
		MALE ,
		FEMALE ,
		NUM ,
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct qwrVector2
	{
		public float x;
		public float y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct qwrProfile
	{
		public float interpupillary_distance;
		public qwrVector2 fov;
		public float neck_to_eye_height;
		public float eye_depth;
		public float player_height;
		public qwrpGENDER gender;
	};
	public enum RENDER_EVENT
	{
		Compose = 0,
		Resume = 1,
		Pause = 2,
		LEFT_EYE = 3,
		RIGHT_EYE = 4,
		Init = 5,
		Deinit = 6,
		Postnontransparent = 7,
		Posttransparent = 8,
        UpdatePose = 9,
	}
	
	public enum qwrcAPPLY
	{
		qwrcAPPLY_NONE ,
		qwrcAPPLY_DISTORTION ,
		qwrcAPPLY_TIMEWARP ,
		qwrcAPPLY_DISTORTION_TIMEWARP ,
		qwrcAPPLY_NUM ,
	}

	/// <summary>
	/// Selects a human eye.
	/// </summary>
	public enum qwrhMESSAGE
	{
		qwrhMESSAGE_OK ,
		qwrhMESSAGE_KEY ,
		qwrhMESSAGE_MOUSE_BUTTON ,
		qwrhMESSAGE_MOUSE_MOVE ,
		qwrhMESSAGE_RESIZE ,
		qwrhMESSAGE_DISPLAY_CHANGE ,
		qwrhMESSAGE_NUM ,
	};
	
	public enum qwruUsageMode {
    	QWRU_UM_DEFAULT=0,
    	QWRU_UM_SENSOR_ONLY=1,
    	QWRU_UM_EDITOR_MODE=2,
    	QWRU_UM_COUNT,
    };

	public enum qwruCONFIG
	{
		qwruCONFIG_FREEZE , //freeze time-warp? 0 not freezed, 1 freezed. time-warp is disabled when freezed.
		qwruCONFIG_INTERPUPILLARY , // interpupillary distance in millimeter.
		qwruCONFIG_RENDER_FOVX , // fov x when rendering, in degree
		qwruCONFIG_RENDER_FOVY , // fov y when rendering, in degree
		qwruCONFIG_COUNT ,
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct qwrRect
	{
		public float x;
		public float y;
		public float w;
		public float h;

		public qwrRect (Rect rect)
		{
			x = rect.x;
			y = rect.y;
			w = rect.width;
			h = rect.height;
		}
	};
	
	[StructLayout(LayoutKind.Sequential)]
	public struct qwrVector3
	{
		public float x;
		public float y;
		public float z;

        public qwrVector3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    }

	[StructLayout(LayoutKind.Sequential)]
	public struct qwrQuarterion
	{
		public float s;
		public float i;
		public float j;
		public float k;

        public qwrQuarterion(float _s, float _i, float _j, float _k)
        {
            s = _s;
            i = _i;
            j = _j;
            k = _k;
        }

        public Quaternion ToQuaternion()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			return new Quaternion( i , j , -k , -s );
#else
            return new Quaternion( -i , -j , -k , s );
#endif
		}
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct qwrTransform
	{
		public qwrQuarterion q;
		public qwrVector3 p;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct qwrSensorData
	{
		public qwrVector3 angular_velocity;
		public qwrVector3 linear_acceleration;
		public qwrVector3 magnetometer;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct qwrnDeviceInfo
	{
		public float ipd;
		public int resolution_x; // single eye
		public int resolution_y; // single eye
		public float fov_x;
		public float fov_y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct qwrnPrediction
	{
		public int key;
		public qwrQuarterion pose;
		public qwrVector3 position;
		public double time;
		double sampleTime;
	}

	public struct Common
	{
		public const string QwrDeviceGameObjectName = "_QwrDeviceGameObject";
        public const string LibQwr = "QwrUnity";
		#if UNITY_ANDROID && !UNITY_EDITOR
		public const int NUM_BUFFER = 3; // triple buffer
        #else
        public const int NUM_BUFFER = 1;

		#endif
	}
}
