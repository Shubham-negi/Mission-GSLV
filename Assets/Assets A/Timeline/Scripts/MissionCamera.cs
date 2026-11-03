using GslvSpace;
using UnityEngine;

namespace GslvMission
{
    /// <summary>
    /// Lives on the Camera Dolly. Most of the time the Timeline moves the dolly itself (the stage tour and the
    /// chase camera that follows the flight). While the orbit map is shown, the Timeline moves an anchor inside
    /// the space set instead (the set is placed in front of the learner only once space is shown), and the dolly
    /// follows that anchor here. The anchor's host is switched on only for the map.
    /// </summary>
    [DefaultExecutionOrder(-50)]   // before FollowDolly, so the UI never lags a frame behind
    public class MissionCamera : MonoBehaviour
    {
        public Transform rig;
        public SpaceEnvironmentSwitcher space;
        [Tooltip("Animated by the Timeline in the space set's own frame; the learner's eyes go here while in space.")]
        public Transform spaceAnchor;

        Vector3 rigOffset;

        void Awake()
        {
            if (rig != null) rigOffset = rig.position - transform.position;
        }

        void LateUpdate()
        {
            if (space == null || !space.InSpace || spaceAnchor == null || !spaceAnchor.gameObject.activeInHierarchy) return;
            transform.position = spaceAnchor.position - rigOffset;
        }
    }
}
