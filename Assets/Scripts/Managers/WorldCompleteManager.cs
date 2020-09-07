using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;
using System.Linq;

public class WorldCompleteManager : ContextManager, IRequireResources
{
	private int World;
	public TMP_Text WorldText;
	public WinScreenStar[] Stars;
	public TMP_Text AnyStarTime;
	public TMP_Text ThreeStarTime;
	public Image Background;

	public GameObject ContinueButton;
	public GameObject ContinueShadow;
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
		string anyStarID = $"World{World}";
		string threeStarID = $"World{World}AllStars";
		store.AddToLeaderboard(anyStarID, anyStarTime, (success) => {
			store.GetLeaderboard(anyStarID, true, (scores) => {
				if(scores.Count() < 1) {
					scores = new LeaderboardEntry[] { new LeaderboardEntry() {
						Rank = 1,
						Score = anyStarTime * 1000f,
						UserName = "You",
						IsUser = true
					}};
				}
				Leaderboard.SetScores(true, scores);
				TrySetLeaderboardButtonInteractable(ref scoresToProcess);
			});
		});
		if(validThreeStarTime) {
			store.AddToLeaderboard(threeStarID, threeStarTime, (success) => {
				store.GetLeaderboard(threeStarID, true, (scores) => {
					if (scores.Count() < 1) {
						scores = new LeaderboardEntry[] { new LeaderboardEntry() {
							Rank = 1,
							Score = threeStarTime * 1000f,
							UserName = "You",
							IsUser = true
						}};
					}
					Leaderboard.SetScores(false, scores);
					TrySetLeaderboardButtonInteractable(ref scoresToProcess);
				});
			});
		}
		else {
			store.GetLeaderboard(threeStarID, false, (scores) => {
				Leaderboard.SetScores(false, scores);
				TrySetLeaderboardButtonInteractable(ref scoresToProcess);
			});
		}

		// Add Achievements
		store.AddAchievement(anyStarID);
		if(validThreeStarTime) {
			store.AddAchievement(threeStarID);
		}

		if(ContinueButton.gameObject.activeInHierarchy) {
			ComparePrompt.SetActive(World == 1);
		}

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
	}

	public void GoToMenu() {
		MMVibrationManager.Haptic(HapticTypes.Selection);

		int nextWorld = World + 1;
		var nextLevel = SceneHelpers.GetBuildIndexFromLevel(nextWorld, 1);
		if (ContinueButton.activeInHierarchy // only active when we've reached here from a level
			&& nextWorld <= GameManager.AvailableWorlds 
			&& nextLevel >= GameManager.Instance.HighestUnlockedLevel) {
			GameManager.Instance.SaveData.LastPlayedWorld = nextWorld;
		}

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
			GameManager.Instance.SaveData.LastPlayedWorld = nextWorld;
			MusicManager.Instance.LoadMusicForWorldAndChangeTrack(nextWorld);
			GameManager.Instance.LoadScene(SceneHelpers.GetBuildIndexFromLevel(nextWorld, 1));
		}
		else {
			GameManager.Instance.LoadScene(SceneHelpers.SceneCount-1);
		}
	}

	public void HideContinue() {
		ContinueButton.SetActive(false);
		ContinueShadow.SetActive(false);
		ComparePrompt.SetActive(false);
		foreach(var rt  in ContinueButton.transform.parent.GetComponentsInChildren<RectTransform>()) {
			var pos = rt.anchoredPosition;
			pos.x *= 0.5f;
			rt.anchoredPosition = pos;
		}
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
