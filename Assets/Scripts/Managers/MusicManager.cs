using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MusicManager : Singleton<MusicManager> {

	private AudioSource audio;
	private float Volume;

	private int LastRequestedWorldMusic = -1;
	private Coroutine transitionTrackCoroutine;

    void Start() {
		Init();
    }

	private void Init() {
		audio = audio ?? gameObject.AddComponent<AudioSource>();
		audio.loop = true;
	}

	public void SetVolume(float volume) {
		Volume = volume;
		audio.volume = volume * GameManager.Instance.SaveData.MusicVolume;
	}

	public void SlideVolume(float volume, float t) {
		Volume = volume;
		StartCoroutine(SlideVolumeRoutine(Volume * GameManager.Instance.SaveData.MusicVolume, t));
	}

	public void SetTrack(AudioClip track) {
		audio.clip = track;
	}

	public void GlobalMusicVolumeAdjusted() {
		SetVolume(Volume);
	}

	public void LoadMusicForWorldAndChangeTrack(int world, float transitionTime = 1f, float finalVolume = -1, bool initial = false) {
		if(finalVolume < 0) finalVolume = Volume;
		LastRequestedWorldMusic = world;
		Addressables.LoadAssetAsync<AudioClip>($"World{world}/Music").Completed +=
			(obj) => {
				if(LastRequestedWorldMusic == world) {
					ChangeTrack(obj.Result, transitionTime, finalVolume, initial);
				}
			};
	}

	public void ChangeTrack(AudioClip track, float transitionTime = -1f, float finalVolume = 1f, bool initial = false) {
		if(audio == null) Init();
		if(audio.clip != null && audio.clip.name == track.name) {
			if(Volume != finalVolume) {
				SlideVolume(finalVolume, transitionTime / 2f);
			}
			return;
		};

		if(transitionTime < 0f) {
			SetTrack(track);
			SetVolume(finalVolume);
			audio.Play();
		}
		else if(audio.clip == null && !initial) {
			// don't allow initial world change to set track, it's set from splash screen
			SetTrack(track);
			Volume = 0f;
			SlideVolume(finalVolume, transitionTime);
			audio.Play();
		}
		else {
			if(transitionTrackCoroutine != null) {
				StopCoroutine(transitionTrackCoroutine);
			}
			transitionTrackCoroutine = StartCoroutine(TransitionTrack(track, transitionTime, finalVolume));
		}
	}

	private IEnumerator SlideVolumeRoutine(float v, float t) {
		float startVolume = audio.volume;
		float elapsed = 0f;
		while(elapsed < t) {
			audio.volume = Mathf.Lerp(startVolume, v, elapsed / t);
			elapsed += Time.deltaTime;
			yield return null;
		}
		audio.volume = v;
	}

	private IEnumerator TransitionTrack(AudioClip track, float time, float finalVolume) {
		float animationTime = time / 2f;
		float elapsedTime = 0f;
		float startVolume = audio.volume / GameManager.Instance.SaveData.MusicVolume;
		float endVolume = 0f;

		System.Action<float> changeVolumeLoop = (t) => {
			float v = Mathf.Lerp(startVolume, endVolume, t);
			SetVolume(v);
			elapsedTime += Time.deltaTime;
		};

		while( elapsedTime < animationTime ) {
			changeVolumeLoop(elapsedTime/animationTime);
			yield return null;
		}
		SetVolume(0f);
		yield return new WaitForSeconds(0.5f);
		SetTrack(track);
		audio.Play();

		elapsedTime = 0f;
		startVolume = 0f;
		endVolume = finalVolume;
		while (elapsedTime < animationTime) {
			changeVolumeLoop(elapsedTime / animationTime);
			yield return null;
		}
		SetVolume(finalVolume);
	}
}
