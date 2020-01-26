﻿using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum WinTypeAction {
	None,
	Menu, // up
	Reset, // left
	LevelSelect, // down
	Next // right
}

public abstract class WinType : MonoBehaviour {
	public WinScreenStar[] Stars;
	public Image Background;
	public TMP_Text ElapsedTime;
	public TMP_Text RecordTime;

	public GameObject ComparePrompt;

	[Range(0, 1)]
	public float NewRecordLerp;

	[Range(0,1)]
	public float PercentAnimated;

	[HideInInspector]
	public bool IsAnimating;

	public bool ActionSelected { get; private set; }
	protected Vector2 ActionMoveDirection;

	private Action<WinTypeAction> OnActionSelected;

	[SerializeField]
	protected RectTransform frontPanel; 
	protected Image frontPanelImage;
	protected Animator anim;

	protected Leaderboard leaderboard;

	public virtual void Start() {
		anim = GetComponent<Animator>();
		frontPanelImage = frontPanel.GetComponent<Image>();
		leaderboard = frontPanel.GetComponentInChildren<Leaderboard>(true);
	}

	public virtual void Run(TimeInfo timeInfo, int stars, int availableStars = 3, Action<WinTypeAction> callback = null) {
		frontPanel.gameObject.SetActive(true);
		for (int i = 0; i < Stars.Length; i++) {
			if (stars > i) {
				Stars[i].AllowAnimate = true;
			}
			else if (availableStars > i) {
				Stars[i].AllowAnimate = false;
			}
			else {
				Stars[i].gameObject.SetActive(false);
			}
		}

		RecordTime.gameObject.SetActive(timeInfo.Record);

		float elapsedTime = timeInfo.Time;
		int minutes = Mathf.FloorToInt(elapsedTime / 60f);
		int intSeconds = Mathf.FloorToInt(elapsedTime);
		int seconds = intSeconds % 60;
		float ms = elapsedTime - intSeconds;
		ElapsedTime.text = $"{minutes:0}:{seconds:00}<sub>{ms:.000}</sub>";

		OnActionSelected = callback;
	}

	public virtual bool SelectAction(WinTypeAction w) {
		if(w == WinTypeAction.Next && !GameManager.Instance.CanPlayNextLevel()) {
			return false;
		}

		ActionSelected = true;
		IsAnimating = true;
		OnActionSelected?.Invoke(w);
		return true;
	}

	protected void Update() {
		if(IsAnimating) {
			frontPanelImage.material.SetFloat("_AnimationPercent", PercentAnimated);
		}
		RecordTime.fontMaterial.SetFloat("_TexLerp", NewRecordLerp);
	}

	public void ShowLeaderboard() {
		GameManager.Instance.SaveData.HasComparedWithFriends = true;
		GameManager.Instance.Save();
		ComparePrompt.SetActive(false);
		leaderboard.gameObject.SetActive(true);
	}

	public void OnShowComplete() {
		int level = SceneHelpers.GetLevelFromBuildIndex(SceneHelpers.GetCurrentLevelBuildIndex());
		if(level > 0 && level % 5 == 0) {
			ComparePrompt.SetActive(!GameManager.Instance.SaveData.HasComparedWithFriends && anim.GetFloat("direction") > 0);
		}
	}

	public void Hide() {
		anim.SetFloat("direction", -1);
		anim.Play("rowwin", 0, 1);
	}

	public virtual void Reset() {
		frontPanel.gameObject.SetActive(false);
		PercentAnimated = 0;
		frontPanelImage.material.SetFloat("_AnimationPercent", PercentAnimated);
		IsAnimating = false;
		ActionSelected = false;
	}

	public IEnumerator WhenTilesOffScreen(Action action = null) {
		yield return new WaitUntil(() => PercentAnimated <= 0.001f);
		action?.Invoke();
	}
}

