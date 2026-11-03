using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LocaliseTextAndVoiceOver))]
public class LocaliseFontSizeChanger : MonoBehaviour 
{
    public int _FontSize;
    public int UK_FontSizeDifference;
    public int US_FontSizeDifference;
    public int Arabic_FontSizeDifference;
    public int Chinese_FontSizeDifference;

    bool IsTextMesh;

	void Awake () 
    {
        if (GetComponentInChildren<Text>() != null)
        {
            _FontSize = GetComponentInChildren<Text>().fontSize;
            IsTextMesh = false;
        }
        else if (GetComponentInChildren<TextMesh>() != null)
        {
            _FontSize = GetComponentInChildren<TextMesh>().fontSize;
            IsTextMesh = true;
        }

        LanguageHandler.LanguageChangeEventFire += ChangeFontSize;
	}

    void OnEnable()
    {
        ChangeFontSize();
    }

    void OnDisable()
    {
        LanguageHandler.LanguageChangeEventFire -= ChangeFontSize;
    }

    void ChangeFontSize()
    {
        if (!IsTextMesh)
        {
            if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID == "en-GB")
                GetComponentInChildren<Text>().fontSize = _FontSize + UK_FontSizeDifference;
            else if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID == "en-US")
                GetComponentInChildren<Text>().fontSize = _FontSize + US_FontSizeDifference;
            else if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID == "ar")
                GetComponentInChildren<Text>().fontSize = _FontSize + Arabic_FontSizeDifference;
            else if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID == "zh-CN")
                GetComponentInChildren<Text>().fontSize = _FontSize + Chinese_FontSizeDifference;
        }
        else
        {
            if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID == "en-GB")
                GetComponentInChildren<TextMesh>().fontSize = _FontSize + UK_FontSizeDifference;
            else if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID == "en-US")
                GetComponentInChildren<TextMesh>().fontSize = _FontSize + US_FontSizeDifference;
            else if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID == "ar")
                GetComponentInChildren<TextMesh>().fontSize = _FontSize + Arabic_FontSizeDifference;
            else if (LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].LanguageID == "zh-CN")
                GetComponentInChildren<TextMesh>().fontSize = _FontSize + Chinese_FontSizeDifference;
        }
    }
}
