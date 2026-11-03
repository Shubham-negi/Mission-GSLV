using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
public class LanguagePanel
{
    public LanguageID LanguageSelect;
    public List<GameObject> Panel_Enabled;
}

public class LanguageBasedPanel : MonoBehaviour
{
    [SerializeField] public List<LanguagePanel> languagePanel;

    // Start is called before the first frame update
    void Start()
    {
        var panels = languagePanel.Find(x => x.LanguageSelect == LanguageHandler.instance.Languages[LanguageHandler.instance.CurrentLanguageIndex].Language_ID);

        foreach (var panel in panels.Panel_Enabled)
        {
            panel.SetActive(true);
        }
    }

}
