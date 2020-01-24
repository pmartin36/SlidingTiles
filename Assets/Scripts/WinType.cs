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

	public virtual void Start() {
		frontPanelImage = frontPanel.GetComponent<Image>();
	}

	public virtual void Run(float elapsedTime, int stars, int availableStars = 3, Action<WinTypeAction> callback = null) {
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
	}

	public void ShowLeaderboard() {
		Debug.Log("Leaderboard");
	}

	public void Hide() {
		Animator a = GetComponent<Animator>();
		a.SetFloat("direction", -1);
		a.Play("rowwin", 0, 1);
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

