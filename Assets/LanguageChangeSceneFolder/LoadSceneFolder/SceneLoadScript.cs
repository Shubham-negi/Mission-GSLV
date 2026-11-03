using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadScript : MonoBehaviour 
{
	public static SceneLoadScript instance;

	void Start(){
		instance = this;
	}

    public void SceneLoad(string _sceneName)
    {
        LoadingScene.LoadingSceneName = _sceneName;
    }
}