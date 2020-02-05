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
		Color clear = Color.white;
		clear.a = 0;
		text.color = stayHidden ? clear : Color.white;
		button.image.color = stayHidden ? clear : tileColor;
		button.enabled = !stayHidden;
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
