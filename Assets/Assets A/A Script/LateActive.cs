using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateActive : MonoBehaviour
{
    public float _delayTime;
    public List<GameObject> _activeObjects;

    void OnEnable()
    {
        if (Resources.Load<AudioClip>(LanguageHandler.VoiceOverPath + LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + gameObject.name) != null)
        {   
            var t = GetAudioClipAndLength.instance.GetVoiceOverLength(gameObject.name);
            Invoke("DeactiveObject", t);
        }
        else
        {
            Invoke("DeactiveObject", _delayTime);
        }
        
    }
    
    void DeactiveObject()
    {
        foreach(var activeObject in _activeObjects)
        {
            activeObject.SetActive(true);
        }
    }
}