using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenButton : MonoBehaviour
{
	private Image background;
	private Image icon;

	[Range(0, 1)]
	public float Alpha;

    void Awake() {
		foreach(Image i in GetComponentsInChildren<Image>()) {
			if(i.gameObject == this.gameObject) {
				background = i;
			}
			else {
				icon = i;
			}
		}
    }

    void Update() {
        UpdateAlpha(Alpha);
	}

	public void UpdateAlpha(float alpha) {
		var bc = background.color;
		bc.a = alpha;
		background.color = bc;

		var ic = icon.color;
		ic.a = alpha;
		icon.color = ic;
	}
}
