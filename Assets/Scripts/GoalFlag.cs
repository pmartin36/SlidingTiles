using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.NiceVibrations;

public class GoalFlag : MonoBehaviour
{
    public SpriteRenderer Flag;
	public ParticleSystem particles;
	public AudioSource audio;

	private Coroutine reachedCoroutine;

	public void Start() {
		particles = GetComponentInChildren<ParticleSystem>(true);
		audio = GetComponentInChildren<AudioSource>(true);
	}

	public void PlayerReached() {
		reachedCoroutine = StartCoroutine("PlayerReachedFireworks");
	}

	public void StopParticlesAndHaptics() {
		particles.gameObject.SetActive(false);
		if (reachedCoroutine != null) {
			StopCoroutine(reachedCoroutine);
			reachedCoroutine = null;
		}
	}

	public void Reset() {
		StopParticlesAndHaptics();
		audio.volume = 0f;
	}

	public void SetAudioVolume(float vol) {
		audio.volume = vol * 0.75f * GameManager.Instance.SaveData.FxVolume;
	}

	private IEnumerator PlayerReachedFireworks() {
		particles.gameObject.SetActive(true);
		audio.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.75f);
		while(true) {
			MMVibrationManager.Haptic(HapticTypes.LightImpact);
			yield return new WaitForSeconds(UnityEngine.Random.value * 0.95f + 0.05f);
		}
	}
}
