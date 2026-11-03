
using System.Collections;
using UnityEngine;
namespace qwr
{
    public class QwrVolumeTips : MonoBehaviour
    {

        Material material;
        Texture[] textures = new Texture[16];

        public GameObject tips;

        public float duration = 3.0f;

        void Awake()
        {
            if (tips == null)
                return;

            tips.SetActive(false);

            Renderer meshRenderer = tips.GetComponent<Renderer>();
            material = meshRenderer.material;

            for (int i = 0; i < textures.Length; ++i)
            {
                textures[i] = Resources.Load<Texture2D>("QWR/Volume/icon_sound_" + i.ToString());
            }
        }

        void OnVolumeUpdate()
        {
            IsVisible = (true);
        }

        IEnumerator delayHide = null;

        void Hide()
        {
            if (!IsVisible)
                return;

            if (delayHide != null)
            {
                StopCoroutine(delayHide);
                delayHide = null;
            }

            tips.SetActive(false);
        }

        public bool IsVisible
        {
            get
            {
                return (tips != null && tips.activeSelf);
            }
            set
            {
                tips.SetActive(value);

                if (value)
                {
                    if (delayHide != null)
                        StopCoroutine(delayHide);

                    delayHide = DelayHide();
                    StartCoroutine(delayHide);

                    textureIndex = QwrAudioManager.MusicVolume;
                    material.SetTexture("_MainTex", textures[textureIndex]);
                }
                else
                {
                    if (delayHide != null)
                    {
                        StopCoroutine(delayHide);
                        delayHide = null;
                    }
                }
            }
        }

        IEnumerator DelayHide()
        {
            yield return new WaitForSeconds(duration);
            delayHide = null;
            Hide();
        }


        int textureIndex = 0;

        void Update()
        {
            if (!IsVisible)
                return;

            if (textureIndex != QwrAudioManager.MusicVolume)
            {
                textureIndex = QwrAudioManager.MusicVolume;
                material.SetTexture("_MainTex", textures[textureIndex]);
            }
        }


        void OnKeyVolumeUpDown(string param)
        {
            OnVolumeUpdate();
        }

        void OnKeyVolumeDownDown(string param)
        {
            OnVolumeUpdate();
        }
    }
}
