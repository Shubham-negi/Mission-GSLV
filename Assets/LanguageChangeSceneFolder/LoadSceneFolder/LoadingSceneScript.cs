using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneScript : MonoBehaviour
{
	static int loadingSceneIndex;

	public static int LoadingSceneIndex {
		set { 
			loadingSceneIndex = value;
			SceneManager.LoadScene ("LanguageChangeScene");

		}
	}

	//public Text ProgressText;
	public Image LoadingBar;

	AsyncOperation _AO;
	// Use this for initialization
	void Start ()
	{       
		//print ("......." + loadingSceneIndex);
		_AO = SceneManager.LoadSceneAsync (loadingSceneIndex + 1);
		_AO.allowSceneActivation = false;
		StartCoroutine (StartLoadingBar ());
	}

	IEnumerator StartLoadingBar ()
	{
		while (_AO.progress < 0.9f) {
			//ProgressText.text = ((Mathf.Ceil(_AO.progress*10))*10).ToString()+"%";
			LoadingBar.fillAmount = Mathf.Lerp (LoadingBar.fillAmount, _AO.progress, 0.1f);
			yield return new WaitForSeconds (0.1f);
		}

		while (LoadingBar.fillAmount < 1) {
			yield return new WaitForSeconds (0.1f);
			LoadingBar.fillAmount += 0.1f;
		}

		LoadingBar.fillAmount = 1;
		_AO.allowSceneActivation = true;
	}
}
