using UnityEngine;

namespace GslvMission
{
    /// <summary>Engine glow: the light flickers around its base intensity, like the fire it comes from.</summary>
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        public float baseIntensity = 3.5f;
        [Range(0f, 1f)] public float flicker = 0.35f;
        public float speed = 16f;

        Light lamp;
        float seed;

        void Awake()
        {
            lamp = GetComponent<Light>();
            seed = Random.value * 100f;
        }

        void Update()
        {
            float n = Mathf.PerlinNoise(seed, Time.time * speed);
            lamp.intensity = baseIntensity * (1f - flicker + 2f * flicker * n);
        }
    }
}
