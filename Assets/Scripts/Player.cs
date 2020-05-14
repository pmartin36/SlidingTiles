﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

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

	void Awake() {
		controller = GetComponent<Controller2D>();
		animator = GetComponent<Animator>();
		audio = GetComponent<AudioSource>();

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
							StartCoroutine(Vibrate());
						}

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
			collision.GetComponent<Star>().Collected(lastFramePositionDelta);
		}	
	}

	public void SetAlive(bool alive, bool skipAnimation = false) {
		if(!respawning) {
			bool aliveStateChanged = Alive != alive;
			Alive = alive;
			this.controller.collider.enabled = alive;
			moveDirection = 1f;
			ChangeGravityDirection(-1f);
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

				SetAnimationBool("Won", Won);
				SetAnimationFloat("Vx", 0f);
				SetAnimationBool("Alive", alive);
			}
		}
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

	public void JumpLanding() {
		
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
		float t = 0f;
		while (t < 0.5f) {
			flag.SetAudioVolume(t);
			t += Time.deltaTime;
			yield return null;
		}
		yield return new WaitUntil(() => controller.collisions.below); // wait for the player to hit the ground

		Won = true;
		yield return new WaitForSeconds(2f); // let player enjoy animation for a second
		GameManager.Instance.LevelManager.PlayerWinAnimation();

		// silence fireworks
		t = 0f;
		while (t < 2f) {
			float sm = Mathf.SmoothStep(0.5f, 0f, t / 2f);
			flag.SetAudioVolume(sm);
			t += Time.deltaTime;
			yield return null;
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

	private IEnumerator Vibrate(YieldInstruction yieldinstruction = null) {
		Vibration.VibratePop();
		yield return yieldinstruction;
		Vibration.VibratePeek();
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

			t += Time.deltaTime;
			yield return null;
		}
		sr.material.SetFloat("_DistortRadius", 0.6f);
		blur.intensity.value = 2;
		transform.localScale = 1.44f * Vector3.one;

		// move
		respawnEffects.PlayMoveClip();
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

		yield return waitQuarter;
		c.EnablePostEffects(false);
		respawnEffects.gameObject.SetActive(false);
		respawning = false;

		// re-enable action buttons
		aliveChanged?.Invoke(this, false);
	}
}
