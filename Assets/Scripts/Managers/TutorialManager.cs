using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : LevelManager
{
	public Image Finger;
	private TrailRenderer FingerTrail;
	private RectTransform FingerRectTransform;

	private bool shouldClearFingerTrail;
	public float FingerRadius;

	public TutorialTile TutorialTile;

	private Animator anim;
	private float lastTimeScale;

	private bool playerEnteredTile, pressed, hasMovedFromCenter;
	private bool PlayerEnteredTile {
		get => playerEnteredTile;
		set {
			anim.SetBool("Spawned", value);
			playerEnteredTile = value;
		}
	}
	private bool Pressed {
		get => pressed;
		set
		{
			anim.SetBool("Pressed", value);
			pressed = value;
		}
	}
	private bool HasMovedFromCenter {
		get => hasMovedFromCenter;
		set
		{
			anim.SetBool("HasMovedFromCenter", value);
			hasMovedFromCenter = value;
		}
	}

	public override void Start() {
		base.Start();
		anim = GetComponent<Animator>();
		FingerTrail = Finger.GetComponentInChildren<TrailRenderer>(true);
		FingerRectTransform = Finger.GetComponent<RectTransform>();

		string sn = gameObject.scene.name;
		int tutorialNumber = int.Parse(sn.Substring(sn.Length - 1));
		anim.SetInteger("Level", tutorialNumber);

		TutorialTile.TutorialInit(
			() => Pressed = true,
			() => {
				if (!HasMovedFromCenter) {
					GameManager.Instance.SetTimescale(1f, 0.05f);
					Finger.gameObject.SetActive(false);

					HasMovedFromCenter = true;
				}
			},
			(entered) => {
				if(!HasMovedFromCenter) {
					if(entered) {
						GameManager.Instance.SetTimescale(0.05f, 0.05f);
					}
					else {
						GameManager.Instance.SetTimescale(1f, 0.05f);
					}
				}
			}
		);
	}

	public override void Update() {
		base.Update();
		Finger.material.SetFloat("_Radius",FingerRadius);
	}

	public void LateUpdate() {
		if(shouldClearFingerTrail) {
			FingerTrail.Clear();
			shouldClearFingerTrail = false;
		}
	}

	//protected override void Pause() {
	//	lastTimeScale = Time.timeScale;
	//	base.Pause();
	//}

	//protected override void Unpause() {
	//	GameManager.Instance.SetTimescale(lastTimeScale);
	//}

	public override void CreateRespawnManager() {
		RespawnManager = new RespawnManager(gameObject.scene, Player);
	}

	public override void Reset(bool fromRightSideButton) {
		base.Reset(fromRightSideButton);
		if(!(fromRightSideButton && Won)) {
			ResetAnimation();
		}
	}

	public override void PlayPauseButtonClicked() {
		Finger.gameObject.SetActive(true);
		PlayerEnteredTile = true;

		base.PlayPauseButtonClicked();
	}

	public void ClearTrail() {
		shouldClearFingerTrail = true;
	}

	public void ReachedTile() {
		anim.SetBool("AtStartingTile", true);
		FingerTrail.enabled = true;
	}

	public override void PlayerAliveChange(object player, bool alive) {
		base.PlayerAliveChange(player, alive);
		if(!alive) {
			ResetAnimation();
			Grid.Reset();
		}
	}

	private void ResetAnimation() {
		lastTimeScale = 1f;
		PlayerEnteredTile = false;
		Pressed = false;
		HasMovedFromCenter = false;
		anim.SetBool("AtStartingTile", false);
		FingerTrail.enabled = false;
		FingerTrail.gameObject.SetActive(false);
		Finger.gameObject.SetActive(true);
	}

	private IEnumerator TimeScaleIndepedentMoveTo(Vector3 moveTo, float time, Action onReachLocation = null) {
		float t = 0f;
		float yt = 1/120f;
		var yieldTime = new WaitForSecondsRealtime(yt);
		Vector3 startPosition = FingerRectTransform.anchoredPosition;

		while(t < time) {
			var l = Vector3.Lerp(startPosition, moveTo, t / time);
			FingerRectTransform.anchoredPosition = Vector3.Lerp(startPosition, moveTo, t/time);
			t += yt;
			yield return yieldTime;
		}

		FingerRectTransform.anchoredPosition = moveTo;
		onReachLocation?.Invoke();
	}
}
