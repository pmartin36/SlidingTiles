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
		ExecuteSpring(collision);
	}

	public void OnTriggerStay2D(Collider2D collision) {
		ExecuteSpring(collision);
	}

	public void ExecuteSpring(Collider2D collision) {
		if (collision.gameObject.scene != this.gameObject.scene) return;

		ISpringable s = collision.GetComponent<ISpringable>();
		if (s != null && s.Grounded) {
			if(s.Vx > 0) {
				if(collision.bounds.max.x < transform.position.x) return;
			}
			else {
				if (collision.bounds.min.x > transform.position.x) return;
			}


			s.Spring(direction);
			Sprung();
		}
	}
}
