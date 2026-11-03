using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AudioSourceA : MonoBehaviour {
	public static AudioSourceA Instance;
	public AudioSource aSourceVO,aSourceExtVO;

    void Awake () {
        Instance = this;
    }

	public void PlayVoiceOverForGaze (){
		if (AudioSourceA.Instance.aSourceVO.clip != Resources.Load<AudioClip> ("VoiceOvers/"  + gameObject.name)) {
			AudioSourceA.Instance.aSourceVO.clip = Resources.Load<AudioClip> ("VoiceOvers/"  + gameObject.name);
			AudioSourceA.Instance.aSourceVO.PlayOneShot (AudioSourceA.Instance.aSourceVO.clip, 1);
		}
	}

	public void PlayVoiceOverSec (string key){
		if (AudioSourceA.Instance.aSourceExtVO.clip != Resources.Load<AudioClip> ("VoiceOvers/"  + key)) {
			AudioSourceA.Instance.aSourceExtVO.clip = Resources.Load<AudioClip> ("VoiceOvers/" + key);
			AudioSourceA.Instance.aSourceExtVO.PlayOneShot (AudioSourceA.Instance.aSourceExtVO.clip, 1);
		}
	}

	public void StopSecVoiceOver (){
		AudioSourceA.Instance.aSourceExtVO.Stop ();
		AudioSourceA.Instance.aSourceExtVO.clip = null;
	}
}
