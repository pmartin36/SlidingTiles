using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScreen : MonoBehaviour
{
	private Animator animator;

	public void Awake() {
		animator = GetComponent<Animator>();
	}

	public void Show(bool show) {	
		if(show) {
			foreach(Transform t in transform) {
				t.gameObject.SetActive(true);
			}

			animator.SetBool("Hide", false);
			animator.Play("Show");
		}
		else {
			animator.SetBool("Hide", true);
		}
	}
}
