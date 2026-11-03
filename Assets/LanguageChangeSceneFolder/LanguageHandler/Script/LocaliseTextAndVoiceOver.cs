using UnityEngine;
using UnityEngine.UI;
using SmartLocalization;


//For Arabic only
using ArabicSupport;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif


public enum LocalizeType
{
	Only_Text,
	Only_Voice,
	TextAndVoice,
	Only_Font}
;

public class LocaliseTextAndVoiceOver : MonoBehaviour
{
	public LocalizeType localizeType;
	public bool StopVOOnDisable;
	public bool DoNotAlign;
	bool IsFirstTime = true;
	public bool DoNotChangeFont = false;
	public bool useColorTag = false;

    

	//For Arabic only

	[Multiline]
	private string Text = "";
	//public InputField RefrenceInput;
	// public bool ShowTashkeel;
	// public bool UseHinduNumbers;

	private UnityEngine.UI.Text txt;

	void CallLanguageChangeListener ()
	{		
		if (GetComponentInChildren < TextMeshProUGUI > () == null) {
			OnLanguageChangeListener ();
		} else {			
			OnLanguageChangeListener_textMesh ();
		}
	}

	void Start ()
	{

        
		if (localizeType == LocalizeType.Only_Text) {
			CallLanguageChangeListener ();
			//ChangeFont();
			IsFirstTime = false;
		}

		if (localizeType == LocalizeType.TextAndVoice) {
			CallLanguageChangeListener ();
			PlayVoiceOver ();
			//ChangeFont();
			IsFirstTime = false;
		}

		if (localizeType == LocalizeType.Only_Voice) {
			PlayVoiceOver ();
			IsFirstTime = false;
		}

		if (localizeType == LocalizeType.Only_Font) {
			ChangeFont ();

			IsFirstTime = false;
		}
	}

	void OnEnable ()
	{
		if (localizeType == LocalizeType.Only_Text || localizeType == LocalizeType.TextAndVoice) {			
			if (GetComponentInChildren <TextMeshProUGUI> () == null) {				
				LanguageHandler.LanguageChangeEventFire += OnLanguageChangeListener;
			} else {
				LanguageHandler.LanguageChangeEventFire += OnLanguageChangeListener_textMesh;			
			}
		}

		if (!IsFirstTime) {
			if (localizeType == LocalizeType.Only_Text) {
				CallLanguageChangeListener ();
			}

			if (localizeType == LocalizeType.TextAndVoice) {
				CallLanguageChangeListener ();
				PlayVoiceOver ();
			}

			if (localizeType == LocalizeType.Only_Voice) {
				PlayVoiceOver ();
			}
			if (localizeType == LocalizeType.Only_Font) {              
				ChangeFont ();

			}
		}
	}

	void OnDisable ()
	{		
		if (localizeType == LocalizeType.Only_Text || localizeType == LocalizeType.TextAndVoice) {			
			if (GetComponentInChildren < TextMeshProUGUI > () == null) {
				LanguageHandler.LanguageChangeEventFire -= OnLanguageChangeListener;
			} else {				
				LanguageHandler.LanguageChangeEventFire -= OnLanguageChangeListener_textMesh;
			}
		}

		if (StopVOOnDisable) {
			if ((localizeType == LocalizeType.Only_Voice || localizeType == LocalizeType.TextAndVoice)) {
				StopVoiceOver ();
			}
		}
	}




	public void OnLanguageChangeListener ()
	{

		if (LanguageHandler.instance.Languages [LanguageHandler.instance.CurrentLanguageIndex]._Alignment == LanguageData.LanguageTextAlignment.LeftToRight) {
			if (!DoNotAlign) {
				switch (GetComponentInChildren<Text> ().alignment) {
				case TextAnchor.UpperRight:
					GetComponentInChildren<Text> ().alignment = TextAnchor.UpperLeft;
					break;
				case TextAnchor.MiddleRight:
					GetComponentInChildren<Text> ().alignment = TextAnchor.MiddleLeft;
					break;
				case TextAnchor.LowerRight:
					GetComponentInChildren<Text> ().alignment = TextAnchor.LowerLeft;
					break;
				}
			}

			if (!DoNotChangeFont)
			{
				GetComponentInChildren < Text >().font = Resources.Load<Font>(LanguageHandler.FontPath + "rwizenEnglish");
			}

			GetComponentInChildren < Text > ().text =  "" + LanguageManager.Instance.GetTextValue (gameObject.name);
			

		} else {
			if (!DoNotAlign) {
				switch (GetComponentInChildren<Text> ().alignment) {
				case TextAnchor.UpperLeft:
					GetComponentInChildren<Text> ().alignment = TextAnchor.UpperRight;
					break;
				case TextAnchor.MiddleLeft:
					GetComponentInChildren<Text> ().alignment = TextAnchor.MiddleRight;
					break;
				case TextAnchor.LowerLeft:
					GetComponentInChildren<Text> ().alignment = TextAnchor.LowerRight;
					break;
				}
			} 

			if (!DoNotChangeFont)
			{
				GetComponentInChildren < Text >().font = Resources.Load<Font>(LanguageHandler.FontPath + "rwizenArabic");
			}

			GetComponentInChildren<Text> ().text = ArabicFixer.Fix ("" + LanguageManager.Instance.GetTextValue (gameObject.name), false, false); 

		}
		ChangeFont();
	}



	public void OnLanguageChangeListener_textMesh ()
	{        
		if (LanguageHandler.instance.Languages [LanguageHandler.instance.CurrentLanguageIndex]._Alignment == LanguageData.LanguageTextAlignment.LeftToRight) {
			if (GetComponentInChildren < TextMeshProUGUI > ().alignment != TextAlignmentOptions.Center && !DoNotAlign) {				
				GetComponentInChildren <TextMeshProUGUI> ().alignment = TextAlignmentOptions.Left;
			}

			if (!DoNotChangeFont) {
				GetComponentInChildren <TextMeshProUGUI> ().font = Resources.Load<TMP_FontAsset> (LanguageHandler.FontPath + "rwizenEnglishSDF");			
			}

			GetComponentInChildren<TextMeshProUGUI> ().text = "" + LanguageManager.Instance.GetTextValue (gameObject.name);

		} else {
			if (GetComponentInChildren <TextMeshProUGUI> ().alignment != TextAlignmentOptions.Center && !DoNotAlign) {			
				GetComponentInChildren <TextMeshProUGUI> ().alignment = TextAlignmentOptions.Right;			
			}

			if (!DoNotChangeFont) {
				GetComponentInChildren <TextMeshProUGUI> ().font = Resources.Load<TMP_FontAsset> (LanguageHandler.FontPath + "rwizenArabicSDF");
			}

			GetComponentInChildren <TextMeshProUGUI> ().text = ArabicFixer.Fix ("" + LanguageManager.Instance.GetTextValue (gameObject.name), false, false);		
		}
		ChangeFont();
	}

	AudioClip _Clip;
	public void PlayVoiceOver()
	{
		if (GlobalAudioSrc.Instance == null)
			return;

		GlobalAudioSrc.Instance.audioSrc.clip = null;
		_Clip = Resources.Load < AudioClip >(LanguageHandler.VoiceOverPath + LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID + "/" + gameObject.name);

		if (_Clip == null)
		{
			return;
		}

		GlobalAudioSrc.Instance.audioSrc.Stop();
		GlobalAudioSrc.Instance.audioSrc.clip = _Clip;
		GlobalAudioSrc.Instance.audioSrc.PlayOneShot(_Clip);
	}

	public void PlayVoiceOverForGaze ()
	{
		if (GlobalAudioSrc.Instance == null)
			return;
        
		PlayVoiceOver ();
	}

	public void StopVoiceOver ()
	{
		if (GlobalAudioSrc.Instance == null)
			return;

		GlobalAudioSrc.Instance.audioSrc.Stop ();
	}

	

	void ChangeFont ()
	{
		if (LanguageHandler.instance.Languages [LanguageHandler.instance.CurrentLanguageIndex]._Alignment == LanguageData.LanguageTextAlignment.LeftToRight) {
			if (GetComponentInChildren <TextMeshProUGUI> () == null) {

				if (LanguageHandler.instance.currentLanguage == "hi-IN")
				{
					string text = GetComponentInChildren<Text>().text;
					GetComponentInChildren<Text>().SetHindiText(text);

					Invoke("HandleColorTagText", .01f);
				}
				else if (LanguageHandler.instance.currentLanguage == "gu-IN")
				{
					GetComponentInChildren<Text>().font = Resources.Load<Font>(LanguageHandler.FontPath + "rwizenGujarati");
				}
				else if (LanguageHandler.instance.currentLanguage == "kn-IN")
				{
					GetComponentInChildren<Text>().font = Resources.Load<Font>(LanguageHandler.FontPath + "rwizenKannada");
				}
				else if (LanguageHandler.instance.currentLanguage == "mr-IN")
				{
					GetComponentInChildren<Text>().font = Resources.Load<Font>(LanguageHandler.FontPath + "rwizenMarathi");
				}
				else if (LanguageHandler.instance.currentLanguage == "or-IN")
				{
					GetComponentInChildren<Text>().font = Resources.Load<Font>(LanguageHandler.FontPath + "rwizenOriya");
				}
				else if (LanguageHandler.instance.currentLanguage == "ta-IN")
				{
					GetComponentInChildren<Text>().font = Resources.Load<Font>(LanguageHandler.FontPath + "rwizenTamil");
				}
				else if (LanguageHandler.instance.currentLanguage == "te-IN")
				{
					GetComponentInChildren<Text>().font = Resources.Load<Font>(LanguageHandler.FontPath + "rwizenTelugu");
				}
				else
					GetComponentInChildren<Text>().font = Resources.Load<Font>(LanguageHandler.FontPath + "rwizenEnglish");

			} else {

				if (LanguageHandler.instance.currentLanguage == "hi-IN")
				{
					GetComponentInChildren<TextMeshProUGUI>().SetHindiTMPro(GetComponentInChildren<TextMeshProUGUI>().text);
                    StartCoroutine(HandleColorTagTextMesh());
				}
				else if (LanguageHandler.instance.currentLanguage == "gu-IN")
				{
					GetComponentInChildren<TextMeshProUGUI>().font = Resources.Load<TMP_FontAsset>(LanguageHandler.FontPath + "rwizenGujaratiSDF");
				}
				else if (LanguageHandler.instance.currentLanguage == "kn-IN")
				{
					GetComponentInChildren<TextMeshProUGUI>().font = Resources.Load<TMP_FontAsset>(LanguageHandler.FontPath + "rwizenKannadaSDF");
				}
				else if (LanguageHandler.instance.currentLanguage == "mr-IN")
				{
					GetComponentInChildren<TextMeshProUGUI>().font = Resources.Load<TMP_FontAsset>(LanguageHandler.FontPath + "rwizenMarathiSDF");
				}
				else if (LanguageHandler.instance.currentLanguage == "or-IN")
				{
					GetComponentInChildren<TextMeshProUGUI>().font = Resources.Load<TMP_FontAsset>(LanguageHandler.FontPath + "rwizenOriyaSDF");
				}
				else if (LanguageHandler.instance.currentLanguage == "ta-IN")
				{
					GetComponentInChildren<TextMeshProUGUI>().font = Resources.Load<TMP_FontAsset>(LanguageHandler.FontPath + "rwizenTamilSDF");
				}
				else if (LanguageHandler.instance.currentLanguage == "te-IN")
				{
					GetComponentInChildren<TextMeshProUGUI>().font = Resources.Load<TMP_FontAsset>(LanguageHandler.FontPath + "rwizenTeluguSDF");
				}
				else
					GetComponentInChildren<TextMeshProUGUI>().font = Resources.Load<TMP_FontAsset>(LanguageHandler.FontPath + "rwizenEnglishSDF");

			}
		} else {
			if (GetComponentInChildren <TextMeshProUGUI> () == null) {
				GetComponentInChildren < Text > ().font = Resources.Load<Font> (LanguageHandler.FontPath + "rwizenArabic");
			} else {           
				GetComponentInChildren <TextMeshProUGUI> ().font = Resources.Load<TMP_FontAsset> (LanguageHandler.FontPath + "rwizenArabicSDF");

			}
		}
	}

	IEnumerator HandleColorTagTextMesh()
    {
       
        if (useColorTag == true)
		{
            Debug.Log("Here");
            string val = GetComponentInChildren<TextMeshProUGUI>().text;
            GetComponentInChildren<TextMeshProUGUI>().text = val.Replace("¾", "=");
            yield return new WaitForSeconds(.1f);
		}
	}
    

    void HandleColorTagText()
	{
		if (useColorTag)
		{
			string val = GetComponentInChildren<TextMeshProUGUI>().text;
			GetComponentInChildren<TextMeshProUGUI>().text = val.Replace("¾", "=");
		}
	}
}