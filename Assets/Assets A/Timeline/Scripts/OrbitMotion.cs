using GslvSpace;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace GslvMission
{
    /// <summary>
    /// The orbit map's satellite after its release: from 'startTime' on the Timeline it coasts along the transfer orbit up to the
    /// apogee (fast near Earth, slowing as it climbs, timed by Kepler's equation), then circles on the final orbit for good,
    /// turning so the same face looks at Earth. It runs on its own clock, so it keeps orbiting while a quiz question holds the Timeline.
    /// On reaching the apogee the transfer orbit's line and label fade away, leaving only the final orbit. The satellite is a child
    /// of the transfer orbit's object, so that object stays on; only its line and label go.
    /// </summary>
    [DefaultExecutionOrder(100)]   // after the Timeline has posed the satellite
    public class OrbitMotion : MonoBehaviour
    {
        public PlayableDirector director;
        public double startTime;
        public OrbitPath transfer;
        public OrbitPath finalOrbit;
        [Tooltip("Seconds from the release point to the apogee.")]
        public float transferSeconds = 8f;
        [Tooltip("Seconds per lap of the final orbit.")]
        public float orbitPeriod = 40f;
        public LineRenderer transferLine;
        public TMP_Text transferLabel;
        [Tooltip("Seconds for the transfer orbit to fade once the satellite reaches the apogee.")]
        public float transferFadeSeconds = 1f;

        Quaternion restRotation;
        Vector3 startRadial;
        float clock = -1f, startMean, lineWidth, transferShown = 1f;

        void Awake()
        {
            restRotation = transform.localRotation;
            if (transferLine != null) lineWidth = transferLine.widthMultiplier;
        }

        // 1 = the transfer orbit's line and label fully there, 0 = gone
        void ShowTransfer(float shown)
        {
            if (Mathf.Approximately(shown, transferShown)) return;
            transferShown = shown;
            if (transferLine != null)
            {
                transferLine.widthMultiplier = lineWidth * shown;
                transferLine.enabled = shown > 0f;
            }
            if (transferLabel != null) transferLabel.alpha = shown;
        }

        void LateUpdate()
        {
            if (director == null || transfer == null || finalOrbit == null) return;
            if (director.time < startTime) { clock = -1f; ShowTransfer(1f); return; }
            float e = transfer.Eccentricity;
            Vector3 centre = transfer.transform.position, axis = transfer.transform.up;
            if (clock < 0f)
            {
                clock = 0f;
                startMean = MeanAnomaly(NearestAnomaly(transform.position) * Mathf.Deg2Rad, e);
                startRadial = transform.position - centre;
            }
            else clock += Time.deltaTime;
            ShowTransfer(1f - Mathf.Clamp01((clock - transferSeconds) / Mathf.Max(0.01f, transferFadeSeconds)));

            Vector3 p = clock < transferSeconds
                ? transfer.GetWorldPoint(TrueAnomaly(Mathf.Lerp(startMean, Mathf.PI, clock / transferSeconds), e) * Mathf.Rad2Deg)
                : finalOrbit.GetWorldPoint(180f + 360f * (clock - transferSeconds) / orbitPeriod);
            transform.position = p;
            Quaternion rest = transform.parent != null ? transform.parent.rotation * restRotation : restRotation;
            transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(startRadial, p - centre, axis), axis) * rest;
        }

        // True anomaly (degrees, 0-180) of the transfer orbit's point nearest to a world position.
        float NearestAnomaly(Vector3 position)
        {
            float best = 0f, bestDist = float.MaxValue;
            for (float a = 0f; a <= 180f; a += 0.5f)
            {
                float d = (transfer.GetWorldPoint(a) - position).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = a; }
            }
            return best;
        }

        static float MeanAnomaly(float trueAnomaly, float e)
        {
            float ecc = 2f * Mathf.Atan2(Mathf.Sqrt(1f - e) * Mathf.Sin(trueAnomaly * 0.5f), Mathf.Sqrt(1f + e) * Mathf.Cos(trueAnomaly * 0.5f));
            return ecc - e * Mathf.Sin(ecc);
        }

        static float TrueAnomaly(float mean, float e)
        {
            float ecc = mean;
            for (int i = 0; i < 10; i++) ecc -= (ecc - e * Mathf.Sin(ecc) - mean) / (1f - e * Mathf.Cos(ecc));
            return 2f * Mathf.Atan2(Mathf.Sqrt(1f + e) * Mathf.Sin(ecc * 0.5f), Mathf.Sqrt(1f - e) * Mathf.Cos(ecc * 0.5f));
        }
    }
}
