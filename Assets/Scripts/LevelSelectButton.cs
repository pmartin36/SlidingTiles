using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectButton : MenuImageSpriteCopy
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

	public void Init() {
		button = GetComponent<Button>();
		text = GetComponentInChildren<TMP_Text>();
		rectTransform = GetComponent<RectTransform>();
		anim = GetComponent<Animator>();
	}

	public void SetPosition(Vector2 position) {
		rectTransform.anchoredPosition = position;
	}

	public void SetHidden(bool hide, System.Action callback = null, bool instant = false) {
		Interactable = false;
		OnHideShowCallback = callback;

		int start = hide ? 1 : 0;
		if(instant) start = 1 - start;

		anim.SetBool("hidden", hide);
		anim.SetFloat("dir", hide ? -1 : 1);
		anim.Play("showLevelSelectButton", -1, start);
	}

	public virtual void SetStayHidden(bool stayHidden) {
		Color textColor = Color.white;
		Color tileColor = button.image.color;
		textColor.a = stayHidden ? 0f : 1f;
		tileColor.a = stayHidden ? 0f : 1f;
		text.color = textColor;
		button.image.color = tileColor;
		button.enabled = !stayHidden;
		if(stayHidden) {
			// for some reason, hidden images and buttons were block visible ones
			// so instead let's toss the hidden ones off to the side
			rectTransform.anchoredPosition = Vector2.one * 1000f;
		}
	}

	public virtual void TryEnableInteractable() {
		Interactable = true;
	}

	public virtual void AtAnimationEnd() {
		if(anim.GetFloat("dir") > 0) {
			TryEnableInteractable();
		}
	}

	public void AtAnimationBegin() {
		OnHideShowCallback?.Invoke();
	}
}
