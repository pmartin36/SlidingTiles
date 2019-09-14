using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScreen : MonoBehaviour
{
	public void Show(bool show) {	
		foreach(Transform child in transform) {
			child.gameObject.SetActive(show);
		}
	}
}
