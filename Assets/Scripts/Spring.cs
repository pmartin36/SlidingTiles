using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
	private Animator animator;
	private Vector2 direction;

    void Start() {
		animator = GetComponent<Animator>();
		direction = Utils.AngleToVector(transform.rotation.eulerAngles.z).Rotate(90);
	}

    public void Sprung() {
		animator.Play("spring");
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		ISpringable s = collision.GetComponent<ISpringable>();
		if( s != null ) {
			s.Spring(direction);
			Sprung();
		}
	}
}
