using UnityEngine;

namespace GslvMission
{
    /// <summary>
    /// "Illuminates" a group of renderers while this GameObject is active: swaps their materials for
    /// emissive copies and pulses the glow. The Timeline switches it on and off with a Control Track.
    /// </summary>
    public class PartHighlight : MonoBehaviour
    {
        public Renderer[] renderers;
        public Material[] sourceMaterials;   // the materials the parts normally use
        public Material[] glowMaterials;     // emissive copy of each source material, same order
        public Color color = new Color(0.15f, 0.85f, 1f);
        public float minIntensity = 0.45f;
        public float maxIntensity = 1.5f;
        public float pulseSpeed = 5f;

        static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        Material[][] originals;
        MaterialPropertyBlock block;

        void OnEnable()
        {
            if (renderers == null) return;
            block = new MaterialPropertyBlock();
            originals = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                var current = renderers[i].sharedMaterials;
                originals[i] = current;
                var swapped = new Material[current.Length];
                for (int m = 0; m < current.Length; m++) swapped[m] = Glow(current[m]);
                renderers[i].sharedMaterials = swapped;
            }
            Pulse();
        }

        void OnDisable()
        {
            if (renderers == null || originals == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null || originals[i] == null) continue;
                renderers[i].sharedMaterials = originals[i];
                renderers[i].SetPropertyBlock(null);
            }
            originals = null;
        }

        void Update()
        {
            Pulse();
        }

        void Pulse()
        {
            if (block == null || renderers == null) return;
            float k = Mathf.Lerp(minIntensity, maxIntensity, 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed));
            block.SetColor(EmissionColor, color * k);
            foreach (var r in renderers)
                if (r != null) r.SetPropertyBlock(block);
        }

        Material Glow(Material source)
        {
            if (sourceMaterials == null) return source;
            for (int i = 0; i < sourceMaterials.Length; i++)
                if (sourceMaterials[i] == source && glowMaterials != null && i < glowMaterials.Length && glowMaterials[i] != null) return glowMaterials[i];
            return source;
        }
    }
}
