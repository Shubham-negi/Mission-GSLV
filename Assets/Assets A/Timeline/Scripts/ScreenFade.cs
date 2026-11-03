using System.Collections;
using UnityEngine;

namespace GslvMission
{
    /// <summary>
    /// Full-view fade. Builds a quad in front of the learner's centre eye at start; the Timeline
    /// animates "alpha" (0 = clear, 1 = covered).
    /// </summary>
    public class ScreenFade : MonoBehaviour
    {
        [Range(0f, 1f)] public float alpha;
        public Material material;      // uses the OverlayUI shader, drawn over everything
        public Color color = Color.black;

        Material instance;
        Renderer quad;

        IEnumerator Start()
        {
            yield return null; // let the camera rig register itself
            Transform eye = null;
            var rig = qwr.QwrCameraRig._instance;
            if (rig != null && rig._center_eye != null) eye = rig._center_eye.transform;
            if (eye == null && Camera.main != null) eye = Camera.main.transform;
            if (eye == null || material == null) yield break;

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Screen Fade";
            Destroy(go.GetComponent<Collider>());   // must never block the pointer
            go.transform.SetParent(eye, false);
            go.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            go.transform.localScale = new Vector3(3f, 3f, 1f);
            quad = go.GetComponent<Renderer>();
            quad.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            quad.receiveShadows = false;
            instance = new Material(material);
            quad.sharedMaterial = instance;
            quad.enabled = false;
        }

        void LateUpdate()
        {
            if (quad == null) return;
            var c = color;
            c.a = alpha;
            instance.color = c;
            quad.enabled = alpha > 0.001f;
        }

        void OnDestroy()
        {
            if (instance != null) Destroy(instance);
        }
    }
}
