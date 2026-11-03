using UnityEngine;

namespace GslvSpace
{
    /// <summary>
    /// Draws an orbit (ellipse, or circle when periapsis equals apoapsis) with this object's
    /// local origin at one focus, so parent it to the Earth's centre. The orbit lies in the
    /// local XZ plane and true anomaly 0 points along local +X (the periapsis).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class OrbitPath : MonoBehaviour
    {
        [Tooltip("Closest distance from the focus (Earth centre), in local units.")]
        public float periapsis = 1.15f;

        [Tooltip("Farthest distance from the focus. Set equal to periapsis for a circular orbit.")]
        public float apoapsis = 2.4f;

        [Range(16, 512)]
        public int segments = 192;

        [Tooltip("Draw only part of the orbit: start and end true anomaly in degrees (0 = periapsis). 0 to 360 draws the full orbit.")]
        public float arcStart = 0f;
        public float arcEnd = 360f;

        LineRenderer line;

        public float SemiMajorAxis { get { return (periapsis + apoapsis) * 0.5f; } }

        public float Eccentricity
        {
            get { return (apoapsis - periapsis) / Mathf.Max(0.0001f, apoapsis + periapsis); }
        }

        /// <summary>Point on the orbit in local space at a true anomaly in degrees.</summary>
        public Vector3 GetLocalPoint(float trueAnomalyDeg)
        {
            float e = Eccentricity;
            float semiLatusRectum = SemiMajorAxis * (1f - e * e);
            float nu = trueAnomalyDeg * Mathf.Deg2Rad;
            float r = semiLatusRectum / (1f + e * Mathf.Cos(nu));
            return new Vector3(Mathf.Cos(nu) * r, 0f, Mathf.Sin(nu) * r);
        }

        public Vector3 GetWorldPoint(float trueAnomalyDeg)
        {
            return transform.TransformPoint(GetLocalPoint(trueAnomalyDeg));
        }

        /// <summary>Unit direction of travel in world space at a true anomaly in degrees.</summary>
        public Vector3 GetWorldTangent(float trueAnomalyDeg)
        {
            Vector3 a = GetLocalPoint(trueAnomalyDeg - 0.5f);
            Vector3 b = GetLocalPoint(trueAnomalyDeg + 0.5f);
            return transform.TransformDirection(b - a).normalized;
        }

        void OnEnable()
        {
            Rebuild();
        }

        void OnValidate()
        {
            periapsis = Mathf.Max(0.01f, periapsis);
            apoapsis = Mathf.Max(apoapsis, periapsis);
#if UNITY_EDITOR
            // Defer: changing renderer data inside OnValidate can trigger editor warnings.
            UnityEditor.EditorApplication.delayCall += () => { if (this != null) Rebuild(); };
#endif
        }

        public void Rebuild()
        {
            if (line == null) line = GetComponent<LineRenderer>();
            if (line == null) return;

            line.useWorldSpace = false;
            float span = arcEnd - arcStart;
            if (span <= 0f)
            {
                line.positionCount = 0;
                return;
            }

            bool full = span >= 360f;
            int count = full ? segments : Mathf.Max(2, Mathf.CeilToInt(segments * span / 360f) + 1);
            line.loop = full;
            line.positionCount = count;
            for (int i = 0; i < count; i++)
            {
                float t = full ? i / (float)count : i / (float)(count - 1);
                line.SetPosition(i, GetLocalPoint(arcStart + t * (full ? 360f : span)));
            }
        }
    }
}
