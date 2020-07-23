using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour, IPlatformMoveBlocker, IGravityChangable, ISpringable, ISpeedChangable {

	public event System.EventHandler<bool> aliveChanged;
	public static event System.EventHandler<float> gravityDirectionChanged;

	public bool Alive { get; private set; }
	public bool Paused { get; set; }

	public float Vx => moveSpeed * moveDirection;
	public bool Grounded => controller.collisions.below;
	public Vector3 Direction => new Vector3(moveDirection, Mathf.Sign(gravity));

	private float moveSpeed = 9;
	private float? temporarySpeed;
	private float temporarySpeedTimer;

	private float gravity;
	private float maxJumpVelocity;
	[SerializeField]
	private Vector3 velocity;
	private Vector3 lastFrameVelocity;
	private float velocityXSmoothing;

	private Vector3 lastFramePosition;
	private Vector3 lastFramePositionDelta;

	private Controller2D controller;

	private float moveDirection;
	private Vector3 spawnPosition;

	private RespawnManager RespawnManager;
	private Animator animator;
	private AudioSource audio;

	private ParticleSystem footfallParticles;
	private ParticleSystem heavyLandParticles;

	public AudioClip WalkSoundClip;
	public AudioClip LandSoundClip;

	public Animator PortraitAnimator;

	private bool respawning;
	private PlayerRespawnEffects respawnEffects;

	private bool Won { get; set; }
	public float GravityAngle => controller.GravityAngle;

	void Awake() {
		controller = GetComponent<Controller2D>();
		animator = GetComponent<Animator>();
		audio = GetComponent<AudioSource>();

		PortraitAnimator.GetComponent<Image>().material.SetFloat("_OverridePct", 0f);

		var ps = GetComponentsInChildren<ParticleSystem>();
		footfallParticles = ps.First(p => p.name == "footfallParticles");
		footfallParticles.Stop();
		heavyLandParticles = ps.First(p => p.name == "heavyLandParticles");
		heavyLandParticles.Stop();

		respawnEffects = GetComponentInChildren<PlayerRespawnEffects>();
		respawnEffects.gameObject.SetActive(false);
	}

	void Start() {
		float timeToJumpApex = 0.4f;
		float maxJumpHeight = 4;

		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		moveDirection = 1f;	
		lastFramePosition = transform.position;
		// player is set inactive in the respawn manager,
		// when switching to next level, we don't initialize LevelManager/RespawnManager 
		// until after the current scene is unloaded (see case WinTypeAction.Next)
	}

	public void SetRespawnManager(RespawnManager m) => RespawnManager = m;

	void FixedUpdate() {
		if(!Paused && Alive) {
			Vector2 v;
			if(!Won) {
				CalculateVelocity();
				controller.GravityAngle = transform.eulerAngles.z;

				// dont include gravity in jump calculations
				velocity.y -= gravity * Time.fixedDeltaTime;
				bool jumping = DetermineJump(velocity * Time.fixedDeltaTime, out var modifiedMove);
				if(!jumping) {
					velocity.y += gravity * Time.fixedDeltaTime;
				}
				v = velocity.Rotate(controller.GravityAngle);
			}
			else {
				v = velocity;
			}

			// perform movement
			// if won, movement comes from bring player to ground
			// otherwise, comes from physics
			Vector2 amountMoved = controller.Move (v * Time.fixedDeltaTime);

			if(!Won) {
				if (controller.collisions.above || controller.collisions.below) {
					if (controller.collisions.slidingDownMaxSlope) {
						velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.fixedDeltaTime;
					}
					else {
						velocity.y = 0;

						float vy = Mathf.Abs(lastFrameVelocity.y);
						float sign = Mathf.Sign(lastFrameVelocity.y);
						// 1 tile fall, 30ish
						// 2 tile fall, 40ish
						// 3 tile fall, 50ish
						if (vy > 10f) {
							float absV = Mathf.Abs(velocity.x);
							float raw = Mathf.Clamp01((vy - 25f) / 25f);
							float lerp = raw * moveSpeed;
							float newV = Mathf.Max(0, absV - lerp);

							velocity.x = newV;

							if(lerp > 3) {
								MMVibrationManager.Haptic(HapticTypes.MediumImpact);
							}

							var lp = heavyLandParticles.transform.localPosition;
							lp.y = Mathf.Abs(lp.y) * sign;
							heavyLandParticles.transform.localPosition = lp;

							heavyLandParticles.Play();
							if (audio.clip != LandSoundClip) {
								audio.clip = LandSoundClip;
							}
							PlaySound(Mathf.Lerp(0.25f, 1.5f, raw), Mathf.Lerp(1, 0.9f, raw));

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
			}

			lastFrameVelocity = velocity;
		}
	}

	void LateUpdate() {
		lastFramePositionDelta = this.transform.position - lastFramePosition;
		lastFramePosition = this.transform.position;
	}

	void Update() {
		if (Alive) {
			if(Paused) {
				SetAnimationFloat("Vx", 0f);
			}
			else {
				float vxAbs = Mathf.Abs(velocity.x);
				float inv = Mathf.InverseLerp(0f, 15f, vxAbs);
				SetAnimationFloat("Vx", Mathf.Lerp(0.15f, 1.6f, inv));
			}
		}
		SetAnimationBool("Grounded", Grounded);
		SetAnimationFloat("Vy", velocity.y);
	}

	// TODO: I don't like this, should implement an abstract class or an interface requires this class to have a composition Jumper object
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
					//float pct = (distanceToObstacle - modifiedMove.Item2.x - minDistanceCanJumpFrom) / (maxDistanceCanJumpFrom - minDistanceCanJumpFrom);
					//string str = $"Doing it at velocity: {velocity.y:.00}, height: {heightToJump}, pct: {pct}";
					//if(modifiedMove.Item1) {
					//	str += $", excessMove: {modifiedMove.Item2.x:0.0}";
					//}
					//Debug.Log(str);
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

		if (collision.CompareTag("Reset") && !Won) {
			MMVibrationManager.Haptic(HapticTypes.Failure);
			SetAlive(false);
		}
		if (collision.CompareTag("Flag")) { 
			StartCoroutine(FlagReached(collision.GetComponent<GoalFlag>()));
		}
		else if(collision.CompareTag("Star")) {
			collision.GetComponent<Star>().Collected(lastFramePositionDelta);
		}	
	}

	public void SetAlive(bool alive, bool skipAnimation = false) {
		if(!respawning) {
			bool aliveStateChanged = Alive != alive;
			Alive = alive;
			this.controller.collider.enabled = alive;
			moveDirection = 1f;
			ChangeGravityDirection(0f);
			lastFrameVelocity = Vector2.zero;
			velocity = Vector2.zero;
			Won = false;

			bool animating = false;

			if (aliveStateChanged) {
				if(!alive && !skipAnimation) {
					animating = true;
					StartCoroutine(Respawn());
				}
				else {
					aliveChanged?.Invoke(this, alive);
				}
			}

			if (!animating) {
				transform.position = RespawnManager.PlayerSpawnPosition;
				transform.localScale = Vector2.one * 1.2f;
				transform.rotation = RespawnManager.PlayerSpawnRotation;
				controller.GravityAngle = RespawnManager.PlayerSpawnRotation.eulerAngles.z;

				SetAnimationBool("Won", Won);
				SetAnimationFloat("Vx", 0f);
				SetAnimationBool("Alive", alive);
			}
		}
	}

	public void OnDestroy() {
		aliveChanged = null;
	}

	public void ChangeGravityDirection(float direction) {
		if(controller.GravityAngle != direction) {
			velocity = velocity.Rotate(direction - controller.GravityAngle) * 0.5f;
			controller.GravityAngle = direction;
			gravityDirectionChanged?.Invoke(this, controller.GravityAngle);
		}
	}

	public void SetTemporarySpeed(float speed) {
		temporarySpeed = Mathf.Abs(speed);
		temporarySpeedTimer = 1f;
	}

	public bool CanUnpause() {
		float skinWidth = 0.03f;
		Vector2 positiveLossyScale = transform.lossyScale * new Vector2(Mathf.Sign(transform.lossyScale.x), Mathf.Sign(transform.lossyScale.y));
		Collider2D hit = Physics2D.OverlapBox(
			(Vector2)transform.position + controller.collider.offset * positiveLossyScale,
			controller.collider.size * positiveLossyScale - new Vector2(2, 10) * skinWidth,
			transform.eulerAngles.z,
			controller.collisionMask
		);
		if(hit) {
			StopCoroutine(UnpauseFailed());
			StartCoroutine(UnpauseFailed());
			return false;
		}
		return true;
	}

	public void SetPaused(bool paused) {
		this.controller.collider.enabled = !paused && Alive;
		Paused = paused;
	}

	public void JumpLanding() {}

	public void MoveFromRotation(Vector3 amount) {
		controller.Move(amount);
	}

	public void Footfall() {
		var vx = Mathf.Abs(this.velocity.x);
		footfallParticles.Play();

		float freq = 1f + 0.1f * UnityEngine.Random.value;
		float volume = 0.2f + 0.2f * UnityEngine.Random.value;
		if(audio.clip != WalkSoundClip) {
			audio.clip = WalkSoundClip;
		}
		PlaySound(volume, freq);
	}

	public void PlaySound(float volume, float frequency) {
		audio.volume = volume * GameManager.Instance.SaveData.FxVolume;
		audio.pitch = frequency;
		audio.Play();
	}

	private void SetAnimationBool(string key, bool value) {
		animator.SetBool(key, value);
		PortraitAnimator.SetBool(key, value);
	}

	private void SetAnimationFloat(string key, float value) {
		animator.SetFloat(key, value);
		PortraitAnimator.SetFloat(key, value);
	}

	private IEnumerator FlagReached(GoalFlag flag) {
		flag.PlayerReached();
		GameManager.Instance.LevelManager.PlayerWin(flag);
		SetAnimationBool("Won", true); //start animation for reaching flag
		
		//play fireworks
		StartCoroutine(flag.SlideVolume(0.5f, 0f, 0.5f));

		Won = true;
		yield return StartCoroutine(BringPlayerToGround(flag)); // wait for the player to hit the ground

		yield return new WaitForSeconds(2f); // let player enjoy animation for a second
		GameManager.Instance.LevelManager.PlayerWinAnimation();

		// silence fireworks
		yield return StartCoroutine(flag.SlideVolume(2, 0.5f, 0f));
		flag.Reset();
	}

	private IEnumerator BringPlayerToGround(GoalFlag flag) {
		float targetRotation = flag.transform.eulerAngles.z;
		float targetDistance = (controller.collider.size.y/2f - controller.collider.offset.y - RaycastController.skinWidth) * transform.lossyScale.y;
		Vector3 perpDirection = Vector3.down.Rotate(targetRotation);
		velocity = velocity.Rotate(controller.GravityAngle);
		controller.GravityAngle = targetRotation;

		// this should be the platform the flag sits on
		RaycastHit2D baseHit = Physics2D.Raycast(flag.transform.position, perpDirection, 10, controller.platformMask);
		Collider2D baseCollider = baseHit.collider;

		float distanceToMove = 0f;
		bool rayHitBase = false;
		int maxHits = 0;
		Vector3 parallelDirection = perpDirection.Rotate(-90);

		if (Mathf.Sign(perpDirection.y) * Mathf.Sign(gravity) < 0) {
			velocity.y = 0;
		}

		Func<Vector3, List<RaycastHit2D>> GetHits = (Vector3 center) => {
			Vector3 halfSize = Vector3.right.Rotate(targetRotation) * ((transform.lossyScale.x * controller.collider.size.x / 2f) - RaycastController.skinWidth);
			RaycastHit2D leftHit = Physics2D.Raycast(center + halfSize, perpDirection, 10, controller.platformMask);
			RaycastHit2D rightHit = Physics2D.Raycast(center - halfSize, perpDirection, 10, controller.platformMask);
			List<RaycastHit2D> hits = new List<RaycastHit2D>();
			if(leftHit.collider == baseCollider) hits.Add(leftHit);
			if(rightHit.collider == baseCollider) hits.Add(rightHit);
			return hits;
		};

		Func<Vector3, Vector3> GetPerpDirectionVelocity = (Vector3 expectedParallelVelocity) => {
			List<RaycastHit2D> hits = GetHits(transform.position + expectedParallelVelocity * Time.fixedDeltaTime);
			maxHits = Mathf.Max(hits.Count, maxHits);
			rayHitBase = hits.Count >= maxHits;
			if(hits.Count == 0) {
				hits = GetHits(transform.position);
			}
			baseHit = hits[0];

			float amountToMove = baseHit.distance - targetDistance;
			distanceToMove = Mathf.Abs(amountToMove);

			Vector3 expectedVelocity = Vector3.Project(velocity, perpDirection) + Mathf.Abs(gravity) * Time.fixedDeltaTime * perpDirection * Mathf.Sign(amountToMove);
			if ((expectedVelocity * Time.fixedDeltaTime).magnitude > distanceToMove) {
				return amountToMove * perpDirection / Time.fixedDeltaTime;
			}
			else {
				return expectedVelocity;
			}
		};

		//in the first t seconds, the player should orient itself with the base, slow parallel direction movement to 0, and move towards movementDirection
		float t = 0.25f;
		float timeElapsed = 0f;
		var fixedDelta = new WaitForFixedUpdate();
		yield return fixedDelta;
		while(timeElapsed < t) {
			float diffFromTargetRotation = targetRotation - transform.eulerAngles.z;
			while(Mathf.Abs(diffFromTargetRotation) > 360) {
				diffFromTargetRotation -= Mathf.Sign(diffFromTargetRotation) * 360;
			}
			transform.Rotate(0, 0, diffFromTargetRotation * 0.1f);

			Vector3 expectedParallelVelocity = Vector3.Project(velocity * 0.9f,  parallelDirection);
			Vector3 perpDirectionVelocity = GetPerpDirectionVelocity(expectedParallelVelocity);

			Debug.DrawRay(transform.position + expectedParallelVelocity * Time.fixedDeltaTime, perpDirection, Color.green, 1f);
			Debug.DrawLine(transform.position + expectedParallelVelocity * Time.fixedDeltaTime, baseHit.point, Color.magenta, 1f); 
			if(rayHitBase) {
				velocity = expectedParallelVelocity + perpDirectionVelocity;
			}
			else {
				// drifted too far
				velocity = 0f * parallelDirection + perpDirectionVelocity;
			}
			Debug.Log(velocity);
			timeElapsed += Time.fixedDeltaTime;
			yield return fixedDelta;
		}
		transform.rotation = Quaternion.Euler(0,0,targetRotation);
		velocity = GetPerpDirectionVelocity(Vector2.zero);
		while (distanceToMove > 0.1f) {
			Vector2 parallelVelocity = Vector2.zero;
			Vector2 moveDirectionVelocity = GetPerpDirectionVelocity(parallelVelocity);
			velocity = (parallelVelocity + moveDirectionVelocity);
			yield return fixedDelta;
		}
	}

	private IEnumerator UnpauseFailed() {
		float time = 0f;
		float animationTime = 0.5f;
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		Color start = Color.red;
		Color end = Color.white;
		while (time < animationTime) {
			sr.color = Color.Lerp(start, end, time / animationTime);
			time += Time.deltaTime;
			yield return null;
		}
		sr.color = end;
	}

	private IEnumerator Respawn() {
		respawning = true;
		CameraController c = CameraManager.Instance.CameraController;
		BlurComposite blur = c.GetModifiablePostProcessSettings<BlurComposite>();

		Vector3 startPosition = transform.position;
		SpriteRenderer sr = GetComponent<SpriteRenderer>();

		Image portrait = PortraitAnimator.GetComponent<Image>();

		Vector3 respawnPosition = RespawnManager.PlayerSpawnPosition;
		Vector3 diff = respawnPosition - startPosition;
		Vector3 cubicPoint = startPosition + new Vector3(-diff.x * 0.2f, diff.y * 1.5f, 0);

		float tShrinkGrow = 0.5f;
		float tMove = Mathf.InverseLerp(100, 400, diff.sqrMagnitude);
		tMove = Mathf.Max(tMove, 0.5f);

		Vector3 scaleNormal = transform.localScale.normalized;

		animator.enabled = false;
		RespawnManager.ActionButtons.SpawnButton.interactable = false;

		c.EnablePostEffects(true);
		respawnEffects.gameObject.SetActive(true);
		respawnEffects.PlayDeathClip();
		respawnEffects.PlayMoveClip();

		float t = 0;
		while (t < tShrinkGrow) {
			float v = t / tShrinkGrow;
			float r = Mathf.Lerp(1f, 0.6f, v);
			sr.material.SetFloat("_DistortRadius", r);
			portrait.material.SetFloat("_OverridePct", v * 0.5f);

			float b = Mathf.Lerp(1, 2, v * 1.1f);
			blur.intensity.value = b;

			float scale = Mathf.Lerp(1.2f, 1.44f, v);
			transform.localScale = scale * scaleNormal;

			respawnEffects.MoveClipVolume = Mathf.Max(v * 0.75f, respawnEffects.MoveClipVolume);

			t += Time.deltaTime;
			yield return null;
		}
		sr.material.SetFloat("_DistortRadius", 0.6f);
		blur.intensity.value = 2;
		transform.localScale = 1.44f * Vector3.one;
		transform.rotation = RespawnManager.PlayerSpawnRotation;
		controller.GravityAngle = RespawnManager.PlayerSpawnRotation.eulerAngles.z;

		// move
		t = 0;
		while (t < tMove) {
			float mt = Mathf.SmoothStep(0, 1, t/tMove);
			Vector3 pos = (1 - mt) * (1 - mt) * startPosition
							+ 2 * (1 - mt) * mt * cubicPoint
							+ mt * mt * respawnPosition;
			transform.position = pos;

			t += Time.deltaTime;
			yield return null;
		}
		transform.position = respawnPosition;

		// set animation parameters for the other side
		SetAnimationBool("Won", Won);
		SetAnimationFloat("Vx", 0f);
		SetAnimationBool("Alive", false);
		animator.enabled = true;

		// grow
		var waitQuarter = new WaitForSeconds(0.25f); 
		yield return waitQuarter; // particles last a second, growing takes 0.5f, so we add 0.25f before and after to take full second

		respawnEffects.PlayRespawnClip();
		t = 0;
		while (t < tShrinkGrow) {
			float v = t / tShrinkGrow;
			float r = Mathf.Lerp(0.6f, 1f, v);
			sr.material.SetFloat("_DistortRadius", r);
			portrait.material.SetFloat("_OverridePct", (1 - v) / 2f);

			float b = (1 - t) * 2;
			blur.intensity.value = b;

			float scale = Mathf.Lerp(1.44f, 1.2f, v);
			transform.localScale = scale * Vector3.one;

			t += Time.deltaTime;
			yield return null;
		}
		blur.intensity.value = 0;
		transform.localScale = 1.2f * Vector3.one;
		sr.material.SetFloat("_DistortRadius", 1f);
		portrait.material.SetFloat("_OverridePct", 0f);

		t = 0;
		while( t < 0.25f ) {
			float v = t / 0.25f;
			respawnEffects.MoveClipVolume = Mathf.Min(v, respawnEffects.MoveClipVolume);
			t += Time.deltaTime;
			yield return null;
		}
		c.EnablePostEffects(false);
		respawnEffects.gameObject.SetActive(false);
		respawning = false;

		// re-enable action buttons
		aliveChanged?.Invoke(this, false);
	}
}
