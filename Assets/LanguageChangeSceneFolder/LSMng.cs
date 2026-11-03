using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.IO;

public class LSMng : MonoBehaviour
{
	private static LSMng instance;
	public static LSMng Instance
    {
        get
        {
            if (instance == null)
				instance = FindObjectOfType<LSMng>();
            return instance;
        }
    }
   // public GameObject Languagebutton;
    private GameObject default_LangButton;
	public  List<GameObject> buttons = new List<GameObject>();

	public int LanguageIndex;

    void Start()
    {
//		Languagebutton.name = LanguageHandler.instance.Languages[0].DisplayName;
//		Languagebutton.GetComponentInChildren<Text>().text = LanguageHandler.instance.Languages[0].DisplayName;
//        buttons.Add(Languagebutton);
//
//		for (int i = 1; i < LanguageHandler.instance.Languages.Count; i++)
//        {
//            GameObject temp = Instantiate(Languagebutton, Languagebutton.transform.parent) as GameObject;
//			temp.name = LanguageHandler.instance.Languages[i].DisplayName;
//			temp.GetComponentInChildren<Text>().text = LanguageHandler.instance.Languages[i].DisplayName;
//            buttons.Add(temp);
//        }
//
		buttons[LanguageHandler.instance.CurrentLanguageIndex].GetComponent<Image>().color = new Color32(141, 141, 141, 255);
		buttons[LanguageHandler.instance.CurrentLanguageIndex].GetComponent<EventTrigger>().enabled = false;
        default_LangButton = buttons[0] ;    
    }

    GameObject ClickedButton ;
    public void OnClickLanguageButton(GameObject Button)
    {
        ClickedButton = Button;
		PlayerPrefs.SetString("currentLanguage", LanguageHandler.instance.Languages[LanguageIndex].LanguageID);
        // try to fetch the file from path
		StartCoroutine("DelayChangeButtonFunctions");
    }

	public void AssignIndex(int myIndex)
	{
		LanguageIndex = myIndex;
	}

	// Call in choose default language method
    public void SetDefaultSettings()
    {
        ClickedButton = default_LangButton;
        StartCoroutine("DelayChangeButtonFunctions");
    }
    public IEnumerator DelayChangeButtonFunctions() 
	{
        makeDefaultcolor();
        if (ClickedButton != null)
        {
			ClickedButton.GetComponent<Image>().color = new Color32(141, 141, 141, 255);
            ClickedButton.GetComponent<EventTrigger>().enabled = false;    
        }

		LanguageHandler.instance.setCurrentLanguage();
		LanguageHandler.instance.changeLanguageUpdate();
        yield return null;
    }
    void makeDefaultcolor()
    {
        if (buttons.Count <= 0)
            return;
        for (int i = 0; i < buttons.Count; i++)
        {
			buttons[i].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            buttons[i].GetComponent<EventTrigger>().enabled = true;
        }
    }
}