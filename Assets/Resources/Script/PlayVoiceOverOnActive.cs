using UnityEngine;
using UnityEngine.UI;

public class PlayVoiceOverOnActive : MonoBehaviour{
	bool IsFirstTime = true;
    public bool _disabledMethodUse=false;

	void Start (){
		PlayVoiceOver ();
		IsFirstTime = false;
	}

	void OnEnable (){
		if (!IsFirstTime) {
			PlayVoiceOver ();
		}
	}

	void OnDisable (){
        if(_disabledMethodUse)StopVoiceOver();
	}
		
	public void PlayVoiceOver (){
		AudioSourceA.Instance.aSourceVO.Stop ();
		AudioSourceA.Instance.aSourceVO.clip = null;
		AudioSourceA.Instance.aSourceVO.clip = Resources.Load<AudioClip> ("VoiceOvers/" + gameObject.name);
		AudioSourceA.Instance.aSourceVO.PlayOneShot (AudioSourceA.Instance.aSourceVO.clip, 1);
	}

	public void StopVoiceOver (){
		AudioSourceA.Instance.aSourceVO.Stop ();
	}
}