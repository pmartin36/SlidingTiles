using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
	public TMP_Text minutesSeconds;
	public TMP_Text milliseconds;

	private bool showMilliseconds;
	
    void Start() {
		showMilliseconds = GameManager.Instance.SaveData.ShowMilliseconds;
		if(showMilliseconds) {
			milliseconds.gameObject.SetActive(true);
		}
		else {
			minutesSeconds.alignment = TextAlignmentOptions.Center;
			minutesSeconds.margin = Vector4.zero;
		}
    }

	public void SetTimer(float seconds) {
		int minutes = Mathf.FloorToInt(seconds / 60f);
		int intSeconds = Mathf.FloorToInt(seconds);
		int sixtySeconds = intSeconds % 60;
		minutesSeconds.text = $"{minutes:0}:{sixtySeconds:00}";
		if(showMilliseconds) {
			float s = seconds - intSeconds;
			milliseconds.text = $"{s:.000}";
		}
	}
}
