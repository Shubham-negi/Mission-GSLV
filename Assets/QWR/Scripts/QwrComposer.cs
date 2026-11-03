/************************************************************************************

Copyright   :   Copyright 2015-2017 QWR Interactive Solutions Pvt. Ltd. All Rights reserved.

QWR Developer Website: http://developer.qwr.asia

************************************************************************************/
using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace qwr
{
    /// <summary>
    /// Communicates with native plugin functions that run on the rendering thread.
    /// </summary>
    public class Composer
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		public static void PostRender(RENDER_EVENT eventType, int timewarp_view_number)
		{
			IssueWithData(eventType, timewarp_view_number);
		}
		private static void IssueWithData(RENDER_EVENT eventType, int eventData)
		{
			// Encode and send-two-bytes of data
			GL.IssuePluginEvent(GetRenderEventFunc(), EncodeData((int)eventType, eventData, 0));
			
			// Encode and send remaining two-bytes of data
			GL.IssuePluginEvent(GetRenderEventFunc(), EncodeData((int)eventType, eventData, 1));
			
			// Explicit event that uses the data
			GL.IssuePluginEvent(GetRenderEventFunc(), EncodeType((int)eventType));
		}
		private const UInt32 IS_DATA_FLAG = 0x80000000;
		private const UInt32 DATA_POS_MASK = 0x40000000;
		private const int DATA_POS_SHIFT = 30;
		private const UInt32 EVENT_TYPE_MASK = 0x3E000000;
		private const int EVENT_TYPE_SHIFT = 25;
		private const UInt32 PAYLOAD_MASK = 0x0000FFFF;
		private const int PAYLOAD_SHIFT = 16;
		
		private static int EncodeType(int eventType)
		{
			return (int)((UInt32)eventType & ~IS_DATA_FLAG); // make sure upper bit is not set
		}
		
		private static int EncodeData(int eventId, int eventData, int pos)
		{
			UInt32 data = 0;
			data |= IS_DATA_FLAG;
			data |= (((UInt32)pos << DATA_POS_SHIFT) & DATA_POS_MASK);
			data |= (((UInt32)eventId << EVENT_TYPE_SHIFT) & EVENT_TYPE_MASK);
			data |= (((UInt32)eventData >> (pos * PAYLOAD_SHIFT)) & PAYLOAD_MASK);
			
			return (int)data;
		}
		
		private static int DecodeData(int eventData)
		{
			//bool hasData = (((UInt32)eventData & IS_DATA_FLAG) != 0);
			UInt32 pos = (((UInt32)eventData & DATA_POS_MASK) >> DATA_POS_SHIFT);
			//UInt32 eventId = (((UInt32)eventData & EVENT_TYPE_MASK) >> EVENT_TYPE_SHIFT);
			UInt32 payload = (((UInt32)eventData & PAYLOAD_MASK) << (PAYLOAD_SHIFT * (int)pos));
			
			return (int)payload;
		}
#endif
        public static void Resume()
        {
            IssuePluginEvent(RENDER_EVENT.Resume);
        }

        public static void Pause()
        {
            IssuePluginEvent(RENDER_EVENT.Pause);
        }
        private static bool _initialized = false;

        public static bool Init()
        {
            if (_initialized)
                return true;


            //qwruConfig( (int)qwruCONFIG.qwruCONFIG_INTERPUPILLARY , (int)( ipd * 1000.0f ) );
#if UNITY_ANDROID && UNITY_EDITOR
            bool ret = false;
            string path = System.Environment.GetEnvironmentVariable("QWR_PLATFORM_DIR");
            bool fileExist = System.IO.File.Exists(path + "/bin/QwrSDKService.exe");
            if(fileExist)
            {
                ret = QwruInit((int)qwruUsageMode.QWRU_UM_DEFAULT, IntPtr.Zero);
            }
#else
            bool ret = QwruInit((int)qwruUsageMode.QWRU_UM_DEFAULT, IntPtr.Zero);
#endif
            if (false == ret)
            {
                int err = QwruGetLastError();
                Debug.Log(err.ToString());
                return false;
            }

            _initialized = true;

            InitSvr();

#if UNITY_STANDALONE_WIN
            SetGameEngineType(0);// 0 unity engine
            SetGameEngineColorSpace(QualitySettings.activeColorSpace);
#endif

            return true;
        }

        public static void Uninit()
        {
            if (false == _initialized)
                return;

            QwruDeinit();

            _initialized = false;
        }

        public static bool SetTextures(IntPtr tex_ptr, qwrcEYE eye)
        {
            bool ret;
            ret = QwruSetTexture(tex_ptr, (int)eye, 3, // distortion time warp
                QwrManager.DeviceInfo.resolution_x, QwrManager.DeviceInfo.resolution_y);

            if (!ret)
                return false;
            return true;
        }


        public void Update()
        {
            //if (false == _initialized)
            //	return;
            //for( int i = 0 ; i < (int)qwrcEYE.NUM ; ++i )
            //	qwruRecordPose( i );
        }

        public void Compose()
        {
            if (false == _initialized)
                return;
            IssuePluginEvent(RENDER_EVENT.Compose);
        }
        UInt32 encodeEventId(byte id, UInt32 data)
        {
            UInt32 r = data;
            r = r << 8;

            r = r | (UInt32)(id & 0xff);

            r = r | 0x80000000;

            return r;
        }

		public qwrnPrediction Predict()
        {
			qwrnPrediction Prediction = new qwrnPrediction();
            IntPtr tempPtr = QwruPredict();
			if (tempPtr == IntPtr.Zero)
			{
				Prediction.key = -1;
			}
			else
			{
				Prediction = (qwrnPrediction)Marshal.PtrToStructure(tempPtr, typeof(qwrnPrediction));
			}
			return Prediction;
        }

        public void UpdatePose(UInt32 cacheIndex)
        {
            UInt32 ev = encodeEventId((byte)RENDER_EVENT.UpdatePose, cacheIndex);
            GL.IssuePluginEvent(GetRenderEventFunc(), (int)ev);
        }

        public static void UpdatePose(qwrQuarterion pose, qwrVector3 pos)
        {
            Composer.QwruUpdatePose(pose, pos);
        }

		public static void IssuePluginEvent( RENDER_EVENT evt )
		{
			if( false == _initialized )
				return;
			GL.IssuePluginEvent (GetRenderEventFunc(), (int)evt);
		}

		//qwr unity
		[DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool QwruInit
			( int mode,IntPtr userData);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwruGetDeviceInfo(ref qwrnDeviceInfo qwrnDeviceInfo);
		
		[DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
		public extern static void QwruDeinit();
		
		[DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool QwruSetTexture
			( IntPtr texture
			 , int eye
 			 , int mode
 			 , int width
 			 , int height
			 );
		[DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool QwruRecordPose(int eye);

		[DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
		public extern static qwrQuarterion QwruGetPose();
		
		[DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
		public extern static qwrVector3 QwruGetPosition();
		
		[DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool QwruGetSensorData(ref qwrSensorData sensorData);

		[DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
		public extern static int QwruGetLastError();

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static float QwruGetFloatValue(IntPtr property_name, float default_value);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwruSetFloatValue(IntPtr property_name, float value);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwruSetPtrValue(IntPtr property_name, IntPtr value);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr QwruGetPtrValue(IntPtr property_name, IntPtr default_value);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwruSetIntValue(IntPtr property_name, int value);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static int QwruGetIntValue(IntPtr property_name, int default_value);

		[DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool QwruSetTextureEx(int index, IntPtr texture, int eye, int mode, qwrRect viewport);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwruResetPose();

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool QwruUpdatePose(qwrQuarterion pose, qwrVector3 position);


        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static qwrQuarterion QwruGetPredictedPose(double timeInSeconds);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static qwrVector3 QwruGetPredictedPosition(double timeInSeconds);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static double QwruGetPredictedDisplayTime(int pipelineDepth);

        [DllImport(Common.LibQwr, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr QwruPredict();

        [DllImport(Common.LibQwr)]
        private static extern IntPtr GetRenderEventFunc();

        static void SetSvrColorSpace(ColorSpace value)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            int colorSpace = (value == ColorSpace.Linear) ? 1 : 0;
            IntPtr tempPtr = Marshal.StringToHGlobalAnsi("color_space");
            Composer.QwruSetIntValue(tempPtr, colorSpace);
            Marshal.FreeHGlobal(tempPtr);
#endif
        }

        static void SetSvrWarpFrameMultiChecking(bool value)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            IntPtr tempPtr = Marshal.StringToHGlobalAnsi("warp_frame_multi_checking");
            Composer.QwruSetIntValue(tempPtr, value ? 1 : 0);
            Marshal.FreeHGlobal(tempPtr);
#endif
        }

        static void InitSvr()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            SetSvrColorSpace(QualitySettings.activeColorSpace);
            SetSvrWarpFrameMultiChecking(true);
#endif
        }

#if UNITY_STANDALONE_WIN
        static void SetGameEngineType(int value)
        {
            IntPtr ptr = Marshal.StringToHGlobalAnsi("game_engine_type");
            Composer.QwruSetIntValue(ptr, value);
            Marshal.FreeHGlobal(ptr);
        }

        static void SetGameEngineColorSpace(ColorSpace colorSpace)
        {
            IntPtr ptr = Marshal.StringToHGlobalAnsi("game_engine_color_space");
            int value = -1;
            switch (colorSpace)
            {
                case ColorSpace.Gamma:
                    value = 0;
                    break;
                case ColorSpace.Linear:
                    value = 1;
                    break;
            }
            Composer.QwruSetIntValue(ptr, value);
            Marshal.FreeHGlobal(ptr);
        }
#endif
    }
}
