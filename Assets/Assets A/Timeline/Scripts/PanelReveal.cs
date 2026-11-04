using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace GslvMission
{
    /// <summary>
    /// A panel's entrance and exit: it fades in with a short slide, its border lights up, its icons appear one by one, and it fades
    /// out before the Timeline switches it off. Everything follows the Timeline's time, so it stays in step with the voice-over,
    /// through a pause too. The Timeline's Control Track switches the panel on and off between 'start' and 'end'.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelReveal : MonoBehaviour
    {
        [Serializable]
        public class Item
        {
            public RectTransform target;
            public CanvasGroup group;
            [Tooltip("Seconds after the panel's start.")]
            public float delay;
        }

        public PlayableDirector director;
        public double start, end;
        public float fadeIn = 0.7f;
        public float fadeOut = 0.6f;
        [Tooltip("Where the panel slides in from, in metres from its place (x = its right, y = up).")]
        public Vector3 slide;
        public Image borderLight;
        [Tooltip("Timeline times at which the border lights up.")]
        public double[] flashes;
        public float flashSeconds = 1.3f;
        public Item[] items;
        public float itemSeconds = 0.45f;

        CanvasGroup group;
        Vector3 home;
        Color borderColor;

        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            home = transform.localPosition;
            if (borderLight != null) borderColor = borderLight.color;
        }

        void OnEnable()
        {
            Apply();
        }

        void LateUpdate()
        {
            Apply();
        }

        void Apply()
        {
            if (director == null) return;
            double t = director.time;
            float shown = Ease(t, start, fadeIn), gone = Ease(t, end - fadeOut, fadeOut);
            group.alpha = shown * (1f - gone);
            transform.localPosition = home + transform.localRotation * (slide * (1f - shown));

            if (borderLight != null)
            {
                float light = 0f;
                if (flashes != null)
                    foreach (double f in flashes)
                    {
                        double x = (t - f) / flashSeconds;
                        if (x > 0.0 && x < 1.0) light = Mathf.Max(light, Mathf.Sin((float)x * Mathf.PI));
                    }
                borderLight.enabled = light > 0f;
                borderLight.color = new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * light);
            }

            if (items == null) return;
            foreach (var item in items)
            {
                float k = Ease(t, start + item.delay, itemSeconds);
                if (item.group != null) item.group.alpha = k;
                if (item.target != null) item.target.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, k);
            }
        }

        static float Ease(double t, double from, float seconds)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((float)((t - from) / Math.Max(0.001, seconds))));
        }
    }
}
