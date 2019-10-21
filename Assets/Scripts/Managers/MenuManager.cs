using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : ContextManager {
	public bool LevelSelectOpen { get; set; }
	public bool SettingsOpen {
		get => SettingsMenu.activeInHierarchy;
		set => SettingsMenu.SetActive(value);
	}

	public GameObject SettingsMenu;
	public SettingsButton SettingsButton;

	[SerializeField]
	private GameObject HomeScreen;
	[SerializeField]
	private GameObject LevelSelectScreen;

	bool temp_hasUserData = true;

	public override void HandleInput(InputPackage p) {
		if (p.Touchdown && !p.PointerOverGameObject && p.TouchdownChange) {
			if(!SettingsOpen && !LevelSelectOpen) {
				if (temp_hasUserData) {
					OpenLevelSelect(true);
				}
				else {
					AcceptingInputs = false;
					GameManager.Instance.LoadScene(GameManager.TutorialLevelStart, null);
				}
			}
		}
	}

	public void OpenLevelSelect(bool skipAnimation = false) { 
		LevelSelectOpen = true;
		int highestUnlocked = GameManager.Instance.HighestUnlockedLevel;


		// User hasn't completed the tutorial levels yet
		if (highestUnlocked < GameManager.TutorialLevelStart + 2) {
			GameManager.Instance.LoadScene(highestUnlocked, null);
			return;
		}

		if(skipAnimation) {
			HomeScreen.SetActive(false);
			LevelSelectScreen.SetActive(true);
		}
		else {

		}
	}

	public void ToggleSettings() {
		SettingsOpen = !SettingsOpen;
		SettingsButton.SetIcon(SettingsOpen);
	}

	public void GooglePlayGamesClicked() {

	}

	public void LogoClicked() {

	}

	public void CreditsClicked() {

	}
}
