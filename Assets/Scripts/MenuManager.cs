using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : ContextManager {
	public bool LevelSelectOpen { get; set; }

	[SerializeField]
	private GameObject HomeScreen;
	[SerializeField]
	private GameObject LevelSelectScreen;

	public void Start() {
		GetComponent<InputManager>().ContextManager = this;
	}

	public override void HandleInput(InputPackage p) {
		if(!LevelSelectOpen && p.Touchdown)  {
			OpenLevelSelect(true);
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
