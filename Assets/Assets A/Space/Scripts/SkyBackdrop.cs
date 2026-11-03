using UnityEngine;

namespace GslvSpace
{
    /// <summary>
    /// Earth seen from the rocket during the flight. Keeps a fixed offset below the learner's eyes every frame,
    /// so it never gets nearer however far the rocket flies (it reads as infinitely far away, like the sky).
    /// The Timeline animates 'height' to make Earth fall away as the rocket climbs.
    /// </summary>
    [DefaultExecutionOrder(-40)]   // after MissionCamera has moved the learner
    public class SkyBackdrop : MonoBehaviour
    {
        [Tooltip("Radius of the Earth sphere child, in metres.")]
        public float radius = 600f;
        [Tooltip("How far below the learner's eyes Earth's surface is.")]
        public float height = 60f;

        void LateUpdate()
        {
            Transform eye = null;
            var rig = qwr.QwrCameraRig._instance;
            if (rig != null && rig._center_eye != null) eye = rig._center_eye.transform;
            if (eye == null && Camera.main != null) eye = Camera.main.transform;
            if (eye != null) transform.position = eye.position + Vector3.down * (radius + height);
        }
    }
}
