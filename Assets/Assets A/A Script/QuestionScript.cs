using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionScript : MonoBehaviour {
    int i;
    public GameObject _rightOpt;
    public List<GameObject> _allOptions,_falseObject,_trueObject;


	// Use this for initialization
	void Start () {
		
	}

    public void RightOption()
    {
        SoundManager.sManager.PlayRightSound();
        for (i = 0; i < _allOptions.Count; i++)
        {
            _allOptions[i].GetComponent<Collider>().enabled = false;
        }
        _rightOpt.GetComponent<Image>().color = new Color32(0, 255, 0, 180);

        Invoke("DelayNextStep" , 2.5f);
    }

    public void WrongOption(GameObject opt)
    {
        SoundManager.sManager.PlayWrongSound();
        opt.GetComponent<Collider>().enabled = false;
        opt.GetComponent<Image>().color = new Color32(255, 0, 0, 180);
    }

    void DelayNextStep()
    {
        for (i = 0; i < _falseObject.Count; i++)
        {
            _falseObject[i].SetActive(false);
        }

        for (i = 0; i < _trueObject.Count; i++)
        {
            _trueObject[i].SetActive(true);
        }
    }
}
