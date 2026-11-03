using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour 
{
	public static SceneLoader instance;

	void Start(){
		instance = this;
	}

	public void LoadScene(int BuildIndex)
	{
		
		//LoadingScene.LoadingSceneIndex = BuildIndex;
	}

	public void Quit()
	{
		
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        Application.Quit();
		Debug.Log("Quit");
	}
}
