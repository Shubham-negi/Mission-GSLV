using UnityEngine;
using TMPro;

public class ChangeTextMeshHindi : MonoBehaviour
{

    void Start()
    {
        string text = gameObject.GetComponent<TextMeshProUGUI>().text; // Getting TMPro Component

        gameObject.GetComponent<TextMeshProUGUI>().SetHindiTMPro(text);
    }

    void Update()
    {

    }
}
