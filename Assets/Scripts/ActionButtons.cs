using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionButtons : MonoBehaviour
{
	private Animator animator;

	public void Start() {
		animator = GetComponent<Animator>();
		animator.SetBool("highlight", true);
	}

	public void Spawn() {
		GameManager.Instance.LevelManager.Respawn();
	}

	public void HighlightSpawn(bool highlight) {
		animator.SetBool("highlight", highlight);
	}

	public void Reset() {
		GameManager.Instance.LevelManager.Reset(true);
	}

	public void Menu() {

	}
}
