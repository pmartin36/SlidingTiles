﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

	private float _timeScale = 1f;
	private float _targetTimeScale = 1f;
	private float TimeScale {
		get => _timeScale;
		set {
			Time.timeScale = value;
			_timeScale = value;
		}
	}

	public ContextManager ContextManager;
	public LevelManager LevelManager {
		get => ContextManager as LevelManager;
		set => ContextManager = value;
	}
	public MenuManager MenuManager {
		get => ContextManager as MenuManager;
		set => ContextManager = value;
	}

	public static readonly int MenuBuildIndex = 0;
	public static readonly int LoadSceneBuildIndex = 1;
	public static readonly int TutorialLevelStart = 2;

	private LoadScreen loadScreen;

	public StoreCommunicator StoreCommunicator { get; set; }
	public bool IsMobilePlatform { get; set; }
	public SaveData SaveData { get; private set; }
	public int HighestOwnedWorld => SaveData.HighestOwnedWorld;
	public int HighestUnlockedLevel => SaveData.HighestUnlockedLevel;

	public void Awake() {
		// TODO: Load saved PlayerData
		// PlayerData = new PlayerData(2f, 1.25f, 0.2f);
		ContextManager = GameObject.FindObjectOfType<ContextManager>();
		SceneManager.LoadSceneAsync(LoadSceneBuildIndex, LoadSceneMode.Additive);

		StoreCommunicator = StoreCommunicator.StoreCommunicatorFactory();
		IsMobilePlatform = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;

		bool retrievedData = StoreCommunicator.TryLoadSaveData(out string jsonString);
		if(retrievedData) {
			SaveData = JsonUtility.FromJson<SaveData>(jsonString);
		}
		else {
			SaveData = new SaveData();
		}
	}

	public void Update() {
		TimeScale = Mathf.Lerp(TimeScale, _targetTimeScale, 0.05f);
	}

	public void HandleInput(InputPackage p) {
		ContextManager.HandleInput(p);
	}

	public void ReloadLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


	public void ToggleSoundOn() {
		
	}

	public void SetTimescale(float timescale) {
		_targetTimeScale = timescale;
	}

	public int GetNextLevelBuildIndex() {
		return GetCurrentLevelBuildIndex() + 1;
	}

	public int GetCurrentLevelBuildIndex() {
		var buildIndex = 0;
		for(int i = 0; i < SceneManager.sceneCount; i++) {
			Scene s = SceneManager.GetSceneAt(i);
			if (s.isLoaded && s.buildIndex != LoadSceneBuildIndex) {
				return s.buildIndex;
			}
		}
		return buildIndex;
	}

	public string GetSceneName() {
		for (int i = 0; i < SceneManager.sceneCount; i++) {
			Scene s = SceneManager.GetSceneAt(i);
			if (s.buildIndex != LoadSceneBuildIndex) {
				return s.name;
			}
		}
		return "";
	}

	public int GetBuildIndexFromLevel(int world, int level) {
		return 
			TutorialLevelStart + 
			2 + // two tutorial levels
			(world-1) * 10 + // each world has 10 levels
			(level - 1); // 0 indexed levels
	}

	private IEnumerator LoadSceneAsync(int buildIndex, Coroutine waitUntil = null, CancellationTokenSource cts = null, Action onSceneSwitch = null, bool shouldUnloadCurrentScene = true) {
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

		asyncLoad.allowSceneActivation = false;

		if(waitUntil != null) {
			yield return waitUntil;
		}
		yield return new WaitUntil(() => asyncLoad.progress >= 0.9f); //when allowsceneactive is false, progress stops at .9f

		if(cts != null && cts.IsCancellationRequested) {
			asyncLoad.allowSceneActivation = true;
			yield return new WaitUntil(() => asyncLoad.isDone);
			SceneManager.UnloadSceneAsync(buildIndex);
		}
		else {
			int currentScene = GetCurrentLevelBuildIndex();

			asyncLoad.allowSceneActivation = true;
			yield return new WaitUntil(() => asyncLoad.isDone);

			ContextManager = GameObject.FindObjectsOfType<ContextManager>().First(g => g.gameObject.scene.buildIndex == buildIndex);
			onSceneSwitch?.Invoke();

			if (shouldUnloadCurrentScene) {
				SceneManager.UnloadSceneAsync(currentScene);
			}
		}
	}

	public void AsyncLoadScene(int buildIndex, Coroutine waitUntil = null, CancellationTokenSource cts = null, Action onSceneSwitch = null, bool shouldUnloadCurrentScene = true) {
		StartCoroutine(
			LoadSceneAsync(
				buildIndex,
				waitUntil,
				cts,
				onSceneSwitch,
				shouldUnloadCurrentScene
			)
		);
	}	

	public void LoadScene(int buildIndex, Coroutine waitUntil = null, Action onSceneSwitch = null) {
		ShowLoadScreen(true);
		StartCoroutine(
			LoadSceneAsync(
				buildIndex, 
				StartCoroutine(LoadWrapper(waitUntil)), // wait for at least 1 seconds + waituntil
				null, 
				() => {
					onSceneSwitch?.Invoke();		
					ShowLoadScreen(false);
				}
			)
		);	
	}

	public void UnloadScene(int buildIndex, Action<AsyncOperation> callback = null) {
		AsyncOperation unload = SceneManager.UnloadSceneAsync(buildIndex);
		if(callback != null) {
			unload.completed += callback;
		}
	}

	public void ShowLoadScreen(bool show) {
		if(loadScreen == null) {
			loadScreen = FindObjectOfType<LoadScreen>();
		}

		loadScreen.Show(show);
	}

	public IEnumerator LoadWrapper(Coroutine waitUntil = null) {
		yield return new WaitForSeconds(1f);
		if(waitUntil != null) {
			yield return waitUntil;
		}
	}

	public void SetHighestUnlockedLevel(int level) {
		SaveData.HighestUnlockedLevel = Mathf.Max(level, HighestUnlockedLevel);
		Save();
	}

	public void AdjustAudio(SoundType t, float value) {
		if(t == SoundType.VFX) {
			SaveData.FxVolume = value;
		}
		else {
			SaveData.MusicVolume = value;
		}
	}

	public void Save() {
		string json = JsonUtility.ToJson(SaveData);
		StoreCommunicator.AddSaveData(json);
	}
}