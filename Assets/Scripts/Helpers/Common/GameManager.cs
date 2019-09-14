using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

	private float _timeScale = 1f;
	public float TimeScale {
		get {
			return _timeScale;
		}
		set {
			Time.timeScale = value;
			_timeScale = value;
		}
	}

	public ContextManager ContextManager;
	public LevelManager LevelManager {
		get
		{
			return ContextManager as LevelManager;
		}
		set
		{
			ContextManager = value;
		}
	}

	public static readonly int MenuBuildIndex = 0;
	public static readonly int LevelSelectBuildIndex = 1;
	public static readonly int LoadSceneBuildIndex = 2;

	private LoadScreen loadScreen;
	
	// public PlayerData PlayerData { get; set; }

	private bool InProgressSceneSwitch = false;

	public void Awake() {
		// TODO: Load saved PlayerData
		// PlayerData = new PlayerData(2f, 1.25f, 0.2f);
		SceneManager.LoadSceneAsync(LoadSceneBuildIndex, LoadSceneMode.Additive);
	}

	public void HandleInput(InputPackage p) {
		ContextManager.HandleInput(p);
	}

	public void ReloadLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


	public void ToggleSoundOn() {
		
	}

	public int GetNextLevelBuildIndex() {
		return GetCurrentLevelBuildIndex() + 1;
	}

	public int GetCurrentLevelBuildIndex() {
		var buildIndex = 0;
		for(int i = 0; i < SceneManager.sceneCount; i++) {
			Scene s = SceneManager.GetSceneAt(i);
			if (s.isLoaded && s.buildIndex != LoadSceneBuildIndex) {
				buildIndex = s.buildIndex;
				break;
			}
		}
		return buildIndex;
	}

	private IEnumerator LoadSceneAsync(int buildIndex, Coroutine waitUntil = null, CancellationTokenSource cts = null, Action onSceneSwitch = null, bool shouldUnloadCurrentScene = true) {
		InProgressSceneSwitch = true;
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

		asyncLoad.allowSceneActivation = false;

		if(waitUntil != null) {
			yield return waitUntil;
		}
		yield return new WaitUntil(() => asyncLoad.progress >= 0.9f); //when allowsceneactive is false, progress stops at .9f

		if(cts != null && cts.IsCancellationRequested) {
			ContextManager tContext = ContextManager;
			asyncLoad.allowSceneActivation = true;
			yield return new WaitUntil(() => asyncLoad.isDone);
			SceneManager.UnloadSceneAsync(buildIndex);
			ContextManager = tContext;
		}
		else {
			Scene currentScene = SceneManager.GetActiveScene();
			asyncLoad.allowSceneActivation = true;
			onSceneSwitch?.Invoke();
			if(shouldUnloadCurrentScene) {
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

	public void LoadScene(int buildIndex, Coroutine waitUntil) {
		ShowLoadScreen(true);
		StartCoroutine(LoadSceneAsync(buildIndex, waitUntil, null, () => ShowLoadScreen(false)));	
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
}