using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borders : MonoBehaviour
{
	private float GravityDirection = -1f;

	public BoxCollider2D TopBorder;
	public BoxCollider2D BottomBorder;

    void Start() {
		Player.gravityDirectionChanged += GravityDirectionChanged;
    }

    public void GravityDirectionChanged(object sender, float newGravity) {
		if(newGravity * GravityDirection < 0.0001f) {
			float dir = Mathf.Sign(newGravity);
			TopBorder.transform.position += Vector3.up * dir * 2;
			BottomBorder.transform.position += Vector3.up * dir * 2;

			if(dir > 0.0001f) {
				TopBorder.isTrigger = true;
				BottomBorder.isTrigger = false;
			}
			else {
				TopBorder.isTrigger = false;
				BottomBorder.isTrigger = true;
			}
			GravityDirection = dir;
		}
	}

	private void OnDestroy() {
		Player.gravityDirectionChanged -= GravityDirectionChanged;
	}
}
