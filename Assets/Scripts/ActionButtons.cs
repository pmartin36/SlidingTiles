using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButtons : MonoBehaviour
{
	private Animator _animator;
	private Animator Animator {
		get {
			_animator = _animator ?? GetComponent<Animator>();
			return _animator;
		}
	}

	public Image SpawnHighlightBorder;
	public float SpawnHighlightBorderRadius;

	public void Spawn() {
		GameManager.Instance.LevelManager.Respawn();
	}

	public void LateUpdate() {
		SpawnHighlightBorder.material.SetFloat("_Radius", SpawnHighlightBorderRadius);
	}

	public void HighlightSpawn(bool highlight) {
		Animator.SetBool("highlight", highlight);
	}

	public void Reset() {
		GameManager.Instance.LevelManager.Reset(true);
	}

	public void Menu() {
		GameManager.Instance.LoadScene(GameManager.MenuBuildIndex);
	}
}
