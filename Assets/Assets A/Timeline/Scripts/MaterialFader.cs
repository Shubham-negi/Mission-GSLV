using UnityEngine;

namespace GslvMission
{
    /// <summary>Fades a renderer (its own material instance) by 'opacity', which the Timeline animates.</summary>
    public class MaterialFader : MonoBehaviour
    {
        [Range(0f, 1f)] public float opacity = 1f;
        public string colorProperty = "_TintColor";

        Material mat;
        Color baseColor;

        void Awake()
        {
            var rend = GetComponent<Renderer>();
            if (rend == null || rend.sharedMaterial == null || !rend.sharedMaterial.HasProperty(colorProperty)) return;
            mat = rend.material;
            baseColor = mat.GetColor(colorProperty);
        }

        void LateUpdate()
        {
            if (mat == null) return;
            var c = baseColor;
            c.a *= opacity;
            mat.SetColor(colorProperty, c);
        }

        void OnDestroy()
        {
            if (mat != null) Destroy(mat);
        }
    }
}
