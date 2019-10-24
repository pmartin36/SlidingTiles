using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenStar : MonoBehaviour
{
	private Image outerStar;
	private Image innerStar;

	[Range(0,1)]
	public float PercentAnimated;
	public bool AllowAnimate { get; set; }

	[Range(-1f, 1f)]
	public float RadiusModifier;

	void Awake() {
		foreach(Image i in GetComponentsInChildren<Image>()) {
			i.material = new Material(i.material);
			if(i.gameObject == this.gameObject) {
				outerStar = i;
			}
			else {
				innerStar = i;
			}
		};
		UpdateAnimation();
	}

	void LateUpdate() {
		if(AllowAnimate) {
			UpdateAnimation();
		}
	}

	private void UpdateAnimation() {
		outerStar.material.SetFloat("_PctAnimated", PercentAnimated);
		innerStar.material.SetFloat("_PctAnimated", PercentAnimated);
		
		outerStar.material.SetFloat("_RadiusModifier", RadiusModifier);
	}
}
