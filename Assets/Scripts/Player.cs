using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour, IPlatformMoveBlocker, IGravityChangable, ISpringable, ISpeedChangable {

	public event System.EventHandler<bool> aliveChanged;
	public static event System.EventHandler<float> gravityDirectionChanged;

    public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;

	public bool Alive { get; private set; }

	public float Vx => moveSpeed * moveDirection;
	public bool Grounded => controller.collisions.below;
	public Vector3 Direction => new Vector3(moveDirection, Mathf.Sign(gravity));

	private float accelerationTimeAirborne = .2f;
	private float accelerationTimeGrounded = .1f;
	private float moveSpeed = 9;
	private float? temporarySpeed;
	private float temporarySpeedTimer;

	private float gravity;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	[SerializeField]
	private Vector3 velocity;
	private Vector3 lastFrameVelocity;
	private float velocityXSmoothing;

	private Controller2D controller;
	private SpriteRenderer lights;

	private float moveDirection;
	private Vector3 spawnPosition;

	private RespawnManager RespawnManager;
	private Animator animator;

	private bool Won { get; set; }

	void Awake() {
		controller = GetComponent<Controller2D>();
		lights = GetComponentsInChildren<SpriteRenderer>().First(s => s.gameObject != this.gameObject);
		animator = GetComponent<Animator>();
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

	void FixedUpdate() {
		CalculateVelocity();

		// dont include gravity in jump calculations
		velocity.y -= gravity * Time.fixedDeltaTime;
		bool jumping = DetermineJump(velocity * Time.fixedDeltaTime, out var modifiedMove);
		if(!jumping) {
			velocity.y += gravity * Time.fixedDeltaTime;
		}

		// actually perform movement
		Vector2 amountMoved = controller.Move (velocity * Time.fixedDeltaTime);

		if (controller.collisions.above || controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.fixedDeltaTime;
			} else {
				velocity.y = 0;

				
				float absLastY = Mathf.Abs(lastFrameVelocity.y);
				float sign = Mathf.Sign(lastFrameVelocity.y);
				// 1 tile fall, 30ish
				// 2 tile fall, 40ish
				// 3 tile fall, 50ish
				if (absLastY > 10f) {
					float absV = Mathf.Abs(velocity.x);
					float lerp = Mathf.Clamp01((absLastY - 25f) / 25f) * moveSpeed;
					float newV = Mathf.Max(0, absV - lerp);

					velocity.x = newV;
					velocityXSmoothing = newV;

					if(lerp > 3) {
						StartCoroutine(Vibrate());
					}

					// little screen shake
					//CameraManager.Instance.CameraController.Shake(
					//	1f,
					//	a * 0.3f,
					//	Vector2.up * sign * a,
					//	Vector2.down * sign * a * 0.9f
					//);
				}
			}
		}

		if((moveDirection > 0.1f && controller.collisions.right) || (moveDirection < -0.1f && controller.collisions.left)) {
			moveDirection *= -1f;
		}

		// swap player direction only when it starts moving the other way, otherwise it swaps rapidly when smushed between two objects
		if (amountMoved.sqrMagnitude > 0.001f && transform.localScale.x * amountMoved.x < 0) {
			bool executeSwap = true;
			// if collider if offset, verify that when we swap direction, we're not putting the collider inside another collider
			if(Mathf.Abs(controller.collider.offset.x) > 0.001f) {
				var hit = Physics2D.OverlapBox(
					(Vector2)transform.position - Vector2.right * transform.lossyScale.x * controller.collider.offset.x,
					transform.lossyScale * controller.collider.size - (2 * 0.015f) * Vector2.one,
					0,
					controller.collisionMask
				);
				executeSwap = hit == null;
			}
			if(executeSwap) {
				Vector3 localScale = transform.localScale;
				localScale.x *= -1f;
				transform.localScale = localScale;
			}
		}

		lastFrameVelocity = velocity;
	}

	void LateUpdate() {
		if(!Won) {
			lights.color = Alive ? Color.green : Color.red;
		}
	}

	// TODO: I don't like this, we should implement an abstract class or an interface requires this class to have a composition Jumper object
	public bool DetermineJump(Vector3 move, out ValueTuple<bool, Vector3> modifiedMove) {
		modifiedMove = (false, Vector3.zero);
		
		// don't jump if move amount is an opposite direction of velocity
		float sign = Mathf.Sign(move.x);
		float gravityDirection = Mathf.Sign(gravity);
		if (sign * Mathf.Sign(velocity.x) < 0) {
			return false;
		}

		if (controller.collisions.below && velocity.y < -gravityDirection) {
			float absX = Mathf.Abs(velocity.x);
			float minDistanceCanJumpFrom = absX * Time.fixedDeltaTime * 8f;
			float maxDistanceCanJumpFrom = absX * Time.fixedDeltaTime * 10f;
			float jumpRange = maxDistanceCanJumpFrom - minDistanceCanJumpFrom;

			Vector2 castLength = move + 9 * velocity * Time.fixedDeltaTime;
			bool hitJumpableObject = controller.CheckForJumpableObjects(castLength, out float heightToJump, out float distanceToObstacle);
			if (hitJumpableObject) {
				//if we're starting this frame inside a valid jump range
				bool hitInValidRange = distanceToObstacle > minDistanceCanJumpFrom && distanceToObstacle < maxDistanceCanJumpFrom;

				// if we're starting this frame further than the valid jump range, and ending PAST the valid jump range
				float diffFromMaxJump = distanceToObstacle - maxDistanceCanJumpFrom;
				bool willPassoverValidRange = (diffFromMaxJump >= 0) && (move.x >= diffFromMaxJump + jumpRange);
				if(hitInValidRange || willPassoverValidRange) {
					velocity.y = Mathf.Max(heightToJump + 0.1f, 0.98f) * 10 * -gravityDirection; // 0.1f is a little buffer, max is because the baby jumps would end too early

					modifiedMove.Item1 = true;
					if (willPassoverValidRange) {
						modifiedMove.Item2 = new Vector3((sign * diffFromMaxJump), 0);
					}

					// DEBUG
					float pct = (distanceToObstacle - modifiedMove.Item2.x - minDistanceCanJumpFrom) / (maxDistanceCanJumpFrom - minDistanceCanJumpFrom);
					string str = $"Doing it at velocity: {velocity.y:.00}, height: {heightToJump}, pct: {pct}";
					if(modifiedMove.Item1) {
						str += $", excessMove: {modifiedMove.Item2.x:0.0}";
					}
					Debug.Log(str);
					// END DEBUG

					return true;
				}
			}
		}

		return false;
	}

	public void Spring(Vector2 direction) {
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
		float smooth = 1.2f;
		if(temporarySpeed.HasValue) {
			targetVelocity = temporarySpeed.Value;
			smooth = 0.25f;
			temporarySpeedTimer -= Time.fixedDeltaTime;
			if(temporarySpeedTimer <= 0) {
				temporarySpeed = null;
			}
		}

		velocity = new Vector2(
			Mathf.SmoothDamp (Mathf.Abs(velocity.x), targetVelocity, ref velocityXSmoothing, smooth) * moveDirection,
			velocity.y + gravity * Time.fixedDeltaTime
		);
	}

	public bool CheckBlocking(ref Vector2 original, HashSet<Tile> tilesToMove) {
		Vector2 largestValidMoveAmount = original;
        Vector2 norm = original.normalized;
		float mag = original.magnitude;
		float skinWidth = 0.015f;
		Vector2 positiveLossyScale = transform.lossyScale * new Vector2(Mathf.Sign(transform.lossyScale.x), Mathf.Sign(transform.lossyScale.y));
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
			(Vector2)transform.position + controller.collider.offset * positiveLossyScale,
			controller.collider.size * positiveLossyScale - Vector2.one * 2 * skinWidth,
			transform.eulerAngles.z,
			norm,
			mag + skinWidth,
			controller.collisionMask
		);

		if (hits.Length > 0) {
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
		if (collision.gameObject.scene != this.gameObject.scene) return;

		if (collision.CompareTag("Reset")) {
			StartCoroutine(Vibrate(new WaitForSeconds(0.1f)));
			SetAlive(false);
		}
		if (collision.CompareTag("Flag")) { 
			moveDirection = 0f;
			StartCoroutine(FlagReached(collision.GetComponent<GoalFlag>()));
		}
		else if(collision.CompareTag("Star")) {
			collision.GetComponent<Star>().Collected();
		}	
	}

	public void SetAlive(bool alive) {
		if (Alive != alive) {
			aliveChanged?.Invoke(this, alive);
		}

		Alive = alive;
		this.controller.collider.enabled = alive;
		this.enabled = alive;
		transform.position = RespawnManager.PlayerSpawnPosition;
		transform.localScale = Vector2.one * 1.2f;

		moveDirection = 1f;
		ChangeGravityDirection(-1f);
		velocity = Vector2.zero;
		Won = false;
		animator.SetBool("Won", Won);
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

	private IEnumerator FlagReached(GoalFlag flag) {
		flag.PlayerReached();
		GameManager.Instance.LevelManager.PlayerWin(flag);
		yield return new WaitUntil(() => controller.collisions.below); // wait for the player to hit the ground
		Won = true;
		animator.SetBool("Won", Won); //start animation for reaching flag
		yield return new WaitForSeconds(1f); // let player enjoy animation for a second
		GameManager.Instance.LevelManager.PlayerWinAnimation();
	}

	private IEnumerator Vibrate(YieldInstruction yieldinstruction = null) {
		Vibration.VibratePop();
		yield return yieldinstruction;
		Vibration.VibratePeek();
	}
}
