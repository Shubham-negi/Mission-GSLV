using UnityEngine;
using System.Collections;


namespace QWR.Rwizen.Demo
{
	public class SoundManager : MonoBehaviour
	{
		public static SoundManager sManager;
		public AudioClip cSound, rSound, wSound;
		AudioSource audioSource;

		void Start()
		{
			sManager = this;
			audioSource = GetComponent<AudioSource>();
		}

		public void PlayClickSound()
		{
			audioSource.clip = cSound;
			audioSource.PlayOneShot(cSound);

		}

		public void PlayRightSound()
		{
			audioSource.clip = rSound;
			audioSource.PlayOneShot(rSound);

		}

		public void PlayWrongSound()
		{
			audioSource.PlayOneShot(wSound);

		}
	}
}