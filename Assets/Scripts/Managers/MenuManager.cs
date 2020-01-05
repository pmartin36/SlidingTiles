using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : ContextManager {
	public bool LevelSelectOpen { get; set; }
	public bool SettingsOpen { get; set; }

	public SettingsMenu SettingsMenu;

	public float LevelBlend { get; set; }

	[SerializeField]
	private GameObject HomeScreen;
	[SerializeField]
	private LevelSelect LevelSelectScreen;

	public override void Start() {
		base.Start();
	}

	public override void HandleInput(InputPackage p) {
		if (p.Touchdown && !p.PointerOverGameObject && p.TouchdownChange) {
			if(!SettingsOpen && !LevelSelectOpen) {
				if (GameManager.Instance.SaveData.LastPlayedWorld > 0) {
					OpenLevelSelect(true);
				}
				else {
					AcceptingInputs = false;
					GameManager.Instance.LoadScene(SceneHelpers.TutorialLevelStart, null);
				}
			}
		}
	}

	public void OpenLevelSelect(bool skipAnimation = false) { 
		LevelSelectOpen = true;
		int highestUnlocked = GameManager.Instance.HighestUnlockedLevel;


		// User hasn't completed the tutorial levels yet
		if (highestUnlocked < SceneHelpers.TutorialLevelStart + 2) {
			GameManager.Instance.LoadScene(highestUnlocked, null);
			return;
		}

		if(skipAnimation) {
			HomeScreen.SetActive(false);

			int world = GameManager.Instance.LastPlayedWorld;
			LevelSelectScreen.Init(world, this);
			(LevelSelectScreen.MirroredComponent as LevelSelect).Init(world, this);

			LevelSelectScreen.gameObject.SetActive(true);
			LevelSelectScreen.MirroredComponent.gameObject.SetActive(true);		
		}
		else {

		}
	}

	public void LateUpdate() {
		CameraManager.Instance.CameraController.ModifyPostProcessSettings(LevelBlend);
	}

	public void ToggleSettings() {
		SettingsOpen = !SettingsOpen;
		SetSettingsOpen();
		GameManager.Instance.Save();
	}

	public void SetSettingsOpen() {
		//SettingsMenu.gameObject.SetActive(SettingsOpen);
		
		SettingsMenu.Show(SettingsOpen);
	}

	public void GooglePlayGamesClicked() {

	}

	public void LogoClicked() {

	}

	public void CreditsClicked() {

	}
}
