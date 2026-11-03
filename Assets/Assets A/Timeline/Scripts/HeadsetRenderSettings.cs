using UnityEngine;
using UnityEngine.Rendering;

namespace GslvMission
{
    /// <summary>
    /// Render settings for the standalone headset, applied when this scene loads (before QWR creates its eye buffers, which
    /// take their MSAA from QualitySettings). When the app renders below the display's rate (72 fps), the headset re-shows
    /// old frames and fine, high-contrast detail (buildings, roads, the lattice towers) flickers; these keep it fast:
    /// 4x MSAA instead of the Android quality level's 8x, no soft-vegetation second pass, and cheaper distant terrain.
    /// The eye cameras' near plane moves out so the depth buffer has more precision for roads on the ground.
    /// The quality settings go back to what they were when the scene unloads (this scene may run inside a larger app).
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class HeadsetRenderSettings : MonoBehaviour
    {
        [Header("Quality")]
        [Range(1, 8)] public int msaa = 4;
        public bool softVegetation = false;
        public bool realtimeReflectionProbes = false;

        [Header("Eye cameras")]
        [Tooltip("Nothing comes closer to the learner than the UI (7 m), so the near plane can be well out from the eyes.")]
        public float nearClip = 0.3f;

        [Header("Terrain")]
        [Tooltip("Coarser terrain lets its slopes poke through the road laid on it, so this stays at the scene's own value.")]
        public float terrainPixelError = 5f;
        [Tooltip("Beyond this the ground uses its pre-blended base map: cheaper, and no shimmer from tiled ground textures far away.")]
        public float basemapDistance = 150f;
        public float treeDistance = 800f;
        public float detailDistance = 40f;

        // the app's own settings, kept while any copy of this scene is loaded (RESTART loads the next before the last goes)
        static int live;
        static int oldMsaa;
        static bool oldSoftVegetation, oldRealtimeProbes;

        void Awake()
        {
            if (live++ == 0)
            {
                oldMsaa = QualitySettings.antiAliasing;
                oldSoftVegetation = QualitySettings.softVegetation;
                oldRealtimeProbes = QualitySettings.realtimeReflectionProbes;
            }
            QualitySettings.antiAliasing = msaa;
            QualitySettings.softVegetation = softVegetation;
            QualitySettings.realtimeReflectionProbes = realtimeReflectionProbes;

            foreach (var t in FindObjectsOfType<Terrain>())
            {
                t.heightmapPixelError = Mathf.Max(t.heightmapPixelError, terrainPixelError);
                t.basemapDistance = Mathf.Min(t.basemapDistance, basemapDistance);
                t.treeDistance = Mathf.Min(t.treeDistance, treeDistance);
                t.detailObjectDistance = Mathf.Min(t.detailObjectDistance, detailDistance);
                t.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        void Start()
        {
            // the QWR rig's eye cameras exist (and may be disabled, rendered by QWR itself) once every Awake has run
            foreach (var cam in FindObjectsOfType<Camera>())
                if (cam.nearClipPlane < nearClip) cam.nearClipPlane = nearClip;
        }

        void OnDestroy()
        {
            if (--live > 0) return;
            QualitySettings.antiAliasing = oldMsaa;
            QualitySettings.softVegetation = oldSoftVegetation;
            QualitySettings.realtimeReflectionProbes = oldRealtimeProbes;
        }
    }
}
