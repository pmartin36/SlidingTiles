using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;

public class MenuManager : ContextManager {
	public bool LevelSelectOpen { get; set; }
	public bool SettingsOpen { get; set; }

	public SettingsMenu SettingsMenu;

	public float LevelBlend { get; set; }

	[SerializeField]
	private GameObject HomeScreen;
	[SerializeField]
	private LevelSelect LevelSelectScreen;
	[SerializeField]
	private GameObject GooglePlayGamesModal;
	[SerializeField]
	private Image GooglePlayGamesIcon;

	public AnimationCurve Curve;
	public Sprite GameCenterIcon;

	private bool finishedTutorial = false;

	public override void Start() {
		base.Start();
		var gm = GameManager.Instance;
		finishedTutorial = gm.HighestUnlockedLevel >= SceneHelpers.TutorialLevelStart + 2;
		if(!finishedTutorial) {
			this.GetComponentsInChildren<Button>().First(b => b.gameObject.name.Contains("Tutorial")).gameObject.SetActive(false);
		}

		if(gm.StoreCommunicator is AppleCommunicator) {
			GooglePlayGamesIcon.sprite = GameCenterIcon;
		}
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

		// User hasn't completed the tutorial levels yet
		if (!finishedTutorial) {
			GameManager.Instance.LoadScene(GameManager.Instance.HighestUnlockedLevel, null);
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
		ScreenWipe wipeSettings = CameraManager.Instance.CameraController.GetModifiablePostProcessSettings<ScreenWipe>();
		if(wipeSettings != null) {
			wipeSettings.blend.value = LevelBlend;
		}
	}

	public void ToggleSettings() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		SettingsOpen = !SettingsOpen;
		SetSettingsOpen();
		GameManager.Instance.Save();
	}

	public void SetSettingsOpen() {
		//SettingsMenu.gameObject.SetActive(SettingsOpen);
		SettingsMenu.Show(SettingsOpen);
	}

	public void ReplayTutorial() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		GameManager.Instance.LoadScene(SceneHelpers.TutorialLevelStart, null);
	}

	public void GooglePlayGamesClicked() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		GameManager.Instance.StoreCommunicator.SignIn((signedIn) => {
			if (signedIn)
				GooglePlayGamesModal.gameObject.SetActive(true);
		});
	}

	public void CloseGooglePlayGamesModal() {
		GooglePlayGamesModal.gameObject.SetActive(false);
	}

	public void SignOut() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		GameManager.Instance.StoreCommunicator.SignOut((signedOut) => {
			if (signedOut)
				CloseGooglePlayGamesModal();
		});
	}

	public void ViewAchiements() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		GameManager.Instance.StoreCommunicator.DisplayAchievementUI();
	}

	public void RemoveAdsClicked() {
		GameManager.Instance.StoreCommunicator.AddPurchase("removeads");
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

		MMVibrationManager.Haptic(HapticTypes.LightImpact);

		lsRT.anchoredPosition = middle;
		lsMirrorRT.anchoredPosition = middle;
		HomeScreen.SetActive(false);
	}
}
