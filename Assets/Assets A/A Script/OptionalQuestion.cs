using UnityEngine;
using System.Collections.Generic;

public class OptionalQuestion : MonoBehaviour {
    int i;
    public GameObject _rightOpt;
    public List<GameObject> _allOptions,_instantFalse,_falseObject,_trueObject;

	void Start () {
		
	}

    public void RightOption()
    {
        SoundManager.sManager.PlayRightSound();
        for (i = 0; i < _allOptions.Count; i++)
        {
            _allOptions[i].GetComponent<Collider>().enabled = false;
        }
        for (i = 0; i < _instantFalse.Count; i++)
        {
            _instantFalse[i].SetActive(false);
        }
        Invoke("DelayNextStep" , 2.5f);
    }

    public void WrongOption(GameObject opt)
    {
        SoundManager.sManager.PlayWrongSound();
        opt.GetComponent<Collider>().enabled = false;
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
