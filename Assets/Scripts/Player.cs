using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour, IMoveableCollider {

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;

	private float accelerationTimeAirborne = .2f;
	private float accelerationTimeGrounded = .1f;
	private float moveSpeed = 6;

	private float gravity;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private Vector3 velocity;
	private float velocityXSmoothing;

	private Controller2D controller;

	private float moveDirection;

	// Wall Stuff, will probably remove
	//public float wallSlideSpeedMax = 3;
	//public float wallStickTime = .25f;
	//private float timeToWallUnstick;
	//public float wallSlideSpeedMax = 3;
	//public float wallStickTime = .25f;
	//float timeToWallUnstick;
	//public Vector2 wallJumpClimb;
	//public Vector2 wallJumpOff;
	//public Vector2 wallLeap;
	//bool wallSliding;
	//int wallDirX;

	void Start() {
		controller = GetComponent<Controller2D> ();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
		moveDirection = 1f;
	}

	void Update() {
		CalculateVelocity ();
		// HandleWallSliding ();

		controller.Move (velocity * Time.deltaTime);

		if (controller.collisions.above || controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else {
				velocity.y = 0;
			}
		}

		if( !controller.collisions.climbingSlope
			&& ((moveDirection > 0.1f && controller.collisions.right) || (moveDirection < -0.1f && controller.collisions.left))
		) {
			moveDirection *= -1f;
		}
	}

	public void OnJumpInputDown() {
		//if (wallSliding) {
		//	if (wallDirX == moveDirection) {
		//		velocity.x = -wallDirX * wallJumpClimb.x;
		//		velocity.y = wallJumpClimb.y;
		//	}
		//	else if (moveDirection == 0) {
		//		velocity.x = -wallDirX * wallJumpOff.x;
		//		velocity.y = wallJumpOff.y;
		//	}
		//	else {
		//		velocity.x = -wallDirX * wallLeap.x;
		//		velocity.y = wallLeap.y;
		//	}
		//}
		if (controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				if (moveDirection != -Mathf.Sign (controller.collisions.slopeNormal.x)) { // not jumping against max slope
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			} else {
				velocity.y = maxJumpVelocity;
			}
		}
	}

	public void OnJumpInputUp() {
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}		

	void HandleWallSliding() {
		//wallDirX = (controller.collisions.left) ? -1 : 1;
		//wallSliding = false;
		//if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
		//	wallSliding = true;

		//	if (velocity.y < -wallSlideSpeedMax) {
		//		velocity.y = -wallSlideSpeedMax;
		//	}

		//	if (timeToWallUnstick > 0) {
		//		velocityXSmoothing = 0;
		//		velocity.x = 0;

		//		if (moveDirection != wallDirX && moveDirection != 0) {
		//			timeToWallUnstick -= Time.deltaTime;
		//		}
		//		else {
		//			timeToWallUnstick = wallStickTime;
		//		}
		//	}
		//	else {
		//		timeToWallUnstick = wallStickTime;
		//	}

		//}

	}

	public void SetVelocityFromBump(Vector2 bumpVelocity) {
		float absBump = Mathf.Abs(bumpVelocity.x);
		float absV = Mathf.Abs(velocity.x);
		if( absBump > absV ) {
			if(absV > absBump) {
				velocity.x = Mathf.Sign(velocity.x) * (absBump + absV);
			}
			else {
				velocity.x = Mathf.Sign(bumpVelocity.x) * (absBump + absV);
			}		
		}
		velocity.x = bumpVelocity.x
	}

	void CalculateVelocity() {
		float targetVelocityX = moveDirection * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}

	public Vector2 CalculateValidMoveAmount(Vector2 original) {
		Vector2 amt = original;
		RaycastHit2D[] hits = Physics2D.BoxCastAll(
			transform.position,
			controller.collider.size * transform.lossyScale,
			transform.eulerAngles.z,
			original.normalized,
			original.magnitude,
			controller.collisionMask
		);

		foreach (RaycastHit2D hit in hits) {
			// TODO: May have more than just platforms in the future
			IMoveableCollider collider = hit.collider.GetComponent<IMoveableCollider>();
			if(collider != null) {
				Vector2 moveAmount = collider.CalculateValidMoveAmount(amt - original.normalized * hit.distance);
				moveAmount += original.normalized * hit.distance;
				if (moveAmount.sqrMagnitude < amt.sqrMagnitude) {
					amt = moveAmount;
				}
			}
		}

		return amt;
	}
}
