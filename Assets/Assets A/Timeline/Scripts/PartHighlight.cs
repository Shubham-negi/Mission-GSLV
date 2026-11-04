using UnityEngine;

namespace GslvMission
{
    /// <summary>
    /// "Illuminates" a group of renderers while this GameObject is active: swaps their materials for
    /// copies on the Part Highlight shader and pulses the glow and the Fresnel rim around the part's edges.
    /// Emission alone only adds light, so a white surface would stay white; the surface colour is tinted as
    /// well, which keeps the part warm on bright and dark backgrounds. Bright parts clip once the sum passes 1,
    /// and red and green clipping together turn lemon yellow, so these values stay low.
    /// The Timeline switches it on and off with a Control Track.
    /// </summary>
    public class PartHighlight : MonoBehaviour
    {
        public Renderer[] renderers;
        public Material[] sourceMaterials;   // the materials the parts normally use
        public Material[] glowMaterials;     // Part Highlight copy of each source material, same order
        [Tooltip("The glow at the top of the pulse.")]
        public Color color = new Color(1f, 0.75f, 0.35f);
        [Tooltip("The glow at the bottom of the pulse.")]
        public Color accentColor = new Color(1f, 0.73f, 0.32f);
        [Tooltip("Multiplies the part's own colour while highlighted (white = unchanged).")]
        public Color surfaceTint = new Color(0.80f, 0.66f, 0.28f);
        [Tooltip("The glow is multiplied by the part's own texture.")]
        public float minIntensity = 0.25f;
        public float maxIntensity = 0.45f;
        [Tooltip("The Fresnel rim around the part's edges.")]
        public Color rimColor = new Color(1f, 0.95f, 0.68f);
        [Tooltip("Higher = a thinner rim, closer to the edge.")]
        public float rimPower = 1.4f;
        public float minRim = 1f;
        public float maxRim = 1.1f;
        public float pulseSpeed = 5f;

        static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        static readonly int BaseColor = Shader.PropertyToID("_Color");
        static readonly int RimColor = Shader.PropertyToID("_RimColor");
        static readonly int RimPower = Shader.PropertyToID("_RimPower");
        Material[][] originals;
        Color[][] tinted;
        MaterialPropertyBlock block;

        void OnEnable()
        {
            if (renderers == null) return;
            block = new MaterialPropertyBlock();
            originals = new Material[renderers.Length][];
            tinted = new Color[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                var current = renderers[i].sharedMaterials;
                originals[i] = current;
                var swapped = new Material[current.Length];
                tinted[i] = new Color[current.Length];
                for (int m = 0; m < current.Length; m++)
                {
                    swapped[m] = Glow(current[m]);
                    var own = swapped[m] != null && swapped[m].HasProperty(BaseColor) ? swapped[m].GetColor(BaseColor) : Color.white;
                    tinted[i][m] = own * surfaceTint;
                }
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
                for (int m = 0; m < originals[i].Length; m++) renderers[i].SetPropertyBlock(null, m);
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
            if (block == null || renderers == null || originals == null) return;
            float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
            block.SetColor(EmissionColor, Color.Lerp(accentColor, color, wave) * Mathf.Lerp(minIntensity, maxIntensity, wave));
            block.SetColor(RimColor, rimColor * Mathf.Lerp(minRim, maxRim, wave));
            block.SetFloat(RimPower, rimPower);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null || tinted[i] == null) continue;
                for (int m = 0; m < tinted[i].Length; m++)
                {
                    block.SetColor(BaseColor, tinted[i][m]);
                    renderers[i].SetPropertyBlock(block, m);
                }
            }
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
