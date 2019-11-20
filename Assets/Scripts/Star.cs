using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
	public float RotationAmount;
	public bool WasCollected => animator.GetBool("collected");
	private Animator animator;

    void Start() {
		animator = GetComponentInParent<Animator>();
		StartIdleAnimation();
		GetComponent<SpriteRenderer>().material.SetFloat("_Seed", Random.Range(0f, 100f));
	}

    void Update() {
        
    }

	public void Collected() {
		GameManager.Instance.LevelManager.AddStar();
		GetComponent<PolygonCollider2D>().enabled = false;
		animator.SetBool("collected", true);
	}

	public void Reset() {
		if(WasCollected) {
			GetComponent<PolygonCollider2D>().enabled = true;
			StartIdleAnimation();
		}
	}

	private void StartIdleAnimation() {
		animator.SetBool("collected",false);
		float startTime = Random.Range(0, animator.GetCurrentAnimatorStateInfo(0).length);
		animator.Play("star", 0, startTime);
	}
}
