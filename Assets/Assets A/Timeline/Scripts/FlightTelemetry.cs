using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace GslvMission
{
    /// <summary>
    /// Altitude and speed readout during the flight, read off the Timeline's time. The builder fills the table with
    /// illustrative numbers for a GSLV climb to its transfer orbit (seconds after lift-off on the Timeline -> km, km/s).
    /// </summary>
    public class FlightTelemetry : MonoBehaviour
    {
        public PlayableDirector director;
        public double liftTime;
        [Tooltip("x = seconds after lift-off on the Timeline, y = altitude in km, z = speed in km/s")]
        public Vector3[] table;
        public TMP_Text text;

        int shownAlt = -1, shownSpeed = -1;

        void LateUpdate()
        {
            if (director == null || text == null || table == null || table.Length == 0) return;
            Vector3 v = Sample((float)(director.time - liftTime));
            int alt = Mathf.RoundToInt(v.y), speed = Mathf.RoundToInt(v.z * 10f);
            if (alt == shownAlt && speed == shownSpeed) return;
            shownAlt = alt;
            shownSpeed = speed;
            text.text = "ALTITUDE  <b>" + alt + "</b> km\nSPEED  <b>" + (speed / 10f).ToString("0.0") + "</b> km/s";
        }

        Vector3 Sample(float x)
        {
            if (x <= table[0].x) return table[0];
            for (int i = 1; i < table.Length; i++)
                if (x <= table[i].x) return Vector3.Lerp(table[i - 1], table[i], Mathf.InverseLerp(table[i - 1].x, table[i].x, x));
            return table[table.Length - 1];
        }
    }
}
