using UnityEngine;
using UnityEngine.Playables;

namespace GslvMission
{
    /// <summary>
    /// Keeps the atmosphere's layer panels and the altitude readout beside the rocket during the climb: turns them around the
    /// learner to the bearing the chase camera's path gives for that moment, so the rocket and the readouts stay in one view.
    /// The curve is worked out by the editor from the flight and the camera (Timeline time -> degrees right of the learner's
    /// forward), which keeps the panels' motion smooth and in step with the Timeline.
    /// </summary>
    public class FlightReadouts : MonoBehaviour
    {
        public PlayableDirector director;
        public AnimationCurve bearing = AnimationCurve.Constant(0f, 1f, -28f);
        [Tooltip("Added to the curve: negative moves the panels further left of the rocket, positive closer to it.")]
        public float offsetDegrees;

        void LateUpdate()
        {
            if (director != null) transform.localRotation = Quaternion.Euler(0f, bearing.Evaluate((float)director.time) + offsetDegrees, 0f);
        }
    }
}
