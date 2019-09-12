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
		SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
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
		var currentScene = SceneManager.GetActiveScene();
		return currentScene.buildIndex + 1;
	}

	public IEnumerator LoadSceneAsync(int buildIndex, Coroutine waitUntil = null, CancellationTokenSource cts = null, Action onSceneSwitch = null) {
		InProgressSceneSwitch = true;
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

		asyncLoad.allowSceneActivation = false;

		if(waitUntil != null) {
			yield return waitUntil;
		}
		yield return new WaitUntil(() => asyncLoad.progress >= 0.9f); //when allowsceneactive is false, progress stops at .9f

		if(cts != null && cts.IsCancellationRequested) {
			SceneManager.UnloadSceneAsync(buildIndex);
		}
		else {
			Scene currentScene = SceneManager.GetActiveScene();
			asyncLoad.allowSceneActivation = true;
			onSceneSwitch?.Invoke();
			SceneManager.UnloadSceneAsync(currentScene);		
		}
	}

	public void LoadScene(int buildIndex, Coroutine waitUntil) {
		ShowLoadScreen(true);
		StartCoroutine(LoadSceneAsync(buildIndex, waitUntil, null, () => ShowLoadScreen(false)));	
	}

	public void ShowLoadScreen(bool show) {
		if(loadScreen == null) {
			loadScreen = FindObjectOfType<LoadScreen>();
		}

		loadScreen.Show(show);
	}
}