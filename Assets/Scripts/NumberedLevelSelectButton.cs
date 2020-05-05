using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NumberedLevelSelectButton : LevelSelectButton
{
	public int Number { get; set; }
	public int? TempNumber { get; set; }

	public Image Lock;
	public Image Money;
	public RawImage[] Stars;
	public bool Unlocked { get; private set; }
	public bool Paywalled { get; private set; }

	public void Init(int num) {
		base.Init();	
		Number = num;
		text.text = num.ToString();
	}

	public void SetButtonInfo(Vector2 position, int num, bool unlocked, bool paywalled, int stars = 0) {
		// soft removing paywalled because it shouldn't be in the final product
		paywalled = false;

		text.text = num.ToString();
		SetPosition(position);
		if(num != Number) {
			TempNumber = num;
		}
		SetUnlocked(unlocked);
		SetPaywalled(paywalled);

		float alpha = unlocked && !paywalled ? 1 : 0.5f;
		Color c = text.color;
		c.a = unlocked ? 1 : 0.5f;
		text.color = c;

		SetStars(stars);
	}

	public void SetUnlocked(bool unlocked) {
		Unlocked = unlocked;
		// add lock icon over top
		Lock.gameObject.SetActive(!unlocked);
	}

	public void SetPaywalled(bool paywalled) {
		//Paywalled = paywalled;
		//// add money icon over top
		//Money.gameObject.SetActive(paywalled);
	}

	public void SetStars(int stars) {
		for (int i = 0; i < 3; i++) {
			RawImage star = Stars[i];
			star.gameObject.SetActive(i < stars);
		}
	}

	public override void TryEnableInteractable() {
		Interactable = Unlocked;
	}

	public void SetSlidePosition(Vector2 position, bool interactableAtEnd, System.Action newOnClickAction = null) {
		StartCoroutine(SlideToPosition(position, interactableAtEnd));
		if (newOnClickAction != null && interactableAtEnd) {
			SetOnClick(newOnClickAction);
		}
	}

	public void SetOnClick(System.Action action) {
		button.onClick = new Button.ButtonClickedEvent();
		button.onClick.AddListener(delegate { action(); });
	}

	public override void SetStayHidden(bool stayHidden) {
		base.SetStayHidden(stayHidden);
		if(stayHidden) {
			Lock.gameObject.SetActive(false);
		}
	}

	private IEnumerator SlideToPosition(Vector2 position, bool interactableAtEnd) {
		Interactable = false;

		Vector2 diff = position - rectTransform.anchoredPosition;
		Vector2 nDiff = diff.normalized;
		float slideTime = 0.75f;
		
		Vector2[] positions = new[] {
			rectTransform.anchoredPosition,
			rectTransform.anchoredPosition,
			position };
		float[] time;
		if (diff.y > 0) {
			// if moving up, do horizontal direction first
			positions[1] += Vector2.right * diff;
			time = new[] { Mathf.Abs(nDiff.x * slideTime), Mathf.Abs(nDiff.y * slideTime) };
		}
		else {
			// if moving down, do vertical direction first
			positions[1] += Vector2.up * diff;
			time = new[] { Mathf.Abs(nDiff.y * slideTime), Mathf.Abs(nDiff.x * slideTime) };
		}

		for (int i = 0; i < time.Length; i++) {
			if(time[i] < 0.25f && time[i] > 0.01f) {
				time[i] = 0.25f;
			}
			float t = 0;
			while (t < time[i]) {
				rectTransform.anchoredPosition = Vector2.Lerp(positions[i], positions[i + 1], Mathf.SmoothStep(0, 1, t / time[i]));
				// rectTransform.anchoredPosition = Vector2.Lerp(positions[i], positions[i + 1], EaseOut(0, 1, t, time[i]));
				// rectTransform.anchoredPosition = Vector2.Lerp(positions[i], positions[i + 1], t / time[i]);
				t += Time.deltaTime;
				yield return null;
			}
		}

		rectTransform.anchoredPosition = position;
		Interactable = interactableAtEnd;
	}

	private float EaseOut(float start, float end, float time, float duration) {
		time /= duration;
		return -end * time * (time - 2) + start;
	}
	
	private Vector2 EaseOut(Vector2 start, Vector2 end, float time, float duration) {
		time /= duration;
		return -end * (Vector2.one * time * (time - 2)) + start;
	}
}
