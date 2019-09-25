using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberedLevelSelectButton : LevelSelectButton
{
	public int Number { get; set; }

	public void Init(int num) {
		base.Init();
		
		Number = num;
		text.text = num.ToString();
	}

	public void SetPositionAndNumber(Vector2 position, int tempNumber) {
		text.text = tempNumber.ToString();
		SetPosition(position);
	}

	public void SetSlidePosition(Vector2 position, bool interactableAtEnd) {
		StartCoroutine(SlideToPosition(position, interactableAtEnd));
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
			float t = 0;
			while (t < time[i]) {
				rectTransform.anchoredPosition = Vector2.Lerp(positions[i], positions[i + 1], t / time[i]);
				t += Time.deltaTime;
				yield return null;
			}
		}

		rectTransform.anchoredPosition = position;
		Interactable = interactableAtEnd;
	}
}
