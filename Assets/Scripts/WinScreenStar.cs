using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenStar : MonoBehaviour
{
	private Image image;
	[Range(0,1)]
	public float Alpha;
	public float MaxAlpha { get; set; }
	public Color Color {
		get => image.color;
		set {
			Color c = value;
			c.a = Mathf.Min(MaxAlpha, Alpha);
			image.color = c;
		}
	}

	void Awake() {
		image = GetComponent<Image>();
	}

	void Update() {
		UpdateAlpha(Alpha);
	}

	public void UpdateAlpha(float alpha) {
		var bc = image.color;
		bc.a = Mathf.Min(MaxAlpha, alpha);
		image.color = bc;
	}
}
