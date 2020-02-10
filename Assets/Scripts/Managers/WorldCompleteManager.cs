using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldCompleteManager : ContextManager
{
	private int World;
	public TMP_Text WorldText;
	public WinScreenStar[] Stars;
	public TMP_Text AnyStarTime;
	public TMP_Text ThreeStarTime;
	public GameObject BottomButtons;

	public override void HandleInput(InputPackage p) {
		
	}

	public override void Awake() {
		base.Awake();

		World = GameManager.Instance.SaveData.LastPlayedWorld;
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
		AnyStarTime.text = Utils.SplitTime(anyStarTime, false);
		ThreeStarTime.text = validThreeStarTime ? Utils.SplitTime(threeStarTime, false) : "--";
	}

	public void GoToMenu() {
		GameManager.Instance.LoadScene(SceneHelpers.MenuBuildIndex);
	}

	public void Continue() {
		GameManager.Instance.LoadScene(SceneHelpers.GetBuildIndexFromLevel(World+1, 1));
	}

	public void HideContinue() {
		BottomButtons.SetActive(false);
		WorldText.GetComponent<RectTransform>().anchoredPosition -= Vector2.down * 40;
	}
}
