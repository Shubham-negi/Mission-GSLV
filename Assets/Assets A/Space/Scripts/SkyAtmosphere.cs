using UnityEngine;

namespace GslvSpace
{
    /// <summary>
    /// The sky while the rocket climbs out of the atmosphere: blue near the ground, deeper blue in the stratosphere,
    /// indigo in the mesosphere and black with stars above, with a thin bright limb along Earth's horizon once Earth
    /// is seen from above. Lives on a large sphere using the "GSLV/Sky Atmosphere" shader and follows the learner's
    /// eyes. The Timeline animates 'altitudeKm' (the real altitude the flight stands for) and 'opacity'.
    /// </summary>
    [DefaultExecutionOrder(-40)]   // after MissionCamera has moved the learner
    public class SkyAtmosphere : MonoBehaviour
    {
        public float altitudeKm;
        [Range(0f, 1f)] public float opacity = 1f;
        [Tooltip("Earth below during the flight; while it is shown the limb glow sits on its horizon.")]
        public SkyBackdrop earth;
        public Light padSun;
        public Light spaceSun;

        // altitude (km) -> sky colour; between the keys the colours blend
        static readonly float[] Km = { 0f, 12f, 30f, 50f, 85f, 120f };
        static readonly Color[] ZenithAt =
        {
            new Color(0.20f, 0.45f, 0.86f), new Color(0.10f, 0.30f, 0.76f), new Color(0.05f, 0.16f, 0.52f),
            new Color(0.03f, 0.07f, 0.30f), new Color(0.01f, 0.02f, 0.08f), new Color(0f, 0f, 0f)
        };
        static readonly Color[] HorizonAt =
        {
            new Color(0.70f, 0.82f, 0.95f), new Color(0.56f, 0.73f, 0.95f), new Color(0.36f, 0.56f, 0.90f),
            new Color(0.20f, 0.38f, 0.80f), new Color(0.07f, 0.14f, 0.38f), new Color(0.02f, 0.03f, 0.08f)
        };

        static readonly int Zenith = Shader.PropertyToID("_ZenithColor"), Horizon = Shader.PropertyToID("_HorizonColor"),
            LimbStrength = Shader.PropertyToID("_LimbStrength"), LimbWidth = Shader.PropertyToID("_LimbWidth"),
            HorizonDip = Shader.PropertyToID("_HorizonDip"), SunDir = Shader.PropertyToID("_SunDir"),
            SunGlow = Shader.PropertyToID("_SunGlow"), Stars = Shader.PropertyToID("_StarStrength"), Opacity = Shader.PropertyToID("_Opacity");

        Material mat;

        void Awake()
        {
            var rend = GetComponent<Renderer>();
            if (rend != null) mat = rend.material;   // an instance, so the shared asset is never changed in Play mode
        }

        void LateUpdate()
        {
            Transform eye = null;
            var rig = qwr.QwrCameraRig._instance;
            if (rig != null && rig._center_eye != null) eye = rig._center_eye.transform;
            if (eye == null && Camera.main != null) eye = Camera.main.transform;
            if (eye != null) transform.position = eye.position;
            if (mat == null) return;

            float a = Mathf.Max(0f, altitudeKm);
            mat.SetColor(Zenith, Blend(ZenithAt, a));
            mat.SetColor(Horizon, Blend(HorizonAt, a));
            mat.SetFloat(LimbStrength, Mathf.Lerp(0f, 1.1f, Mathf.InverseLerp(10f, 60f, a)));
            mat.SetFloat(LimbWidth, Mathf.Lerp(0.12f, 0.035f, Mathf.InverseLerp(20f, 150f, a)));
            mat.SetFloat(Stars, Mathf.Lerp(0f, 1.5f, Mathf.InverseLerp(45f, 110f, a)));
            mat.SetFloat(SunGlow, Mathf.Lerp(0.35f, 0.08f, Mathf.InverseLerp(0f, 80f, a)));
            mat.SetFloat(Opacity, opacity);

            float dip = 0f;
            if (earth != null && earth.isActiveAndEnabled)
                dip = Mathf.Acos(Mathf.Clamp01(earth.radius / (earth.radius + Mathf.Max(0.01f, earth.height))));
            mat.SetFloat(HorizonDip, dip);

            Light sun = padSun != null && padSun.isActiveAndEnabled ? padSun : (spaceSun != null && spaceSun.isActiveAndEnabled ? spaceSun : RenderSettings.sun);
            if (sun != null) mat.SetVector(SunDir, -sun.transform.forward);
        }

        static Color Blend(Color[] keys, float km)
        {
            for (int i = 1; i < Km.Length; i++)
                if (km <= Km[i]) return Color.Lerp(keys[i - 1], keys[i], Mathf.InverseLerp(Km[i - 1], Km[i], km));
            return keys[keys.Length - 1];
        }

        void OnDestroy()
        {
            if (mat != null) Destroy(mat);
        }
    }
}
