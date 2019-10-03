using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour, IPlatformMoveBlocker, IGravityChangable, ISpringable, ISpeedChangable {

	public event System.EventHandler<bool> aliveChanged;
	public static event System.EventHandler<float> gravityDirectionChanged;

    public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;

	public bool Ghost;

	public float Vx {
		get {
			return moveSpeed * moveDirection;
		}
	}
	private float accelerationTimeAirborne = .2f;
	private float accelerationTimeGrounded = .1f;
	private float moveSpeed = 6;
	private float? temporarySpeed;
	private float temporarySpeedTimer;

	private float gravity;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private Vector3 velocity;
	private float velocityXSmoothing;

	private Controller2D controller;
	private SpriteRenderer lights;

	private float moveDirection;
	private Vector3 spawnPosition;

	private RespawnManager RespawnManager;

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

	void Awake() {
		controller = GetComponent<Controller2D>();
		lights = GetComponentsInChildren<SpriteRenderer>().First(s => s.gameObject != this.gameObject);
	}

	void Start() {
		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
		moveDirection = 1f;	
		// player is set inactive in the respawn manager,
		// when switching to next level, we don't initialize LevelManager/RespawnManager 
		// until after the current scene is unloaded (see case WinTypeAction.Next)
	}

	public void SetRespawnManager(RespawnManager m) => RespawnManager = m;

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
		float vx = Mathf.Abs(velocity.x);
		velocity = maxJumpVelocity * 1.8f * direction;
		if(Mathf.Abs(direction.x) > 0.1f) {
			moveDirection = Mathf.Sign(direction.x);
		}
		velocity += Vector3.right * moveDirection * vx;
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
		float targetVelocity = moveSpeed;
		float smooth = 0.5f;
		if(temporarySpeed.HasValue) {
			targetVelocity = temporarySpeed.Value;
			smooth = 0.25f;
			temporarySpeedTimer -= Time.deltaTime;
			if(temporarySpeedTimer <= 0) {
				temporarySpeed = null;
			}
		}

		velocity.x = Mathf.SmoothDamp (Mathf.Abs(velocity.x), targetVelocity, ref velocityXSmoothing, smooth) * moveDirection;
		velocity.y += gravity * Time.deltaTime;
	}

	public bool CheckBlocking(ref Vector2 original, HashSet<Tile> tilesToMove) {
		Vector2 largestValidMoveAmount = original;
        Vector2 norm = original.normalized;
		float mag = original.magnitude;
		float skinWidth = 0.015f;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
			(Vector2)transform.position + controller.collider.offset * transform.lossyScale,
			controller.collider.size * transform.lossyScale - Vector2.one * 2 * skinWidth,
			transform.eulerAngles.z,
			norm,
			mag + skinWidth,
			controller.collisionMask
		);

        if(hits.Length > 0) {
			LayerMask border = LayerMask.NameToLayer("Wall");
			float min = mag;
			foreach(RaycastHit2D hit in hits) {
				float dist = (hit.distance - skinWidth);
				if(dist < min && hit.distance > 0) {
					if (hit.collider.gameObject.layer == border) {
						min = dist;
					}
					else {
						// only stop if this tile is not also moving
						// this only works presuming that the contents of the tile don't move independently of the tile
						PlatformController p = hit.collider.GetComponent<PlatformController>();
						if (!tilesToMove.Contains(p.Parent)) {
							min = dist;
						}
					}
				}
			}
			original = min * norm;
		}
		return false;
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		if (collision.CompareTag("Reset")) {
			SetAlive(false);
		}
		else if(!Ghost) {
			if (collision.CompareTag("Flag")) { 
				moveDirection = 0f;
				collision.GetComponent<GoalFlag>().PlayerReached();
			}
			else if(collision.CompareTag("Star")) {
				collision.GetComponent<Star>().Collected();
			}
		}
	}

	public void SetAlive(bool alive) {
		this.controller.collider.enabled = alive;
		this.enabled = alive;
		transform.position = RespawnManager.PlayerSpawnPosition;
		transform.localScale = Vector2.one * 1.2f;

		lights.color = alive ? Color.green : Color.red;

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

	public void SetTemporarySpeed(float speed) {
		temporarySpeed = Mathf.Abs(speed);
		temporarySpeedTimer = 1f;
	}
}
