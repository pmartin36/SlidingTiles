using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    void Start() {
		var animator = GetComponent<Animator>();
		float startTime = Random.Range(0, animator.GetCurrentAnimatorStateInfo(0).length);
		animator.Play("star", 0, startTime);
    }

    void Update() {
        
    }

	public void Collected() {
		GameManager.Instance.LevelManager.AddStar();
		this.gameObject.SetActive(false);
	}
}
