using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelInfoUI : MonoBehaviour
{
	private static int msShowingSpace = 72;
	private static int msHiddenSpace = 90;

	private bool showMilliseconds = true;

	public TMP_Text minutesSeconds;
	private RectTransform minutesSecondsRT;
	public TMP_Text milliseconds;
	private RectTransform millisecondsRT;

	public TMP_Text LevelName;

	private RectTransform rt;

	private int minutes;
	private int seconds;

	private bool showTimer;
	private int minutesDigits = 1;
	private float maxWidth;
	
    void Start() {
		rt = this.GetComponent<RectTransform>();
		maxWidth = rt.sizeDelta.x * 0.9f;

		minutesSecondsRT = minutesSeconds.GetComponent<RectTransform>();
		//millisecondsRT = milliseconds.GetComponent<RectTransform>();
		LevelName.text = this.gameObject.scene.name;

		showTimer = GameManager.Instance.SaveData.ShowTimer;
		if (showTimer) {
			SetTimer(0);
			minutesSeconds.ForceMeshUpdate();

			// milliseconds.ForceMeshUpdate();
			// SetMillisecondsEnabled(showMilliseconds);
		}
		else {
			minutesSeconds.gameObject.SetActive(false);
			milliseconds.gameObject.SetActive(false);

			// Position/Size Level Name
			//LevelName.fontSize = 200;
			//LevelName.alignment = TextAlignmentOptions.Center;
			//LevelName.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -9);
		}
	}

	public void SetTimer(float _seconds) {
		if(showTimer) {
			minutes = Mathf.FloorToInt(_seconds / 60f);
			int intSeconds = Mathf.FloorToInt(_seconds);
			seconds = intSeconds % 60;
			float ms = _seconds - intSeconds;
			minutesSeconds.text = $"{minutes:0}:{seconds:00}<voffset=0.05em><sub>{ms:.000}</sub></voffset>";

			// if you want more advanced timer stuff, this is it
			/*
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
			*/
		}
	}

	private void FitAndCenter() {
		minutesSeconds.ForceMeshUpdate();
		milliseconds.ForceMeshUpdate();

		float width = minutesSeconds.textBounds.size.x;
		if (width > maxWidth) {
			minutesSeconds.text = $"<mspace={msHiddenSpace}>{minutes}</mspace>";
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
			minutesSecondsRT.anchoredPosition = new Vector2(minutesSecondsPos + squishOffset, minutesSecondsRT.anchoredPosition.y);

			float millisecondsPos = (minutesSecondsRT.sizeDelta.x + millisecondsRT.sizeDelta.x) / 2f + minutesSecondsRT.anchoredPosition.x;
			millisecondsRT.anchoredPosition = new Vector2(millisecondsPos - squishOffset, millisecondsRT.anchoredPosition.y);
			minutesSeconds.alignment = TextAlignmentOptions.BottomRight;

			// add space for one extra letter - otherwise when we overflow next time, the textsize is measured incorrectly
			Vector2 overflowSpace = Vector2.right * msShowingSpace;
			minutesSecondsRT.sizeDelta += overflowSpace;
			minutesSecondsRT.anchoredPosition -= overflowSpace / 2f;
		}
		else {
			minutesSecondsRT.sizeDelta = new Vector2(maxWidth, rt.sizeDelta.y);
			minutesSeconds.alignment = TextAlignmentOptions.Bottom;
			//minutesSecondsRT.anchoredPosition = new Vector2(-6, -6);
			//minutesSeconds.fontSize = 240;
		}
	}
}
