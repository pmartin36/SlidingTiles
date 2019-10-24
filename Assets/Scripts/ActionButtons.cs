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
		//CameraManager.Instance.CameraController.RandomShake(5f, 5f, 0.7f, false);
	}

	public void Menu() {
		//CameraManager.Instance.CameraController.Shake(
		//	2f,
		//	0.3f,
		//	Vector2.up * 5f,
		//	Vector2.up * -1f
		//);
		GameManager.Instance.LoadScene(GameManager.MenuBuildIndex);
	}
}
