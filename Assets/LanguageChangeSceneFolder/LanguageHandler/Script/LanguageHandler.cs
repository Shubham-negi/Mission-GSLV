using UnityEngine;
using System.Collections;
using SmartLocalization;
using UnityEngine.UI;
using ArabicSupport;
using System.Collections.Generic;
using TMPro;
using Rwizen.Assessment;

[System.Serializable]
public class LanguageData
{

    public enum LanguageTextAlignment{LeftToRight, RightToLeft};

    public string DisplayName;
    public LanguageID Language_ID;

    public string LanguageID
    {
        get { return Language_ID.ToString().Replace("_", "-"); }
    }

    public LanguageTextAlignment _Alignment;

}



public class LanguageHandler : MonoBehaviour
{
    [Header("Enter the Languages IDs here and then Click on the Save button")]
    public List<string> languageIDs;

    public static string VoiceOverPath = "VoiceOvers/";
	public static string FontPath = "Fonts/";


    public delegate void LanguageChanged();
    public static event LanguageChanged LanguageChangeEventFire;

    [Header("This will set tool language editing bar ")]
    public string currentLanguage;
    public List<LanguageData> Languages;
    [HideInInspector]
    public int CurrentLanguageIndex;
    
    public const int _DefaultLanguageIndex = 1;


    private static LanguageHandler Instance = null;
    public static LanguageHandler instance
    {
        get
        {
            if (Instance == null)
                Instance = FindObjectOfType<LanguageHandler>();
            return Instance;
        }

    }

    public bool IsLeftToRight
    {
        get
        { 
            return (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex]._Alignment == LanguageData.LanguageTextAlignment.LeftToRight);
        }
    }

    public bool IsRightToLeft
    {
        get
        { 
            return (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex]._Alignment == LanguageData.LanguageTextAlignment.RightToLeft);
        }
    }

    string languageId;
    public const string defaultLanguage = "en-GB";


    public void Awake()
    {


        // below condition is if your playing your app first time
        if (!PlayerPrefs.HasKey("currentLanguage"))
            PlayerPrefs.SetString("currentLanguage", defaultLanguage);

        //Step1
        setCurrentLanguage();
        changeLanguageUpdate();
        currentLanguage = PlayerPrefs.GetString("currentLanguage");



    }



    internal void setCurrentLanguage(){    
        if (!PlayerPrefs.HasKey("currentLanguage")){
			PlayerPrefs.SetString("currentLanguage", defaultLanguage);
        }
        else
        {
            for (int i = 0; i < Languages.Count; i++)
            {
                if (PlayerPrefs.GetString("currentLanguage").Equals(Languages[i].LanguageID))
                {
                    CurrentLanguageIndex = i;
                    return;
                }
            }
        }
    }

    internal void changeLanguageUpdate()
    {
        LanguageManager.Instance.ChangeLanguage(Languages[CurrentLanguageIndex].LanguageID);
        if (LanguageChangeEventFire != null)
            LanguageChangeEventFire();
    }   

    public void OnLanguageChangeListener(Text TextBox, string CurrKey)
    {
        if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex]._Alignment==LanguageData.LanguageTextAlignment.RightToLeft)
        {
            switch (TextBox.alignment)
            {
                case TextAnchor.UpperLeft:
                    TextBox.alignment = TextAnchor.UpperRight;
                    break;
                case TextAnchor.MiddleLeft:
                    TextBox.alignment = TextAnchor.MiddleRight;
                    break;
                case TextAnchor.LowerLeft:
                    TextBox.alignment = TextAnchor.LowerRight;
                    break;
            }
            TextBox.text = ArabicFixer.Fix("" + LanguageManager.Instance.GetTextValue(CurrKey));          
        }
        else
        {
            switch (TextBox.alignment)
            {
                case TextAnchor.UpperRight:
                    TextBox.alignment = TextAnchor.UpperLeft;
                    break;
                case TextAnchor.MiddleRight:
                    TextBox.alignment = TextAnchor.MiddleLeft;
                    break;
                case TextAnchor.LowerRight:
                    TextBox.alignment = TextAnchor.LowerLeft;
                    break;
            }
            TextBox.text = "" + LanguageManager.Instance.GetTextValue(CurrKey);
         //   if (currentLanguage == "hi-IN")
               // TextBox.SetHindiText(TextBox.text);
        }
    }

    public void OnLanguageChangeListener(TextMeshProUGUI TextBox, string CurrKey)
    {
        if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex]._Alignment==LanguageData.LanguageTextAlignment.RightToLeft)
        {
            TextBox.alignment = TextAlignmentOptions.Right;

            TextBox.text = ArabicFixer.Fix("" + LanguageManager.Instance.GetTextValue(CurrKey));
        }
        else
        {         
            TextBox.alignment = TextAlignmentOptions.Left;

          
         
            TextBox.text = "" + LanguageManager.Instance.GetTextValue(CurrKey);

          //  if (currentLanguage == "hi-IN")
             //   TextBox.SetHindiTMPro(TextBox.text);

        }
    }

    public void PlayVoiceOver(string key)
    {


        if (GlobalAudioSrc.Instance.audioSrc.isPlaying && GlobalAudioSrc.Instance.audioSrc.clip.name == key)
            return;

        
        GlobalAudioSrc.Instance.audioSrc.clip = null;
       

		AudioClip _Clip;
		_Clip = Resources.Load < AudioClip >(VoiceOverPath + LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + key);
		if (_Clip == null)
			{
				return;
			}

        GlobalAudioSrc.Instance.audioSrc.Stop();
        GlobalAudioSrc.Instance.audioSrc.clip = _Clip;
        GlobalAudioSrc.Instance.audioSrc.PlayOneShot(_Clip);
    }

    public float VoiceClipLength(string key)
    {

        AudioClip _Clip;
        _Clip = Resources.Load<AudioClip>(VoiceOverPath + LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + key);
       

        return _Clip.length;

    }

    public void StopVoiceOver()
    {
        if (GlobalAudioSrc.Instance == null)
            return;
        
        GlobalAudioSrc.Instance.audioSrc.Stop();
    }

    public void PlayBackMenuVoiceOver(string key)
    {
        if (GlobalAudioSrc.Instance.SecondAudioSrc.isPlaying && GlobalAudioSrc.Instance.SecondAudioSrc.clip.name == key)
            return;
        GlobalAudioSrc.Instance.SecondAudioSrc.clip = null;
    	//AudioClip _Clip = Resources.Load<AudioClip>("VoiceOvers/" + LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + key);
		AudioClip _Clip;
		_Clip = Resources.Load < AudioClip >(VoiceOverPath + LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + key);

		if (_Clip == null)
		{
			return;
		}

        GlobalAudioSrc.Instance.audioSrc.Stop();
        GlobalAudioSrc.Instance.SecondAudioSrc.clip = _Clip;
        GlobalAudioSrc.Instance.SecondAudioSrc.PlayOneShot(_Clip);
    }

    public void StopBackMenuVoiceOver()
    {
        if (GlobalAudioSrc.Instance == null)
            return;

        GlobalAudioSrc.Instance.SecondAudioSrc.Stop();
    }

    public bool CheckIfLanguageExist(string _currentLanguage)
    {
        List < SmartCultureInfo > languages = SmartLocalization.LanguageManager.Instance.GetSupportedLanguages();

        for (int i = 0; i < languages.Count; i++)
        {
            if (languages[i].languageCode == _currentLanguage)
            {
                return true;
            }
        }
        return false;
    }
}
