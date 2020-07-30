using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
	private Animator animator;
	private AudioSource audio;

    void Start() {
		animator = GetComponent<Animator>();
		audio = GetComponent<AudioSource>();

		float yx = transform.localScale.x / transform.localScale.y;
		SpringIndicator indicator = GetComponentInChildren<SpringIndicator>();
		Transform it = indicator.transform;
		it.localScale = new Vector3(it.localScale.y / yx, it.localScale.y);
	}

    public void Sprung() {
		animator.Play("spring");
		audio.Play();
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
		if (s != null) {
			Vector2 direction = Utils.AngleToVector(transform.rotation.eulerAngles.z).Rotate(90); // default up
			Vector2 ga = Utils.AngleToVector(s.GravityAngle).Rotate(-90); // default down
			// Debug.DrawRay(collision.transform.position, ga, Color.blue);
			// Debug.DrawRay(this.transform.position, direction, Color.red);

			if(Vector2.Dot(direction, ga) < -0.75f) {
				// verify center or back of ispringable collider hits the spring
				Vector2 size = collision.transform.lossyScale * (collision as BoxCollider2D).size;
				// Debug.DrawRay(collision.transform.position, ga * ((size.y / 2f) + 0.015f), Color.magenta);
				RaycastHit2D hit = Physics2D.Raycast(
					collision.transform.position,
					ga,
					(size.y / 2f) + 0.015f,
					1 << this.gameObject.layer);
				if(!hit) {
					Vector3 offset = (Vector2.left.Rotate(collision.transform.eulerAngles.z) * Mathf.Sign(s.Vx));
					// Debug.DrawRay(collision.transform.position + offset, ga * ((size.y / 2f) + 0.015f), Color.magenta);
					hit = Physics2D.Raycast(
						collision.transform.position + offset,
						ga,
						(size.y / 2f) + 0.015f,
						1 << this.gameObject.layer);

					if(!hit)
						return;
				}
			};

			s.Spring(direction.Rotate(-s.GravityAngle));
			Sprung();
		}
	}
}
