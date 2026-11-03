using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.IO;

public class LanguageSelectionManager : MonoBehaviour
{
    private static LanguageSelectionManager instance;
    public static LanguageSelectionManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<LanguageSelectionManager>();
            return instance;
        }
    }
    public GameObject Languagebutton;
    private GameObject default_LangButton;
    List<GameObject> buttons = new List<GameObject>();

    void Awake ()
    {
    }
    void Start()
    {
        Languagebutton.name = LanguageHandler.instance.Languages[0].DisplayName;
        Languagebutton.GetComponentInChildren<Text>().text = LanguageHandler.instance.Languages[0].DisplayName;
        buttons.Add(Languagebutton);

        for (int i = 1; i < LanguageHandler.instance.Languages.Count; i++)
        {
            GameObject temp = Instantiate(Languagebutton, Languagebutton.transform.parent) as GameObject;
            temp.name = LanguageHandler.instance.Languages[i].DisplayName;
            temp.GetComponentInChildren<Text>().text = LanguageHandler.instance.Languages[i].DisplayName;
            buttons.Add(temp);
        }

        buttons[LanguageHandler.instance.CurrentLanguageIndex].GetComponentInChildren<Text>().color = new Color32(105, 223, 0, 255);
        buttons[LanguageHandler.instance.CurrentLanguageIndex].GetComponent<EventTrigger>().enabled = false;
        default_LangButton = buttons[0] ;    
    }

    GameObject ClickedButton ;
    public void OnClickLanguageButton(GameObject Button)
    {
        ClickedButton = Button;
        PlayerPrefs.SetString("currentLanguage", LanguageHandler.instance.Languages[ClickedButton.transform.GetSiblingIndex()].LanguageID);
        // try to fetch the file from path
		StartCoroutine("DelayChangeButtonFunctions");
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
            ClickedButton.transform.GetChild(0).GetComponentInChildren<Text>().color = new Color32(105, 223, 0, 255);
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
            buttons[i].GetComponentInChildren<Text>().color = new Color32(233, 233, 233, 255);
            buttons[i].GetComponent<EventTrigger>().enabled = true;
        }
    }

    public void OnOffTable(GameObject Table)
    {
        Table.SetActive(!Table.activeInHierarchy);
		//SoundManager.Instance.PlayClickSound();
    }

    public void TableDisable(GameObject obj)
    {
        obj.SetActive(false);
		//SoundManager.Instance.PlayClickSound();
    }
}
