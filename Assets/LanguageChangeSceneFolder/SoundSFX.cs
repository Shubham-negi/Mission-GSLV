using UnityEngine;
using System.Collections;

public class SoundSFX : MonoBehaviour
{

	public static SoundSFX sm;

	public AudioClip ClickSound;

	public AudioSource asc;


	void Awake ()
	{
		sm = this;
		asc = GetComponent<AudioSource> ();
		asc.clip = null;
	}

	public void PlayClickSound ()
	{
		asc.clip = ClickSound;
		sm.asc.PlayOneShot (ClickSound);

	}
}