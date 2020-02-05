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

	public AnimationCurve Curve;

	public override void Start() {
		base.Start();
	}

	public override void HandleInput(InputPackage p) {
		if (p.Touchdown && !p.PointerOverGameObject && p.TouchdownChange) {
			if(!SettingsOpen && !LevelSelectOpen) {
				if (GameManager.Instance.SaveData.LastPlayedWorld > 0) {
					OpenLevelSelect(false);
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

		int world = GameManager.Instance.LastPlayedWorld;
		LevelSelectScreen.Init(world, this);
		(LevelSelectScreen.MirroredComponent as LevelSelect).Init(world, this);

		if (skipAnimation) {
			HomeScreen.SetActive(false);
			LevelSelectScreen.gameObject.SetActive(true);
			LevelSelectScreen.MirroredComponent.gameObject.SetActive(true);
		}
		else {
			StartCoroutine(MoveToLevelSelectAnimation());
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

	public void RemoveAdsClicked() {
		SettingsMenu.HideAdRemovalWidget();
		
	}

	private IEnumerator MoveToLevelSelectAnimation() {
		float time = 0;

		Vector2 levelSelectStart = new Vector2(1600, 0);
		Vector2 homeEnd = new Vector2(-1600, 0);
		Vector2 middle = Vector2.zero;

		var lsRT = LevelSelectScreen.GetComponent<RectTransform>();
		var lsMirrorRT = LevelSelectScreen.MirroredComponent.GetComponent<RectTransform>();
		var homeRT = HomeScreen.GetComponent<RectTransform>();

		LevelSelectScreen.gameObject.SetActive(true);
		LevelSelectScreen.MirroredComponent.gameObject.SetActive(true);

		float anim_time = 0.75f;
		while (time < anim_time) {
			float t = Curve.Evaluate(time / anim_time);
			Vector2 levelSelectPosition = new Vector2( 1600 - t * 1600, 0 );
			Vector2 homePosition = new Vector2( 0 - t * 1600, 0 );

			homeRT.anchoredPosition = homePosition;
			lsRT.anchoredPosition = levelSelectPosition;
			lsMirrorRT.anchoredPosition = levelSelectPosition;

			time += Time.deltaTime;
			yield return null;
		}

		lsRT.anchoredPosition = middle;
		lsMirrorRT.anchoredPosition = middle;
		HomeScreen.SetActive(false);
	}
}
