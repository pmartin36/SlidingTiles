using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
	public TMP_Text minutesSeconds;
	private RectTransform minutesSecondsRT;
	public TMP_Text milliseconds;
	private RectTransform millisecondsRT;

	private RectTransform rt;

	private int minutes;
	private int seconds;

	private bool showMilliseconds;
	private int minutesDigits = 1;
	private float maxWidth;
	
    void Start() {
		rt = this.GetComponent<RectTransform>();
		maxWidth = rt.sizeDelta.x * 0.9f;

		minutesSecondsRT = minutesSeconds.GetComponent<RectTransform>();
		millisecondsRT = milliseconds.GetComponent<RectTransform>();

		Debug.Log(minutesSeconds.GetComponent<RectTransform>().anchoredPosition);

		showMilliseconds = GameManager.Instance.SaveData.ShowMilliseconds;
		SetMillisecondsEnabled(showMilliseconds);
    }

	public void SetTimer(float _seconds) {
		minutes = Mathf.FloorToInt(_seconds / 60f);
		int intSeconds = Mathf.FloorToInt(_seconds);
		seconds = intSeconds % 60;
		minutesSeconds.text = $"{minutes:0}:{seconds:00}";
		if(showMilliseconds) {
			float s = _seconds - intSeconds;
			milliseconds.text = $"{s:.000}";		
		}

		int newMinutesDigits = minutes > 1 ? (int)(Mathf.Log10(minutes) + 1) : 1;
		if (newMinutesDigits != minutesDigits) {
			minutesDigits = newMinutesDigits;
			FitAndCenter();
		}
	}

	private void FitAndCenter() {
		float width = minutesSeconds.textBounds.size.x;
		if (width > maxWidth) {
			minutesSeconds.text = minutes.ToString();
			SetMillisecondsEnabled(false);
		}
		else if (showMilliseconds) {
			bool msActive = width + milliseconds.textBounds.size.x < maxWidth;
			SetMillisecondsEnabled(msActive);
		}
	}

	private void SetMillisecondsEnabled(bool enabled) {
		milliseconds.gameObject.SetActive(enabled);
		if (enabled) {
			minutesSeconds.ForceMeshUpdate();
			milliseconds.ForceMeshUpdate();

			float minuteSecondSizeFactor = 1.05f;
			float millisecondsSizeFactor = 1.1f;

			float totalWidth = minutesSeconds.textBounds.size.x * minuteSecondSizeFactor + milliseconds.textBounds.size.x * millisecondsSizeFactor;
			minutesSecondsRT.sizeDelta = new Vector2(minutesSeconds.textBounds.size.x* minuteSecondSizeFactor, minutesSecondsRT.sizeDelta.y);
			millisecondsRT.sizeDelta = new Vector2(milliseconds.textBounds.size.x* millisecondsSizeFactor, millisecondsRT.sizeDelta.y);
			minutesSecondsRT.anchoredPosition = new Vector2((minutesSeconds.textBounds.size.x - totalWidth) / 2f, minutesSecondsRT.anchoredPosition.y);
			millisecondsRT.anchoredPosition = new Vector2(
				(minutesSecondsRT.sizeDelta.x + millisecondsRT.sizeDelta.x) / 2f + minutesSecondsRT.anchoredPosition.x
				, millisecondsRT.anchoredPosition.y);
			minutesSeconds.alignment = TextAlignmentOptions.MidlineRight;
		}
		else {
			minutesSecondsRT.sizeDelta = new Vector2(maxWidth, rt.sizeDelta.y);
			minutesSecondsRT.anchoredPosition = Vector2.zero;
			minutesSeconds.alignment = TextAlignmentOptions.Center;
		}
	}
}
