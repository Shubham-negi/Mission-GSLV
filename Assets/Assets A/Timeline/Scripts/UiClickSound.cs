using UnityEngine;
using UnityEngine.UI;

namespace GslvMission
{
    /// <summary>
    /// Plays the UI click sound whenever one of 'buttons' is clicked. A click that comes within 'cooldown' seconds of the
    /// previous one is not played again, so a double click or two buttons at once never stack the sound.
    /// Add a button to the list (or call Hook at run time) to give it the same sound.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class UiClickSound : MonoBehaviour
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.8f;
        public float cooldown = 0.15f;
        public Button[] buttons;

        AudioSource source;
        float lastPlayed = -10f;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            if (buttons == null) return;
            foreach (var b in buttons) Hook(b);
        }

        public void Hook(Button button)
        {
            if (button != null) button.onClick.AddListener(Play);
        }

        public void Play()
        {
            if (clip == null || source == null || Time.unscaledTime - lastPlayed < cooldown) return;
            lastPlayed = Time.unscaledTime;
            source.PlayOneShot(clip, volume);
        }
    }
}
