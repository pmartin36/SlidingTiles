using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScreen : MonoBehaviour
{
	Canvas canvas;

	public void Start() {
		canvas = GetComponent<Canvas>();
	}

	public void Show(bool show) {	
		if(show) {
			Camera c = FindObjectOfType<Camera>();
			canvas.worldCamera = c;
		}
		else {
			canvas.worldCamera = null;
		}

		foreach(Transform child in transform) {
			child.gameObject.SetActive(show);
		}
	}
}
