using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalFlag : MonoBehaviour
{
    public SpriteRenderer Flag;
	public ParticleSystem particles;
	public AudioSource audio;

	public void Start() {
		particles = GetComponentInChildren<ParticleSystem>(true);
		audio = GetComponentInChildren<AudioSource>(true);
	}

	public void PlayerReached() {
		particles.gameObject.SetActive(true);
	}

	public void Reset() {
		particles.gameObject.SetActive(false);
		audio.volume = 0f;
	}

	public void SetAudioVolume(float vol) {
		audio.volume = vol * GameManager.Instance.SaveData.FxVolume / 2f;
	}
}
