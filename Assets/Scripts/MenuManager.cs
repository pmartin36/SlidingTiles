using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : ContextManager {
	public bool LevelSelectOpen { get; set; }

	[SerializeField]
	private GameObject HomeScreen;
	[SerializeField]
	private GameObject LevelSelectScreen;

	bool temp_hasUserData = false;

	public override void Start() {
		GetComponent<InputManager>().ContextManager = this;
	}

	public override void HandleInput(InputPackage p) {
		if (p.Touchdown && p.TouchdownChange) {
			if (LevelSelectOpen) {
				
			}
			else {
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

		if(skipAnimation) {
			HomeScreen.SetActive(false);
			LevelSelectScreen.SetActive(true);
		}
		else {

		}
	}

	public void SettingsClicked() {

	}

	public void GooglePlayGamesClicked() {

	}

	public void MBMClicked() {

	}
}
