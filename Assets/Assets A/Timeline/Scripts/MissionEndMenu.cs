using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GslvMission
{
    /// <summary>
    /// The menu shown when the mission is complete. RESTART loads this scene again, so everything starts from its
    /// first state (nothing is carried over or duplicated). EXIT leaves through the app's Exit Menu when the scene has one
    /// (ExitMenu.GOHome), otherwise it quits; in the Editor it stops Play mode. Each button acts once.
    /// </summary>
    public class MissionEndMenu : MonoBehaviour
    {
        public Button restartButton, exitButton;

        bool busy;

        void Awake()
        {
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (exitButton != null) exitButton.onClick.AddListener(Exit);
        }

        public void Restart()
        {
            if (busy) return;
            busy = true;
            var scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
            if (scene.buildIndex < 0)   // not in Build Settings: the Editor can still load it by path
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }

        public void Exit()
        {
            if (busy) return;
            busy = true;
            if (ExitMenu.instance != null)
            {
                try
                {
                    ExitMenu.instance.GOHome();
#if UNITY_EDITOR
                    Quit();   // Application.Quit does nothing in the Editor
#endif
                }
                catch (Exception e)   // e.g. the app's SoundManager is not in this scene
                {
                    Debug.LogWarning("[Mission] Exit Menu could not quit (" + e.Message + "); quitting directly.");
                    Quit();
                }
                return;
            }
            Quit();
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
