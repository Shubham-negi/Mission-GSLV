using UnityEngine;
using DG.Tweening;

public class AnimationEvent : MonoBehaviour {
	public Animation Anim;
	public string AnimName;

	void Start () {
        Anim.Play();
		Anim[AnimName].speed = 0;
	}

	void SpeedZero () {
		Anim[AnimName].speed = 0;
	}
}