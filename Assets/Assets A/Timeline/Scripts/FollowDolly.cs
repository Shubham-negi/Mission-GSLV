using UnityEngine;

namespace GslvMission
{
    /// <summary>Keeps this object (the UI) at the same place relative to the camera dolly the Timeline moves.</summary>
    public class FollowDolly : MonoBehaviour
    {
        public Transform dolly;
        Vector3 basePosition;

        void Awake()
        {
            basePosition = transform.position - (dolly != null ? dolly.position : Vector3.zero);
        }

        void LateUpdate()
        {
            if (dolly != null) transform.position = basePosition + dolly.position;
        }
    }
}
