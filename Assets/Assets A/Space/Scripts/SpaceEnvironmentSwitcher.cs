using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GslvSpace
{
    /// <summary>
    /// Swaps the learner's surroundings between the launch pad and space. In the editor the space
    /// set is parked away from the launch pad; ShowSpace() moves it in front of the learner's eyes,
    /// hides the launch pad, and swaps the sun, skybox, ambient light and reflections.
    /// ShowLaunchpad() restores exactly what ShowSpace() changed.
    /// Call ShowSpace() at the Scene 3 to 4 transition (for example during a fade).
    /// Edit the objects inside Space Content; this root is repositioned when ShowSpace() runs.
    /// </summary>
    public class SpaceEnvironmentSwitcher : MonoBehaviour
    {
        [Header("Launch pad")]
        [Tooltip("Hidden while in space. The branch holding the learner's camera rig is never hidden.")]
        public GameObject[] launchpadObjects;
        public Light launchpadSun;

        [Header("Space")]
        public GameObject spaceContent;
        public Light spaceSun;
        public Material spaceSkybox;
        public Cubemap spaceReflection;
        public Color spaceAmbientColor = new Color(0.07f, 0.08f, 0.11f);

        [Header("Learner")]
        [Tooltip("Fallback rig, used only if QWR has no active camera rig.")]
        public Transform viewer;

        [Tooltip("Show space as soon as the scene starts instead of the launch pad.")]
        public bool startInSpace;

        [Tooltip("Turn the space set to the rig's forward direction (where the UI is) rather than to where the learner happens to look.")]
        public bool faceRigForward = true;

        public bool InSpace { get; private set; }

        readonly List<GameObject> hidden = new List<GameObject>();

        // Launch-pad state captured by ShowSpace() and put back by ShowLaunchpad().
        Material savedSkybox;
        AmbientMode savedAmbientMode;
        Color savedAmbientLight;
        bool savedFog;
        DefaultReflectionMode savedReflectionMode;
        Cubemap savedReflection;
        bool savedSunEnabled;

        void Awake()
        {
            // Space stays switched off until it is needed; the launch pad and its lighting are left as authored.
            if (spaceContent != null) spaceContent.SetActive(false);
            if (spaceSun != null) spaceSun.enabled = false;
        }

        IEnumerator Start()
        {
            if (!startInSpace) yield break;
            yield return null; // let QWR settle on one camera rig and a head pose first
            ShowSpace();
        }

        public void ShowSpace()
        {
            if (InSpace) return;
            InSpace = true;

            Transform head;
            Transform rig = ActiveRig(out head);
            if (rig != null) AlignTo(rig, head);

            savedSkybox = RenderSettings.skybox;
            savedAmbientMode = RenderSettings.ambientMode;
            savedAmbientLight = RenderSettings.ambientLight;
            savedFog = RenderSettings.fog;
            savedReflectionMode = RenderSettings.defaultReflectionMode;
            savedReflection = RenderSettings.customReflection;
            savedSunEnabled = launchpadSun != null && launchpadSun.enabled;

            if (launchpadObjects != null)
                foreach (GameObject go in launchpadObjects)
                    if (go != null) Hide(go.transform, rig);
            if (launchpadSun != null) launchpadSun.enabled = false;
            if (spaceContent != null) spaceContent.SetActive(true);
            if (spaceSun != null) spaceSun.enabled = true;

            RenderSettings.skybox = spaceSkybox;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = spaceAmbientColor;
            RenderSettings.fog = false;
            if (spaceReflection != null)
            {
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                RenderSettings.customReflection = spaceReflection;
            }
            DynamicGI.UpdateEnvironment();
        }

        public void ShowLaunchpad()
        {
            if (!InSpace) return;
            InSpace = false;

            foreach (GameObject go in hidden)
                if (go != null) go.SetActive(true);
            hidden.Clear();
            if (launchpadSun != null) launchpadSun.enabled = savedSunEnabled;
            if (spaceContent != null) spaceContent.SetActive(false);
            if (spaceSun != null) spaceSun.enabled = false;

            RenderSettings.skybox = savedSkybox;
            RenderSettings.ambientMode = savedAmbientMode;
            RenderSettings.ambientLight = savedAmbientLight;
            RenderSettings.fog = savedFog;
            RenderSettings.defaultReflectionMode = savedReflectionMode;
            RenderSettings.customReflection = savedReflection;
            DynamicGI.UpdateEnvironment();
        }

        // Hides a launch-pad branch, but descends into (rather than hides) any branch that holds the
        // rendering camera rig or this switcher, so the learner's view never goes dark.
        void Hide(Transform t, Transform rig)
        {
            if (!t.gameObject.activeSelf || t == rig) return;
            if ((rig != null && rig.IsChildOf(t)) || transform.IsChildOf(t))
            {
                foreach (Transform child in t) Hide(child, rig);
                return;
            }
            t.gameObject.SetActive(false);
            hidden.Add(t.gameObject);
        }

        /// <summary>The rig QWR renders through, else the fallback viewer, else the main camera.</summary>
        Transform ActiveRig(out Transform head)
        {
            head = null;
            qwr.QwrCameraRig qwrRig = qwr.QwrCameraRig._instance;
            if (qwrRig != null && qwrRig.isActiveAndEnabled)
            {
                head = qwrRig._center_transform;
                return qwrRig.transform;
            }
            if (viewer != null && viewer.gameObject.activeInHierarchy) return viewer;
            Camera main = Camera.main;
            if (main == null) return null;
            head = main.transform;
            var parentRig = main.GetComponentInParent<qwr.QwrCameraRig>();
            return parentRig != null ? parentRig.transform : main.transform;
        }

        // Puts the space set's origin at the learner's eyes, turned to the rig's forward (or to where they look).
        void AlignTo(Transform rig, Transform head)
        {
            Transform eye = head != null ? head : rig;
            Vector3 look = Vector3.ProjectOnPlane(faceRigForward ? rig.forward : eye.forward, Vector3.up);
            if (look.sqrMagnitude < 0.0001f) look = Vector3.ProjectOnPlane(rig.forward, Vector3.up);
            if (look.sqrMagnitude < 0.0001f) look = Vector3.forward;
            transform.SetPositionAndRotation(eye.position, Quaternion.LookRotation(look.normalized, Vector3.up));
        }
    }
}
