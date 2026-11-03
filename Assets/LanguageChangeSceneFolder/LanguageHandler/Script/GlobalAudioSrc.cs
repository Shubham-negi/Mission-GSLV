using UnityEngine;
using System.Collections;

public class GlobalAudioSrc : MonoBehaviour
{
    public static GlobalAudioSrc Instance;
    public AudioSource audioSrc,SecondAudioSrc;

    // Use this for initialization
    void Awake()
    {
        Instance = this;
    }
}
