using UnityEngine;

namespace GslvSpace
{
    /// <summary>
    /// Draws a cubic Bezier curve through four control points (move them in the Scene view).
    /// Used for the rocket's flight path and for the transfer-to-final-orbit arrow.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class BezierPath : MonoBehaviour
    {
        public Transform p0;
        public Transform p1;
        public Transform p2;
        public Transform p3;

        [Range(4, 256)]
        public int segments = 64;

        [Header("Arrow head")]
        public bool arrowHead;

        [Tooltip("Share of the line used by the arrow head.")]
        [Range(0.05f, 0.5f)]
        public float arrowHeadLength = 0.18f;

        [Tooltip("How much wider the arrow head's base is than the line.")]
        public float arrowHeadWidth = 3.5f;

        LineRenderer line;

        public bool IsValid { get { return p0 != null && p1 != null && p2 != null && p3 != null; } }

        /// <summary>World-space point at t in [0, 1].</summary>
        public Vector3 GetPoint(float t)
        {
            t = Mathf.Clamp01(t);
            float u = 1f - t;
            return u * u * u * p0.position
                + 3f * u * u * t * p1.position
                + 3f * u * t * t * p2.position
                + t * t * t * p3.position;
        }

        /// <summary>World-space unit direction of travel at t in [0, 1].</summary>
        public Vector3 GetTangent(float t)
        {
            t = Mathf.Clamp01(t);
            float u = 1f - t;
            Vector3 d = 3f * u * u * (p1.position - p0.position)
                + 6f * u * t * (p2.position - p1.position)
                + 3f * t * t * (p3.position - p2.position);
            return d.normalized;
        }

        void OnEnable()
        {
            Rebuild();
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            // Defer: changing renderer data inside OnValidate can trigger editor warnings.
            UnityEditor.EditorApplication.delayCall += () => { if (this != null) Rebuild(); };
#endif
        }

        void LateUpdate()
        {
            if (!IsValid) return;
            if (p0.hasChanged || p1.hasChanged || p2.hasChanged || p3.hasChanged || transform.hasChanged)
            {
                Rebuild();
                p0.hasChanged = p1.hasChanged = p2.hasChanged = p3.hasChanged = false;
                transform.hasChanged = false;
            }
        }

        public void Rebuild()
        {
            if (line == null) line = GetComponent<LineRenderer>();
            if (line == null) return;
            if (!IsValid)
            {
                line.positionCount = 0;
                return;
            }

            // Local space, so the line moves with its parent without a rebuild.
            line.useWorldSpace = false;
            line.loop = false;
            line.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
                line.SetPosition(i, transform.InverseTransformPoint(GetPoint(i / (float)segments)));

            line.widthCurve = arrowHead ? ArrowCurve() : AnimationCurve.Constant(0f, 1f, 1f);
        }

        // Straight-edged width profile: constant shaft, then a step out to the head's base, tapering to a point.
        AnimationCurve ArrowCurve()
        {
            float neck = 1f - arrowHeadLength;
            float[] times = { 0f, neck, neck + 0.001f, 1f };
            float[] values = { 1f, 1f, arrowHeadWidth, 0f };
            var keys = new Keyframe[times.Length];
            for (int i = 0; i < times.Length; i++)
            {
                float inSlope = i > 0 ? (values[i] - values[i - 1]) / (times[i] - times[i - 1]) : 0f;
                float outSlope = i < times.Length - 1 ? (values[i + 1] - values[i]) / (times[i + 1] - times[i]) : 0f;
                keys[i] = new Keyframe(times[i], values[i], inSlope, outSlope);
            }
            return new AnimationCurve(keys);
        }
    }
}
