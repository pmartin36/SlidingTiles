using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedChanger : MonoBehaviour
{
	public HashSet<ISpeedChangable> EffectedBodies;
	private float direction;

    void Start() {
		EffectedBodies = new HashSet<ISpeedChangable>();
		direction = Mathf.Sign(transform.localScale.x);
    }

    void Update() {
        foreach(var effected in EffectedBodies) {
			float eVx = effected.Vx;
			if (Mathf.Sign(eVx) * direction < 0) {
				effected.SetTemporarySpeed(eVx * 0.5f);
			}
			else {
				effected.SetTemporarySpeed(eVx * 3f);
			}		
		}
    }

	public void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.scene != this.gameObject.scene) return;

		ISpeedChangable s = collision.GetComponent<ISpeedChangable>();
		if (s != null) {
			EffectedBodies.Add(s);
		}
	}

	public void OnTriggerExit2D(Collider2D collision) {
		ISpeedChangable s = collision.GetComponent<ISpeedChangable>();
		if (s != null) {
			EffectedBodies.Remove(s);
		}
	}
}
