using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
	
	public bool Interactable { 
		get => button.interactable;
		set => button.interactable = value;
	}
	protected TMP_Text text;
	protected RectTransform rectTransform;
	protected Button button;

	protected System.Action OnHideShowCallback;
	protected Animator anim;

	protected Color tileColor; // temporary I think

	public void Init() {
		button = GetComponent<Button>();
		text = GetComponentInChildren<TMP_Text>();
		rectTransform = GetComponent<RectTransform>();
		anim = GetComponent<Animator>();

		tileColor = button.image.color;
	}

	public void SetPosition(Vector2 position) {
		rectTransform.anchoredPosition = position;
	}

	public void SetHidden(bool hide, System.Action callback = null) {
		OnHideShowCallback = callback;

		anim.SetBool("hidden", hide);
		anim.SetFloat("dir", hide ? -1 : 1);
		anim.Play("showLevelSelectButton", -1, hide ? 1 : 0);
	}

	public void SetStayHidden(bool stayHidden) {
		text.color = stayHidden ? Color.clear : Color.white;
		button.image.color = stayHidden ? Color.clear : tileColor;
		button.enabled = !stayHidden;
	}

	public void InvokeHideShowCallback() {
		OnHideShowCallback?.Invoke();
	}
}
