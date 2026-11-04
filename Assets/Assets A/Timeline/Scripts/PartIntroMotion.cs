using System;
using UnityEngine;
using UnityEngine.Playables;

namespace GslvMission
{
    /// <summary>
    /// While the voice-over introduces a rocket part, the part comes out of the stack to the middle of the learner's view, close
    /// enough to look big and be the one thing to look at, then goes back. The camera stays where it is. A part keeps its real size
    /// and looks bigger because it is nearer, except one with a 'scale' (the satellite, which is too small to see otherwise).
    /// Driven by Timeline time, so a gate or a pause holds it. It runs after the Timeline has posed the rocket and puts each part
    /// back on its exact starting position at the end of its window.
    /// </summary>
    [DefaultExecutionOrder(100)]   // after the Timeline has posed the rocket
    public class PartIntroMotion : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public string name;
            public Transform[] parts;
            public Vector3 centre;       // the middle of the part's meshes on the pad, in world space
            public Vector3 size;         // their overall size
            public bool riseFirst;       // leaves upwards before coming forwards (the boosters stand inside the pad's pedestals)
            public double start, end;    // Timeline times: leaves its place at 'start', back home at 'end'
            public float scale = 1f;     // how much larger than life it is on arrival
            [NonSerialized] public Vector3[] rest;
            [NonSerialized] public Vector3[] restScale;
            [NonSerialized] public Vector3[] toCentre;
            [NonSerialized] public bool moved;
        }

        public PlayableDirector director;
        [Tooltip("The learner's camera rig: the parts come to the middle of its view.")]
        public Transform viewpoint;
        [Tooltip("How tall a part looks when it arrives, in degrees of the view. Larger = closer and bigger.")]
        public float viewAngle = 32f;
        [Tooltip("A part never comes closer than this, in metres (the captions float 8 m away).")]
        public float minDistance = 11f;
        [Tooltip("How far above eye level the middle of the part sits, in degrees: the bottom captions start about 14.6 degrees below eye level, and a part's lower end stays above them. The boosters also need 2.2 m of rise from the pad to clear its pedestals.")]
        public float focusAngle = 4.5f;
        [Tooltip("The highest ground between the rocket and the learner: a part stops above this.")]
        public float groundY;
        public float groundClearance = 0.5f;
        [Tooltip("Seconds to come forward, and again to go back.")]
        public float moveSeconds = 2.2f;
        public AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Tooltip("Share of the move spent going straight out (or up) before the height changes too, so a stage clears the stages above and below it.")]
        [Range(0f, 0.6f)] public float clearShare = 0.4f;
        public Entry[] entries;

        void Awake()
        {
            if (entries == null) return;
            foreach (var e in entries)
            {
                e.rest = new Vector3[e.parts.Length];
                e.restScale = new Vector3[e.parts.Length];
                e.toCentre = new Vector3[e.parts.Length];
                for (int i = 0; i < e.parts.Length; i++)
                    if (e.parts[i] != null)
                    {
                        e.rest[i] = e.parts[i].localPosition;
                        e.restScale[i] = e.parts[i].localScale;
                        e.toCentre[i] = e.centre - e.parts[i].position;
                    }
            }
        }

        void LateUpdate()
        {
            if (director == null || viewpoint == null || entries == null) return;
            float t = (float)director.time;
            Vector3 ahead = Vector3.ProjectOnPlane(viewpoint.forward, Vector3.up).normalized;
            foreach (var e in entries)
            {
                float k = Mathf.Min(Mathf.InverseLerp((float)e.start, (float)e.start + moveSeconds, t), 1f - Mathf.InverseLerp((float)e.end - moveSeconds, (float)e.end, t));
                if (k <= 0f)
                {
                    if (e.moved) Place(e, Vector3.zero, 0f);
                    e.moved = false;
                    continue;
                }
                Vector3 move = Focus(e, viewpoint.position, ahead) - e.centre;
                float leads = easing.Evaluate(Mathf.Clamp01(k / (1f - clearShare)));
                float lags = easing.Evaluate(Mathf.Clamp01((k - clearShare) / (1f - clearShare)));
                float along = e.riseFirst ? lags : leads, up = e.riseFirst ? leads : lags;
                Place(e, new Vector3(move.x * along, move.y * up, move.z * along), along);
                e.moved = true;
            }
        }

        /// <summary>Where the middle of a part arrives, for an eye at 'eye' looking along 'ahead'.</summary>
        public Vector3 Focus(Entry e, Vector3 eye, Vector3 ahead)
        {
            float tall = e.size.y * e.scale;
            float distance = Mathf.Max(minDistance, 0.5f * tall / Mathf.Tan(0.5f * viewAngle * Mathf.Deg2Rad));
            float height = Mathf.Max(eye.y + distance * Mathf.Tan(focusAngle * Mathf.Deg2Rad), groundY + groundClearance + 0.5f * tall);
            Vector3 p = eye + ahead * distance;
            p.y = height;
            return p;
        }

        static void Place(Entry e, Vector3 worldOffset, float growth)
        {
            float s = Mathf.Lerp(1f, e.scale, growth);
            for (int i = 0; i < e.parts.Length; i++)
            {
                if (e.parts[i] == null) continue;
                // grows around its middle, not around its pivot, so the middle still arrives where Focus says
                e.parts[i].localPosition = e.rest[i] + e.parts[i].parent.InverseTransformVector(worldOffset - e.toCentre[i] * (s - 1f));
                if (e.scale != 1f) e.parts[i].localScale = e.restScale[i] * s;
            }
        }
    }
}
