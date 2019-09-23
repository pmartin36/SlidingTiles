using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionButtons : MonoBehaviour
{
	private Animator _animator;
	private Animator Animator {
		get {
			_animator = _animator ?? GetComponent<Animator>();
			return _animator;
		}
	}

	public void Spawn() {
		GameManager.Instance.LevelManager.Respawn();
	}

	public void HighlightSpawn(bool highlight) {
		Animator.SetBool("highlight", highlight);
	}

	public void Reset() {
		GameManager.Instance.LevelManager.Reset(true);
	}

	public void Menu() {

	}
}
