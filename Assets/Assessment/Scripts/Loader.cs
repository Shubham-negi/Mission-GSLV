using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rwizen.Assessment
{
    public class Loader : MonoBehaviour
    {

        [SerializeField] private Image m_LoaderFillImage;
        private AsyncOperation m_AsyncOperation;
        private static string currentSceneToLoad = "MainMenu";


        public static bool initialLoad = true;

        private void Start ()
        {

            if (initialLoad)
            {
               Invoke("LoadNextScene", 0.1f);                
            }    
            
            else
                LoadNextScene();
            
        }

        void LoadNextScene()
        {
            m_AsyncOperation = SceneManager.LoadSceneAsync(currentSceneToLoad);
            m_AsyncOperation.allowSceneActivation = false;
            StartCoroutine(LoadSceneAsyncRoutine());
            initialLoad = false;
        }




        public static void LoadScene(string sceneName)
        {

            currentSceneToLoad = sceneName;
            SceneManager.LoadScene(0);
      
        }

       

        IEnumerator LoadSceneAsyncRoutine()
        {
            while (m_AsyncOperation.progress < 0.9f)
            {
                m_LoaderFillImage.fillAmount = Mathf.Lerp(m_LoaderFillImage.fillAmount, m_AsyncOperation.progress, 0.1f);
                yield return new WaitForSeconds(0.1f);
            }

            while (m_LoaderFillImage.fillAmount < 1)
            {
                yield return new WaitForSeconds(0.1f);
                m_LoaderFillImage.fillAmount += 0.1f;
            }

            m_LoaderFillImage.fillAmount = 1;
            m_AsyncOperation.allowSceneActivation = true;

        }

    }


}
