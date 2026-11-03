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
    public class Sensor
    {
        /// <summary>
        /// Gets the pose of the given eye, predicted for the time when the current frame will scan out.
        /// </summary>

        public Pose GetEyePose
            (qwrcEYE eye
            , qwrQuarterion pose
            , qwrVector3 position // position should be left-handed (Unity coordinates)
            , float interpupillary_distance
            )
        {
            Quaternion rot = pose.ToQuaternion(); // Right-handed spiral, left-handed -> Left-handed spiral, left-handed (Unity coordinates)

            float eye_offset_x = 0.5f * interpupillary_distance;
            eye_offset_x = (eye == qwrcEYE.LEFT) ? -eye_offset_x : eye_offset_x;

            float neck_to_eye_height = 0.0f;
            float eye_depth = 0.0f; // PC: head model do in native sdk, Android: for compatible old game, do here as old apps
#if UNITY_ANDROID && !UNITY_EDITOR
            float eye_height = 1.6f;
            float player_height = eye_height * 1.067f; // eye should be at 15/16 of total height.
            neck_to_eye_height = player_height * 0.0625f * QwrManager.worldScale; //neck_to_eye_height should be 1/16 of total height.
            eye_depth = 0.0805f * QwrManager.worldScale;
#endif

            Vector3 neck_model = new Vector3(0.0f, neck_to_eye_height, eye_depth);
            Vector3 pos = rot * (new Vector3(eye_offset_x, 0.0f, 0.0f) + neck_model); // neck-pivot space
            pos -= neck_model; // what it does is converting pos to initially oriented center-eye space, making the initial position of center eye (0,0,0)

            Pose ret = new Pose();
            ret.position = pos + new Vector3(position.x, position.y, position.z);
            ret.orientation = rot;
            return ret;
        }

        /// <summary>
        /// Recenters the head pose.
        /// </summary>
        public void RecenterPose()
        {
			Composer.QwruResetPose();
        }

        /// <summary>
        /// Gets the head pose.
        /// </summary>
        /// <param name="predictionTime">The prediction time.</param>
        /// <returns></returns>
        public Pose GetHeadPose(double predictionTime = 0d)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            qwrQuarterion pose = Composer.QwruGetPredictedPose(predictionTime);
            qwrVector3 position = Composer.QwruGetPredictedPosition(predictionTime);
            #else
            qwrQuarterion pose = Composer.QwruGetPose();
            qwrVector3 position = Composer.QwruGetPosition();
            #endif
            return new Pose
            {
                position = new Vector3(position.x, position.y, position.z),
                orientation = pose.ToQuaternion(),
            };
        }

        public Vector3 acceleration
        {
            get
            {
                qwrSensorData SensorData = new qwrSensorData();
                Composer.QwruGetSensorData(ref SensorData);
                return new Vector3(SensorData.linear_acceleration.x, SensorData.linear_acceleration.y, SensorData.linear_acceleration.z);
            }
        }

        public Vector3 angularVelocity
        {
            get
            {
                qwrSensorData SensorData = new qwrSensorData();
                Composer.QwruGetSensorData(ref SensorData);
                return new Vector3(SensorData.angular_velocity.x, SensorData.angular_velocity.y, SensorData.angular_velocity.z);
            }
        }

        public float GetYaw()
        {
            string property_name = "get_yaw";
            IntPtr tempPtr = Marshal.StringToHGlobalAnsi(property_name);
            return Composer.QwruGetFloatValue(tempPtr, 0.0f);
        }

        /// <summary>
        /// Sets the yaw.
        /// </summary>
        /// <param name="angle">The angle.</param>
        public void SetYaw(float angle)
        {
            string property_name = "set_yaw";
            IntPtr tempPtr = Marshal.StringToHGlobalAnsi(property_name);
            Composer.QwruSetFloatValue(tempPtr, angle);
        }
    }
}
