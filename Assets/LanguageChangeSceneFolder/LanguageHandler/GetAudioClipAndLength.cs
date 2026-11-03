using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetAudioClipAndLength : MonoBehaviour {
  
    public static GetAudioClipAndLength instance;


    void Start()
    {
        instance = this;
    }

    public float GetVoiceOverLength(string Key) {
		//Debug.Log("Audio file path of Rest"+PlayerPrefs.GetString("currentLanguage"));
		if ( Resources.Load < AudioClip >(LanguageHandler.VoiceOverPath + 
			LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + Key) == null){
			Debug.LogError("Audio with this name is missing");
			return -1;
		} 

		return Resources.Load < AudioClip >(LanguageHandler.VoiceOverPath + 
			LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + Key).length;
    }

    public AudioClip GetVoiceOver(string Key) {
		// Debug.Log("Audio file path of Rest"+PlayerPrefs.GetString("currentLanguage"));
		if ( Resources.Load < AudioClip >(LanguageHandler.VoiceOverPath + 
			LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + Key) == null){
			Debug.LogError("Audio with this name is missing");
			return null;
		} 

		return Resources.Load < AudioClip >(LanguageHandler.VoiceOverPath + 
			LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + Key);
		
    }
}
