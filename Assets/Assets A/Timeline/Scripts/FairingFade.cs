using UnityEngine;
using UnityEngine.Playables;

namespace GslvMission
{
    /// <summary>
    /// Makes a group of renderers (the payload fairing) see-through between two Timeline times, so what is inside shows:
    /// fades down to 'minOpacity' from 'start', back up from 'end'. While faded the renderers use transparent copies of
    /// their materials (built by the editor and kept as assets, so the shader variant is in the build); the opacity is set
    /// with a property block, so no material asset is changed. Once opaque again the original materials go back.
    /// </summary>
    public class FairingFade : MonoBehaviour
    {
        public PlayableDirector director;
        public Renderer[] renderers;
        public Material[] sourceMaterials;   // the materials the parts normally use
        public Material[] fadeMaterials;     // transparent copy of each source material, same order
        public double start, end;
        public float fadeTime = 1f;
        [Range(0f, 1f)] public float minOpacity = 0.2f;

        static readonly int ColorId = Shader.PropertyToID("_Color");
        Material[][] originals;
        MaterialPropertyBlock block;

        void LateUpdate()
        {
            if (director == null || renderers == null) return;
            double t = director.time;
            float k = Mathf.Min(Mathf.InverseLerp((float)start, (float)start + fadeTime, (float)t), 1f - Mathf.InverseLerp((float)end, (float)end + fadeTime, (float)t));
            float opacity = Mathf.Lerp(1f, minOpacity, Mathf.SmoothStep(0f, 1f, k));
            if (opacity > 0.995f) { Restore(); return; }
            Swap();
            foreach (var r in renderers)
            {
                if (r == null) continue;
                var mat = r.sharedMaterial;
                if (mat == null || !mat.HasProperty(ColorId)) continue;
                var c = mat.GetColor(ColorId);
                c.a *= opacity;
                block.SetColor(ColorId, c);
                r.SetPropertyBlock(block);
            }
        }

        void Swap()
        {
            if (originals != null) return;
            block = new MaterialPropertyBlock();
            originals = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                var current = renderers[i].sharedMaterials;
                originals[i] = current;
                var swapped = new Material[current.Length];
                for (int m = 0; m < current.Length; m++) swapped[m] = FadeCopy(current[m]);
                renderers[i].sharedMaterials = swapped;
            }
        }

        void Restore()
        {
            if (originals == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null || originals[i] == null) continue;
                renderers[i].sharedMaterials = originals[i];
                renderers[i].SetPropertyBlock(null);
            }
            originals = null;
        }

        void OnDisable()
        {
            Restore();
        }

        Material FadeCopy(Material source)
        {
            if (sourceMaterials == null) return source;
            for (int i = 0; i < sourceMaterials.Length; i++)
                if (sourceMaterials[i] == source && fadeMaterials != null && i < fadeMaterials.Length && fadeMaterials[i] != null) return fadeMaterials[i];
            return source;
        }
    }
}
