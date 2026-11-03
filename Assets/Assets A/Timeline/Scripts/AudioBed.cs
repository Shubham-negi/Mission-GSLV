using UnityEngine;
using UnityEngine.Playables;

namespace GslvMission
{
    /// <summary>
    /// Looping music or ambience that plays while the Timeline's time is between 'start' and 'end', fading in and out.
    /// It is kept off the Timeline's audio tracks because those stop while a gate holds the Timeline; this keeps playing.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioBed : MonoBehaviour
    {
        public PlayableDirector director;
        public double start, end;
        public float fadeIn = 2f, fadeOut = 2f;
        [Range(0f, 1f)] public float volume = 0.2f;

        AudioSource source;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.volume = 0f;
        }

        void Update()
        {
            if (director == null) return;
            double t = director.time;
            float v = 0f;
            if (t > start && t < end)
                v = volume * Mathf.Clamp01((float)(t - start) / Mathf.Max(0.01f, fadeIn)) * Mathf.Clamp01((float)(end - t) / Mathf.Max(0.01f, fadeOut));
            source.volume = v;
            if (v > 0f && !source.isPlaying) source.Play();
            else if (v <= 0f && source.isPlaying) source.Stop();
        }
    }
}
