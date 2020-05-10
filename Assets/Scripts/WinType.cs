using System;
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

	private ConditionalWinScreenPopup[] ConditionalPopups;

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
		ConditionalPopups = GetComponentsInChildren<ConditionalWinScreenPopup>(true);
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
		ElapsedTime.text = Utils.SplitTime(elapsedTime, true);
		ElapsedTime.ForceMeshUpdate();

		RectTransform elapsedTimeRT = ElapsedTime.GetComponent<RectTransform>();
		float width = ElapsedTime.textBounds.size.x;
		elapsedTimeRT.sizeDelta = new Vector2(width, elapsedTimeRT.sizeDelta.y);
		elapsedTimeRT.anchoredPosition = new Vector2((-width / 2f) - 25f, elapsedTimeRT.anchoredPosition.y);

		RectTransform newRecordRT = RecordTime.GetComponent<RectTransform>();
		newRecordRT.sizeDelta = new Vector2(width, newRecordRT.sizeDelta.y);
		newRecordRT.anchoredPosition = new Vector2(elapsedTimeRT.anchoredPosition.x, newRecordRT.anchoredPosition.y);

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
		RecordTime.fontMaterial.SetFloat("_UnderlayDilate", 1-NewRecordLerp);
	}

	public void ShowLeaderboard() {
		GameManager.Instance.SaveData.HasComparedWithFriends = true;
		GameManager.Instance.Save();
		foreach(var c in ConditionalPopups) {
			if(c.PopupType == WinScreenPopup.Compare)
				c.gameObject.SetActive(false);
		}

		leaderboard.gameObject.SetActive(true);
	}

	public void RateClicked() {
		GameManager.Instance.SaveData.HasClickedToRate = true;
		GameManager.Instance.Save();
		foreach (var c in ConditionalPopups) {
			if (c.PopupType == WinScreenPopup.Rate)
				c.gameObject.SetActive(false);
		}

		GameManager.Instance.StoreCommunicator.GoToStore();
	}

	public void OnShowComplete() {
		var (world, level) = SceneHelpers.GetWorldAndLevelFromBuildIndex(SceneHelpers.GetCurrentLevelBuildIndex());
		if(anim.GetFloat("direction") > 0) {
			foreach (var c in ConditionalPopups) {
				c.gameObject.SetActive(c.ShouldShow(world, level));
			}
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

