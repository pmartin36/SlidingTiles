using System;
using System.Collections;
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

	public virtual void Run(int stars, int availableStars = 3, Action<WinTypeAction> callback = null) {
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
		OnActionSelected = callback;
	}

	public virtual void SelectAction(WinTypeAction w) {
		ActionSelected = true;
		IsAnimating = true;
		OnActionSelected?.Invoke(w);
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

