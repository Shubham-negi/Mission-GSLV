using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public List<string> _sceneName;
	static int loadingSceneIndex;
    static string loadingSceneName= "LanguageChangeScene";

    public static string LoadingSceneName
    {
        set
        {
            loadingSceneName = value;
            SceneManager.LoadScene("LoadingScene");
        }
    }

    //public Text ProgressText;
    public Image LoadingBar;

	AsyncOperation _AO;
	// Use this for initialization
	void Start ()
	{
        for(int i = 0; i < _sceneName.Count; i++)
        {
            if(_sceneName[i]== loadingSceneName)
            {
                _AO = SceneManager.LoadSceneAsync(i);
                _AO.allowSceneActivation = false;
            }
        }
		StartCoroutine (StartLoadingBar ());
	}

	IEnumerator StartLoadingBar ()
	{
		while (_AO.progress < 0.9f) {
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
