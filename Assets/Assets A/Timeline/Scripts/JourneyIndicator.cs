using GslvSpace;
using UnityEngine;
using UnityEngine.Playables;

namespace GslvMission
{
    /// <summary>
    /// The journey from India to the transfer orbit, on the orbit map. A small golden rocket flies along the flight path, the path
    /// lights up behind it, small arrows run ahead of it to show the way, and a ring and the transfer orbit's glow mark its arrival,
    /// where it goes: the rest of the mission belongs to the upper stage and the satellite.
    /// Everything is worked out from Timeline time, so a pause holds it and nothing is left over. It sits under the Flight Path, so
    /// the Timeline shows and hides all of it together.
    /// </summary>
    [DefaultExecutionOrder(100)]   // after the Timeline
    public class JourneyIndicator : MonoBehaviour
    {
        public PlayableDirector director;
        public BezierPath path;
        [Tooltip("The learner's camera rig: arrows and the rocket turn to read side-on from here.")]
        public Transform viewpoint;
        [Tooltip("Timeline time when the rocket leaves India.")]
        public double launchTime;
        [Tooltip("Timeline time when it reaches the transfer orbit.")]
        public double arrivalTime;

        public Transform rocket;
        public LineRenderer litLine;
        public SpriteRenderer[] arrows;
        public SpriteRenderer arrivalRing;
        public Transform[] faceViewer;

        [Tooltip("Half the small rocket's length, in world units.")]
        public float rocketLead = 1.6f;
        [Tooltip("Shares of the path per second that the arrows run.")]
        public float arrowFlow = 0.3f;
        [Tooltip("The arrows' size in world units.")]
        public float arrowSize = 1.1f;
        public Color arrowColor = new Color(0.55f, 0.85f, 1f, 0.9f);
        [Tooltip("How far the arrival ring grows, in world units.")]
        public float ringSize = 5f;
        public Color ringColor = new Color(1f, 0.82f, 0.4f, 0.9f);
        public float ringSeconds = 1.4f;
        [Tooltip("Seconds the rocket takes to go once it has arrived.")]
        public float leaveSeconds = 0.5f;
        [Tooltip("Seconds for the arrows to go once it has arrived.")]
        public float arrowsOutSeconds = 0.6f;

        const int Samples = 64;
        Vector3[] points;       // along the path, in this object's space
        float[] travelled;      // share of the path's length up to each point
        Vector3 rocketScale;
        float spriteArrow, spriteRing;
        bool ready;

        void Prepare()
        {
            if (ready || path == null || !path.IsValid) return;
            points = new Vector3[Samples + 1];
            travelled = new float[Samples + 1];
            float length = 0f;
            for (int i = 0; i <= Samples; i++)
            {
                points[i] = transform.InverseTransformPoint(path.GetPoint(i / (float)Samples));
                if (i > 0) length += Vector3.Distance(points[i - 1], points[i]);
                travelled[i] = length;
            }
            for (int i = 0; i <= Samples; i++) travelled[i] /= length;
            if (rocket != null) rocketScale = rocket.localScale;
            spriteArrow = arrows != null && arrows.Length > 0 && arrows[0].sprite != null ? arrows[0].sprite.bounds.size.y : 1f;
            spriteRing = arrivalRing != null && arrivalRing.sprite != null ? arrivalRing.sprite.bounds.size.y : 1f;
            ready = true;
        }

        void LateUpdate()
        {
            if (director != null) Pose(director.time);
        }

        /// <summary>Puts everything where it belongs at this Timeline time.</summary>
        public void Pose(double time)
        {
            Prepare();
            if (!ready) return;
            float flown = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp((float)launchTime, (float)arrivalTime, (float)time));
            float afterArrival = (float)(time - arrivalTime);
            Vector3 eye = viewpoint != null ? viewpoint.position : transform.position - transform.forward;

            PoseLitLine(flown);
            PoseRocket(flown, afterArrival, eye);
            PoseArrows(flown, (float)(time - launchTime), afterArrival, eye);
            PoseRing(afterArrival, eye);
            if (faceViewer != null)
                foreach (var t in faceViewer)
                    if (t != null) t.rotation = Quaternion.LookRotation(t.position - eye, transform.up);
        }

        void PoseLitLine(float flown)
        {
            if (litLine == null) return;
            int passed = 0;
            while (passed < Samples && travelled[passed + 1] <= flown) passed++;
            litLine.positionCount = passed + 2;
            for (int i = 0; i <= passed; i++) litLine.SetPosition(i, points[i]);
            litLine.SetPosition(passed + 1, PointAt(flown));
        }

        void PoseRocket(float flown, float afterArrival, Vector3 eye)
        {
            if (rocket == null) return;
            float shown = 1f - Mathf.Clamp01(afterArrival / Mathf.Max(0.01f, leaveSeconds));
            if (rocket.gameObject.activeSelf != shown > 0f) rocket.gameObject.SetActive(shown > 0f);
            if (shown <= 0f) return;
            Vector3 at = transform.TransformPoint(PointAt(flown));
            Vector3 way = OnScreenDirection(flown, at, eye);
            at += way * (rocketLead * (1f - flown));   // starts with its tail on the launch point, not its middle
            rocket.position = at;
            rocket.rotation = Quaternion.LookRotation(eye - at, way);
            rocket.localScale = rocketScale * shown;
        }

        void PoseArrows(float flown, float sinceLaunch, float afterArrival, Vector3 eye)
        {
            if (arrows == null) return;
            float window = 1f - Mathf.Clamp01(afterArrival / Mathf.Max(0.01f, arrowsOutSeconds));
            for (int i = 0; i < arrows.Length; i++)
            {
                var a = arrows[i];
                if (a == null) continue;
                float along = Mathf.Repeat(sinceLaunch * arrowFlow + i / (float)arrows.Length, 1f);
                // only on the part of the path still ahead of the rocket, coming and going softly at the ends
                float seen = window * Mathf.Clamp01((along - flown - 0.04f) / 0.08f) * Mathf.Clamp01((1f - along) / 0.08f);
                if (a.enabled != seen > 0f) a.enabled = seen > 0f;
                if (seen <= 0f) continue;
                Vector3 at = transform.TransformPoint(PointAt(along));
                a.transform.position = at;
                // the sprite's right is its point
                Vector3 away = (at - eye).normalized;
                a.transform.rotation = Quaternion.LookRotation(away, Vector3.Cross(away, OnScreenDirection(along, at, eye)));
                a.transform.localScale = Vector3.one * (arrowSize / spriteArrow / Mathf.Max(0.0001f, transform.lossyScale.x));
                var c = arrowColor;
                c.a *= seen;
                a.color = c;
            }
        }

        void PoseRing(float afterArrival, Vector3 eye)
        {
            if (arrivalRing == null) return;
            float k = afterArrival / Mathf.Max(0.01f, ringSeconds);
            bool on = k > 0f && k < 1f;
            if (arrivalRing.enabled != on) arrivalRing.enabled = on;
            if (!on) return;
            Vector3 at = transform.TransformPoint(points[Samples]);
            arrivalRing.transform.position = at;
            arrivalRing.transform.rotation = Quaternion.LookRotation(at - eye, transform.up);
            arrivalRing.transform.localScale = Vector3.one * (Mathf.Lerp(0.2f, 1f, 1f - (1f - k) * (1f - k)) * ringSize / spriteRing / Mathf.Max(0.0001f, transform.lossyScale.x));
            var c = ringColor;
            c.a *= 1f - k;
            arrivalRing.color = c;
        }

        // The point at this share of the path's length, in this object's space.
        Vector3 PointAt(float share)
        {
            int lo = 0, hi = Samples;
            while (hi - lo > 1)
            {
                int mid = (lo + hi) / 2;
                if (travelled[mid] <= share) lo = mid;
                else hi = mid;
            }
            float span = travelled[hi] - travelled[lo];
            return Vector3.Lerp(points[lo], points[hi], span > 0f ? (share - travelled[lo]) / span : 0f);
        }

        // The way along the path as the learner sees it. Looks a little ahead, because the path starts by coming straight at them.
        Vector3 OnScreenDirection(float share, Vector3 at, Vector3 eye)
        {
            Vector3 from = transform.TransformPoint(PointAt(Mathf.Max(0f, Mathf.Min(share, 0.94f))));
            Vector3 to = transform.TransformPoint(PointAt(Mathf.Min(share, 0.94f) + 0.06f));
            Vector3 d = Vector3.ProjectOnPlane(to - from, at - eye);
            return d.sqrMagnitude > 0.000001f ? d.normalized : transform.up;
        }
    }
}
