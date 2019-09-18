using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour, ISquishable, IGravityChangable, ISpringable {

	public event System.EventHandler<bool> aliveChanged;
	public static event System.EventHandler<float> gravityDirectionChanged;

    public bool WasSquishedThisFrame { get; set; }
	public Vector3 UnsquishedDimensions { get; set; }

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

	private Vector3 initialPosition;

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

		UnsquishedDimensions = transform.localScale;
		initialPosition = transform.position;
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

	void LateUpdate() {
		
	}

	public void Spring(Vector2 direction) {
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
		//if (controller.collisions.slidingDownMaxSlope) {
		//	if (moveDirection != -Mathf.Sign (controller.collisions.slopeNormal.x)) { // not jumping against max slope
		//		velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
		//		velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
		//	}
		//} 
		velocity = maxJumpVelocity * 1.8f * direction;
		if(Mathf.Abs(direction.x) > 0.1f) {
			moveDirection = Mathf.Sign(direction.x);
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

        velocity.x = bumpVelocity.x;
	}

	void CalculateVelocity() {
		//float targetVelocityX = moveDirection * moveSpeed;
		velocity.x = Mathf.SmoothDamp (Mathf.Abs(velocity.x), moveSpeed, ref velocityXSmoothing, 0.5f) * moveDirection;
		velocity.y += gravity * Time.deltaTime;
	}

	public bool CheckSquishedAndResolve(Vector2 original) {
		Vector2 largestValidMoveAmount = original;
        Vector2 norm = original.normalized;
		float skinWidth = 0.015f;
        RaycastHit2D hit = Physics2D.BoxCast(
			transform.position,
			controller.collider.size * transform.lossyScale - Vector2.one * 2 * skinWidth,
			transform.eulerAngles.z,
			original.normalized,
			original.magnitude + skinWidth,
			controller.collisionMask
		);

        if(hit) {
			float amountToShrink = (original.magnitude - (hit.distance - skinWidth));
			if(Mathf.Min(transform.localScale.x, transform.localScale.y) - amountToShrink < 1f) {
				Destroy(this.gameObject);
			}
			else {
				Destroy(this.gameObject);

				// replace with logic to squish player
				//Vector2 localScale = transform.localScale;
				//float mag = localScale.magnitude;
				//transform.localScale = (localScale - norm * amountToShrink).normalized * mag;
				//WasSquishedThisFrame = true;
			}
			return true;
		}

		return false;
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		if(collision.CompareTag("Flag")) { 
			moveDirection = 0f;
			collision.GetComponent<GoalFlag>().PlayerReached();
		}
		else if(collision.CompareTag("Star")) {
			collision.GetComponent<Star>().Collected();
		}
		else if(collision.CompareTag("Reset")) {
			SetAlive(false);
		}
	}

	public void SetAlive(bool alive) {
		this.gameObject.SetActive(alive);
		transform.position = initialPosition;
		moveDirection = 1f;
		ChangeGravityDirection(-1f);
		velocity = Vector2.zero;
		aliveChanged?.Invoke(this, alive);
	}

	public void OnDestroy() {
		aliveChanged = null;
	}

	public void ChangeGravityDirection(float g) {
		if(Mathf.Sign(gravity) * Mathf.Sign(g) < 0.0001f) {
			gravity = Mathf.Abs(gravity) * g;
			gravityDirectionChanged?.Invoke(this, g);
		}
	}
}
