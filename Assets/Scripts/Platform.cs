using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
	public event EventHandler<Vector3> Moved;
	private Vector3 lastPosition;

	private HashSet<GameObject> passengers;

	BoxCollider2D box;

    void Start() {
		lastPosition = transform.position;
		passengers = new HashSet<GameObject>();
		box = GetComponent<BoxCollider2D>();
    }

    void LateUpdate() {
        if(lastPosition != transform.position) {
			Vector3 diff = transform.position - lastPosition;

			Moved?.Invoke(this, diff);
			foreach(GameObject t in passengers) {
				t.transform.position += diff;
			}

			lastPosition = transform.position;
		}
    }

	public void AddPassenger(GameObject pass) {
		passengers.Add(pass);
	}

	public void RemovePassenger(GameObject pass) {
		passengers.Remove(pass);
	}
}
