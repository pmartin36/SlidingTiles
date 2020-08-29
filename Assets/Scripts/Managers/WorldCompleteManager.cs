using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;

public class WorldCompleteManager : ContextManager, IRequireResources
{
	private int World;
	public TMP_Text WorldText;
	public WinScreenStar[] Stars;
	public TMP_Text AnyStarTime;
	public TMP_Text ThreeStarTime;
	public Image Background;

	public GameObject ContinueButton;

	public GameObject ComparePrompt;

	public Button LeaderboardButton;
	public Leaderboard Leaderboard;

	public bool Loaded { get; set; } = false;

	public override void HandleInput(InputPackage p) { }

	public override void Awake() {
		base.Awake();
		World = GameManager.Instance.SaveData.LastPlayedWorld;
		Addressables.LoadAssetAsync<CopyObject>($"World{World}/Background").Completed +=
			(obj) => {
				if(Background != null) {
					Background.material = obj.Result.Material;
				}
				Loaded = true;
			};

		LevelData[,] levelData = GameManager.Instance.SaveData.LevelData;

		int len = levelData.GetLength(1);
		int minStars = 4;
		float anyStarTime = 0;
		bool validThreeStarTime = true;
		float threeStarTime = 0;
		for (int i = 0; i < len; i++) {
			LevelData ld = levelData[World - 1, i];
			minStars = Mathf.Min(minStars, ld.MaxStarsCollected);
			anyStarTime += ld.AnyStarCompletionTime;
			threeStarTime += ld.ThreeStarCompletionTime;
			if(ld.MaxStarsCollected < 3) {
				validThreeStarTime = false;
			}
		}

		// Add to leaderboard (if applicable) then get leaderboard data from server
		// once this process is completed, enable the leaderboard button
		int scoresToProcess = validThreeStarTime ? 2 : 1;
		var store = GameManager.Instance.StoreCommunicator;
		string anyStarLeaderboardID = $"World{World}";
		string threeStarLeaderboardID = $"World{World}AllStars";
		store.AddToLeaderboard(anyStarLeaderboardID, anyStarTime, (success) => {
			store.GetLeaderboard(anyStarLeaderboardID, true, (scores) => {
				Leaderboard.SetScores(true, scores);
				TrySetLeaderboardButtonInteractable(ref scoresToProcess);
			});
		});
		if(validThreeStarTime) {
			store.AddToLeaderboard(threeStarLeaderboardID, threeStarTime, (success) => {
				store.GetLeaderboard(threeStarLeaderboardID, true, (scores) => {
					Leaderboard.SetScores(false, scores);
					TrySetLeaderboardButtonInteractable(ref scoresToProcess);
				});
			});
		}
		else {
			store.GetLeaderboard(threeStarLeaderboardID, false, (scores) => {
				Leaderboard.SetScores(false, scores);
				TrySetLeaderboardButtonInteractable(ref scoresToProcess);
			});
		}

		ComparePrompt.SetActive(World == 1);

		// set text
		WorldText.text = $"World {World}";

		// set stars
		for (int i = 0; i < Stars.Length; i++) {
			if (minStars > i) {
				Stars[i].AllowAnimate = true;
				Stars[i].PercentAnimated = 1;
			}
			else {
				Stars[i].PercentAnimated = 0;
			}
		}

		// set times
		AnyStarTime.text = Utils.SplitTime(anyStarTime, MillisecondDisplay.None);
		ThreeStarTime.text = validThreeStarTime ? Utils.SplitTime(threeStarTime, MillisecondDisplay.None) : "--";

		// set last played world to the next world if player haven't been there before
		int nextWorld = World+1;
		var nextLevel = SceneHelpers.GetBuildIndexFromLevel(nextWorld, 1);
		if (nextWorld <= GameManager.AvailableWorlds && nextLevel >= GameManager.Instance.HighestUnlockedLevel) {
			GameManager.Instance.SaveData.LastPlayedWorld = nextWorld;
		}
		else {
			GameManager.Instance.SaveData.LastPlayedWorld = World;
		}
	}

	public void GoToMenu() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		GameManager.Instance.LoadScene(
			SceneHelpers.MenuBuildIndex,
			null,
			() => GameManager.Instance.MenuManager.OpenLevelSelect(true)
		);
	}

	public void Continue() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		int nextWorld = World+1;
		if(nextWorld <= GameManager.AvailableWorlds) {
			MusicManager.Instance.LoadMusicForWorldAndChangeTrack(nextWorld);
			GameManager.Instance.LoadScene(SceneHelpers.GetBuildIndexFromLevel(nextWorld, 1));
		}
		else {
			GameManager.Instance.LoadScene(SceneHelpers.SceneCount-1);
		}
	}

	public void HideContinue() {
		ContinueButton.SetActive(false);
	}

	public void ShowLeaderboard() {
		Leaderboard.Open();
	}

	public void TrySetLeaderboardButtonInteractable(ref int blockers) {
		if(--blockers <= 0) {
			LeaderboardButton.interactable = true;
			foreach(var image in LeaderboardButton.GetComponentsInChildren<Image>()) {
				Color c = image.color;
				c.a = 1f;
				image.color = c;
			}
		}
	}
}
