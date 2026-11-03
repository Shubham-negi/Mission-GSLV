using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextEffect : MonoBehaviour {
	public string InstTextMessage;
	[Range(0.01f,0.2f)]
	public float DelayForNextWord=0.1f;

	string Word_Text_Msg;
	float CalculateTime;
	Text HoldText;

	void OnEnable(){
		HoldText = GetComponent<Text> ();
        HoldText.text = null;
        Word_Text_Msg = "";
        CalculateTime = 0;
    }

	void Start () {

	}

	void Update () {
		CalculateTime += Time.deltaTime;
		while(CalculateTime>=DelayForNextWord && Word_Text_Msg.Length<InstTextMessage.Length){
			Word_Text_Msg += InstTextMessage[Word_Text_Msg.Length];
			CalculateTime -= DelayForNextWord;
		}
		HoldText.text = Word_Text_Msg;
	}
}