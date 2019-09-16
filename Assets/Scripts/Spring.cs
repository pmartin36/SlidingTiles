using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
	private Animator animator;

    void Start() {
		animator = GetComponent<Animator>();
    }

	public Vector2 GetSpringDirection() {
		return Utils.AngleToVector(transform.rotation.eulerAngles.z).Rotate(90);
	}

    public void Sprung() {
		animator.Play("spring");
	}
}
