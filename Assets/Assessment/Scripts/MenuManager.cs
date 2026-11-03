using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rwizen.Assessment
{
    public class MenuManager : MonoBehaviour
    {

        public AudioClip m_AudioClip;
        public AudioSource m_AudioSource;

        public void LoadAssessment()
        {
            m_AudioSource.PlayOneShot(m_AudioClip);
            Loader.LoadScene("Assessment");
        }

        public void LoadGameplay()
        {
            m_AudioSource.PlayOneShot(m_AudioClip);
            Loader.LoadScene("Level1");
        }


    }

}
