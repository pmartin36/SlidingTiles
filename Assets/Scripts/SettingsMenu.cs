using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
	private bool showing;
	private float source, target;
	private RectTransform rt;

	public RectTransform MusicWidget;
	public RectTransform SoundWidget;
	public RectTransform ShowMSWidget;
	public GameObject AdRemovalWidget;
	public RectTransform CreditsPanel;

	public void Start() {
		if (GameManager.Instance.SaveData.AdsRemoved) {
			HideAdRemovalWidget();
		}
	}

	public void HideAdRemovalWidget() {
		AdRemovalWidget.SetActive(false);
	}

	public void Show(bool show) {
		showing = show;
		if (showing) {
			gameObject.SetActive(true);
		}
		if(rt == null) {
			rt = GetComponent<RectTransform>();
		}

		source = showing ? -2000 : 0;
		target = showing ? 50 : -2000;
		StartCoroutine(ExecuteShow());
	}

	private IEnumerator ExecuteShow() {
		rt.anchoredPosition = new Vector2(source, 0);
		while (Mathf.Abs(rt.anchoredPosition.x - target) > 50) {
			rt.anchoredPosition += new Vector2((target - rt.anchoredPosition.x) * Time.deltaTime * 5, 0);
			yield return null;
		}
		rt.anchoredPosition = new Vector2(Mathf.Min(0, target), 0);

		if (!showing) {
			gameObject.SetActive(false);
		}
	}
}
