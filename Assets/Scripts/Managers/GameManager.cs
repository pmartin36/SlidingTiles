using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

public class GameManager : Singleton<GameManager> {

	private float _timeScale = 1f;
	private float _targetTimeScale = 1f;
	private float timeLerpScale;
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

	private LoadScreen loadScreen;

	public StoreCommunicator StoreCommunicator { get; set; }
	public AdvertisementManager AdManager { get; set; }
	public bool IsMobilePlatform { get; set; }
	public SaveData SaveData { get; private set; }

	public const int AvailableWorlds = 4;
	public int HighestUnlockedLevel => SaveData.HighestUnlockedLevel;
	public int LastPlayedWorld => SaveData.LastPlayedWorld;

	public void Awake() {
		// TODO: Load saved PlayerData
		// PlayerData = new PlayerData(2f, 1.25f, 0.2f);
		ContextManager = GameObject.FindObjectOfType<ContextManager>();
		SceneManager.LoadSceneAsync(SceneHelpers.LoadSceneBuildIndex, LoadSceneMode.Additive);

		IsMobilePlatform = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
		StoreCommunicator = StoreCommunicator.StoreCommunicatorFactory();
		Load();

		AdManager = new AdvertisementManager();
	}

	public void Update() {
		TimeScale = Mathf.Lerp(TimeScale, _targetTimeScale, timeLerpScale);
	}

	public void HandleInput(InputPackage p) {
		ContextManager.HandleInput(p);
	}

	public void ReloadLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


	public void RemoveAds() {
		SaveData.AdsRemoved = true;
		Save();

		MenuManager mm = (ContextManager as MenuManager);
		if(mm != null) {
			mm.SettingsMenu.HideAdRemovalWidget();
		}
	}

	public void ShowAd() => AdManager.TryShowAd();

	public void SetTimescale(float timescale, float lerpScale = 0.5f) {
		_targetTimeScale = timescale;
		timeLerpScale = lerpScale;
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
			int currentScene = SceneHelpers.GetCurrentLevelBuildIndex();

			asyncLoad.allowSceneActivation = true;
			yield return new WaitUntil(() => asyncLoad.isDone);

			ContextManager = GameObject.FindObjectsOfType<ContextManager>().First(g => g.gameObject.scene.buildIndex == buildIndex);
			yield return new WaitUntil(() => ContextManager.ResourcesLoaded);
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

	public void ShowLoadScreen(bool show, bool showLoadTiles = true) {
		if(loadScreen == null) {
			loadScreen = FindObjectOfType<LoadScreen>();
		}

		loadScreen.Show(show, showLoadTiles);
	}

	public IEnumerator LoadWrapper(Coroutine waitUntil = null) {
		yield return new WaitForSeconds(1f);
		if(waitUntil != null) {
			yield return waitUntil;
		}
	}

	public void SaveLevelCompleteData(int level) {
		SaveData.HighestUnlockedLevel = Mathf.Max(level, HighestUnlockedLevel);
		Save();
	}

	public void AdjustAudio(SoundType t, float value) {
		if(t == SoundType.SFX) {
			SaveData.FxVolume = value;
		}
		else {
			SaveData.MusicVolume = value;
			MusicManager.Instance.GlobalMusicVolumeAdjusted();
		}
	}

	public void SetShowMilliseconds(bool val) {
		SaveData.ShowTimer = val;
	}

	public void Save() {
		SaveData.SaveTime = DateTime.UtcNow;
		string json = JsonConvert.SerializeObject(SaveData);
		StoreCommunicator.AddSaveData(json);

		string path = Path.Combine(Application.persistentDataPath, "data.json");
		File.WriteAllText(path, json);
	}

	public void Load() {
		bool localExists = false;
		string path = Path.Combine(Application.persistentDataPath, "data.json");
		if(File.Exists(path)) {
			string json = File.ReadAllText(path);
			SaveData = JsonConvert.DeserializeObject<SaveData>(json);

			// TODO: Delete before release
			if(DateTime.Now.Subtract(SaveData.SaveTime).Days > 30) {
				SaveData = new SaveData();
			}
			localExists = true;
		}
		else {
			SaveData = new SaveData();
		}

		StartCoroutine(TryLoadSaveData(localExists));
	}

	private IEnumerator TryLoadSaveData(bool localSaveExists) {
		yield return new WaitUntil(() => StoreCommunicator.IsInitialized);
		StoreCommunicator.TryLoadSaveData((string result) => {
			SaveData sd = JsonConvert.DeserializeObject<SaveData>(result);
			if (sd != null && (!localSaveExists || SaveData.SaveTime.Ticks - sd.SaveTime.Ticks < 60)) {
				SaveData = sd;
				if (ContextManager != null && ContextManager is MenuManager) {
					(ContextManager as MenuManager).ReceivedNewSaveData();
				}
			}
		});
	}
}