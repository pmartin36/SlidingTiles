using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borders : MonoBehaviour
{
	private float gravityDirection = -1f;
	private BoxCollider2D activeReset;

	private static LayerMask wallMask;
	private static LayerMask defaultMask = 0;

	public BoxCollider2D TopBorder;
	public BoxCollider2D BottomBorder;
	public BoxCollider2D LeftBorder;
	public BoxCollider2D RightBorder;

    void Start() {
		Player.gravityDirectionChanged += GravityDirectionChanged;
		activeReset = BottomBorder;
		wallMask = 1 << LayerMask.NameToLayer("Wall");
	}

    public void GravityDirectionChanged(object sender, float newGravity) {
		while(newGravity < 0) {
			newGravity += 360;
		}
		if(Mathf.Abs(newGravity - gravityDirection) > 5) {
			gravityDirection = newGravity;
			int dir = Mathf.FloorToInt(gravityDirection / 90f);
			switch(dir) {
				case 0:
					SetResetBorder(BottomBorder);
					break;
				case 1:
					SetResetBorder(RightBorder);
					break;
				case 2:
					SetResetBorder(TopBorder);
					break;
				case 3:
				default:
					SetResetBorder(LeftBorder);
					break;
			}
		}
	}

	private void SetResetBorder(BoxCollider2D border) {
		border.offset = 0.4f * Vector2.up;
		border.gameObject.layer = defaultMask;
		border.gameObject.tag = "Reset";

		activeReset.offset = Vector2.zero;
		activeReset.gameObject.layer = wallMask;
		activeReset.gameObject.tag = "Untagged";

		activeReset = border;
	}

	private void OnDestroy() {
		Player.gravityDirectionChanged -= GravityDirectionChanged;
	}
}
