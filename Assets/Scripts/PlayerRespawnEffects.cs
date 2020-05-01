using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawnEffects : MonoBehaviour
{
	public AudioClip DeathClip;
	public AudioClip MoveClip;
	public AudioClip RespawnClip;

	AudioSource audio;

    void OnEnable() {
		audio = GetComponent<AudioSource>();
    }

    public void PlayDeathClip() => PlayClip(DeathClip, false);
    public void PlayMoveClip() => PlayClip(MoveClip, true);
    public void PlayRespawnClip() => PlayClip(RespawnClip, false);

	private void PlayClip(AudioClip clip, bool loop = false, float volume = 1f, float pitch = 1f) {
		audio.Stop();
		audio.time = 0f;
		audio.clip = clip;
		audio.volume = volume;
		audio.pitch = pitch;
		audio.loop = loop;
		audio.Play();
	}
}
