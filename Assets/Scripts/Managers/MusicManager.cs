using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MusicManager : Singleton<MusicManager> {

	private AudioSource audio;
	private float Volume;

    void Start() {
		Init();
    }

	private void Init() {
		audio = audio ?? gameObject.AddComponent<AudioSource>();
	}

	public void SetVolume(float volume) {
		Volume = volume * GameManager.Instance.SaveData.MusicVolume;
		audio.volume = volume;
	}

	public void SetTrack(AudioClip track) {
		audio.clip = track;
	}

	public void LoadMusicForWorldAndChangeTrack(int world, float transitionTime = 1f) {
		Addressables.LoadAssetAsync<CopyMusicObject>($"World{world}/Music").Completed +=
			(obj) => {
				ChangeTrack(obj.Result.Track, transitionTime);
			};
	}

	public void ChangeTrack(AudioClip track, float transitionTime = -1f) {
		if(audio == null) Init();
		if(audio.clip.name == track.name) return;

		if(audio.clip == null || transitionTime < 0f) {
			SetTrack(track);
		}
		else {
			StartCoroutine(TransitionTrack(track, transitionTime));
		}
	}

	private IEnumerator TransitionTrack(AudioClip track, float time) {
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

		SetTrack(track);

		elapsedTime = 0f;
		startVolume = 0f;
		endVolume = 1f;
		while (elapsedTime < animationTime) {
			changeVolumeLoop(elapsedTime / animationTime);
			yield return null;
		}
		SetVolume(1f);
	}
}
