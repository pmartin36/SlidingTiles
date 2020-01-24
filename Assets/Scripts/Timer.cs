using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
	private static int msShowingSpace = 120;
	private static int msHiddenSpace = 130;

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

		SetTimer(0);
		minutesSeconds.ForceMeshUpdate();
		milliseconds.ForceMeshUpdate();
		showMilliseconds = GameManager.Instance.SaveData.ShowMilliseconds;
		SetMillisecondsEnabled(showMilliseconds);
    }

	public void SetTimer(float _seconds) {
		minutes = Mathf.FloorToInt(_seconds / 60f);
		int intSeconds = Mathf.FloorToInt(_seconds);
		seconds = intSeconds % 60;
		int mspace = msHiddenSpace;
		if(showMilliseconds) {
			mspace = msShowingSpace;
			float s = _seconds - intSeconds;
			milliseconds.text = $"{s:.000}";		
		}
		minutesSeconds.text = $"<mspace={mspace}>{minutes:0}</mspace>:<mspace={mspace}>{seconds:00}</mspace>";

		int newMinutesDigits = minutes > 1 ? (int)(Mathf.Log10(minutes) + 1) : 1;
		if (newMinutesDigits != minutesDigits) {
			minutesDigits = newMinutesDigits;
			FitAndCenter();
		}
	}

	private void FitAndCenter() {
		minutesSeconds.ForceMeshUpdate();
		milliseconds.ForceMeshUpdate();

		float width = minutesSeconds.textBounds.size.x;
		if (width > maxWidth) {
			minutesSeconds.text = $"<mspace=130>{minutes}</mspace>";
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
			float minuteSecondSingleLetterSize = minutesSeconds.textBounds.size.x / (float)minutesSeconds.text.Length;
			float minuteSecondSizeFactor = 1f;
			float millisecondsSizeFactor = 1.05f;

			// use this to move things towards eachother
			float squishFactor = 0.05f;

			float totalWidth = minutesSeconds.textBounds.size.x * minuteSecondSizeFactor + milliseconds.textBounds.size.x * millisecondsSizeFactor;
			float squishOffset = totalWidth * (squishFactor / 2f);

			minutesSecondsRT.sizeDelta = new Vector2(minutesSeconds.textBounds.size.x* minuteSecondSizeFactor, minutesSecondsRT.sizeDelta.y);
			millisecondsRT.sizeDelta = new Vector2(milliseconds.textBounds.size.x* millisecondsSizeFactor, millisecondsRT.sizeDelta.y);

			float minutesSecondsPos = (minutesSeconds.textBounds.size.x - totalWidth) / 2f;
			minutesSecondsRT.anchoredPosition = new Vector2(minutesSecondsPos + squishOffset, 2);

			float millisecondsPos = (minutesSecondsRT.sizeDelta.x + millisecondsRT.sizeDelta.x) / 2f + minutesSecondsRT.anchoredPosition.x;
			millisecondsRT.anchoredPosition = new Vector2(millisecondsPos - squishOffset, millisecondsRT.anchoredPosition.y);
			minutesSeconds.alignment = TextAlignmentOptions.MidlineRight;

			// add space for one extra letter - otherwise when we overflow next time, the textsize is measured incorrectly
			Vector2 overflowSpace = Vector2.right * msShowingSpace;
			minutesSecondsRT.sizeDelta += overflowSpace;
			minutesSecondsRT.anchoredPosition -= overflowSpace / 2f;
		}
		else {
			minutesSecondsRT.sizeDelta = new Vector2(maxWidth, rt.sizeDelta.y);
			minutesSecondsRT.anchoredPosition = new Vector2(-6, -6);
			minutesSeconds.alignment = TextAlignmentOptions.Center;
			minutesSeconds.fontSize = 240;
		}
	}
}
