using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawnEffects : MonoBehaviour
{
	public AudioClip DeathClip;
	public AudioClip MoveClip;
	public AudioClip RespawnClip;

	AudioSource audioPrimary;
	AudioSource audioSecondary;

    void OnEnable() {
		audioPrimary = GetComponent<AudioSource>();
		audioSecondary = this.gameObject.AddComponent<AudioSource>();
		audioSecondary.playOnAwake = false;
    }

    public void PlayDeathClip() => PlayClip(DeathClip, true, false, volume: 0.75f);
    public void PlayMoveClip() => PlayClip(MoveClip, false, false, volume: 0f, pitch: 1.2f);
    public void PlayRespawnClip() => PlayClip(RespawnClip, true, false, volume: 0.75f);

	public float MoveClipVolume {
		get => audioSecondary.volume;
		set => audioSecondary.volume = value;
	}

	public float DeathClipVolume {
		get => audioPrimary.volume;
		set => audioPrimary.volume = value;
	}

	private void PlayClip(AudioClip clip, bool playOnPrimary = true, bool loop = false, float volume = 1f, float pitch = 1f) {
		AudioSource audio = playOnPrimary ? audioPrimary : audioSecondary;
		audio.Stop();
		audio.time = 0f;
		audio.clip = clip;
		audio.volume = volume * GameManager.Instance.SaveData.FxVolume;
		audio.pitch = pitch;
		audio.loop = loop;
		audio.Play();
	}
}
