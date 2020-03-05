﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Tile : MonoBehaviour, IRequireResources
{
	public bool Loaded { get; set; }
	public const float BaseThreshold = 0.15f;
	private const float BaseThresholdSquared = BaseThreshold * BaseThreshold;

	private const float SpeedCap = 4f;

	public Tilespace Space { get; set; }

	public Movable Movable;

	public bool Selected { get; set; }
	public Vector2 PositionWhenSelected { get; set; }
	public Vector2 ResidualVelocity { get; set; }

	public bool Centered { get => transform.localPosition.sqrMagnitude < 0.000002f; } // (skinWidth / scale)^2 
	public Vector2 NormalizedPosition => new Vector2(Mathf.Abs(transform.localPosition.x), Mathf.Abs(transform.localPosition.y)).normalized;

	private LayerMask tileMask;
	protected LayerMask playerMask;
	private SpriteRenderer spriteRenderer;

	private PlatformController[] childPlatforms;

	private Vector3 lastFrameVelocity;
	private Vector3 lastFramePosition;

	private bool initialMovable;
	private Tilespace initialTilespace;

	private static int LoadedMaterialWorld = -1;
	private static Material ImmobileMaterial;
	private static Material SelectedMaterial;
	private static Material UnselectedMaterial;

	private static AsyncOperationHandle<Material> OnImmobileMaterialLoad;

	private void Awake() {
		playerMask = 1 << LayerMask.NameToLayer("Player");
	}

	private void Start() {
		tileMask = 1 << LayerMask.NameToLayer("Tile");
		spriteRenderer = GetComponent<SpriteRenderer>();

		if(GameManager.Instance.LastPlayedWorld != LoadedMaterialWorld && Movable) {
			LoadedMaterialWorld = GameManager.Instance.LastPlayedWorld;
		
			UnselectedMaterial = spriteRenderer.sharedMaterial;
			Addressables.LoadAssetAsync<Material>($"Level_SelectedTile").Completed +=
				(obj) =>
					{
						SelectedMaterial = obj.Result;
						Loaded = true;
					};
		}
		else {
			Loaded = true;
		}

		//if (!Movable) {
		//	OnImmobileMaterialLoad.Completed += (obj) => spriteRenderer.sharedMaterial = obj.Result;
		//}

		childPlatforms = GetComponentsInChildren<PlatformController>();
		lastFramePosition = transform.position;
		lastFrameVelocity = Vector3.zero;	

	}

	public void Init(Tilespace t) {
		this.Space = t;
		this.initialTilespace = t;
		this.initialMovable = this.Movable;
	}

	public virtual void FixedUpdate() {
		LevelManager lm = GameManager.Instance.LevelManager;
		if(ResidualVelocity.sqrMagnitude > 0.1f) {
			HashSet<Tile> tilesToMove = new HashSet<Tile>() { this };
			Vector2 moveDirection = ResidualVelocity.normalized;
			Direction direction = GetDirectionFromPosition(ref moveDirection);
			Vector3 moveAmount = ResidualVelocity * Time.fixedDeltaTime;
			Vector2 position = moveAmount + transform.localPosition;


			//if (Vector2.Dot(position, transform.localPosition) < 0) {
			//	moveAmount = -(Vector2)transform.localPosition;
			//}
			//if (CanMoveTo(ref moveAmount, tilesToMove, direction)) {
			//	foreach (Tile t in tilesToMove) {
			//		t.Move(moveAmount, direction);
			//	}
			//}


			TryMoveToPosition(position, moveAmount, true);
		}
		else if(lm != null && lm.SnapAfterDeselected && lm.SelectedTile == null && !Centered) {
			Vector3 position = new Vector3(Mathf.Round(transform.localPosition.x), Mathf.Round(transform.localPosition.y));
			Vector3 moveAmount = position - transform.localPosition;

			float distToMove = Time.fixedDeltaTime * SpeedCap;
			if (distToMove < moveAmount.magnitude) {
				moveAmount = moveAmount.normalized * distToMove;
			}

			HashSet<Tile> tilesToMove = new HashSet<Tile>() { this };
			Vector2 v2 = position;
			Direction direction = GetDirectionFromPosition(ref v2);
			if (CanMoveTo(ref moveAmount, tilesToMove, direction)) {
				foreach (Tile t in tilesToMove) {
					t.Move(moveAmount, direction);
				}
			}
			return;
		}
		// try centering selected tiles that are close to being centered
		else if (Selected && transform.localPosition.sqrMagnitude <= BaseThresholdSquared) {
			float distToMove = Time.fixedDeltaTime * SpeedCap;
			Vector3 moveAmount;
			if (distToMove > transform.localPosition.magnitude) {
				moveAmount = -transform.localPosition;
			}
			else {
				moveAmount = -transform.localPosition.normalized * distToMove;
			}

			var noDirection = Direction.None;
			HashSet<Tile> tilesToMove = new HashSet<Tile>() { this };
            if (CanMoveTo(ref moveAmount, tilesToMove, noDirection)) {
				foreach (Tile t in tilesToMove) {
					t.Move(moveAmount, noDirection);
				}
			}
		}
	}

	public virtual void SetResidualVelocity(Vector2 avgVelocity) {
		if(avgVelocity.sqrMagnitude > 1.5f) {
			ResidualVelocity = SpeedCap * avgVelocity.normalized;
		}
	}

	public virtual void Select(bool select) {
		Selected = select;
		if (Selected) {
			PositionWhenSelected = transform.position;
			ResidualVelocity = Vector2.zero;
		}
		spriteRenderer.sharedMaterial = Selected ? SelectedMaterial : UnselectedMaterial;
	}

	public void CompleteMove(Tilespace space) {
		this.Space = space;
		space.Tile = this;		
	}

	public virtual bool Move(Vector3 moveAmount, Direction d) {
		Vector2 globalMoveAmount = moveAmount * transform.lossyScale.x;

		foreach (PlatformController c in childPlatforms) {
			c.Premove(ref globalMoveAmount);
		}
		transform.localPosition += moveAmount;
		foreach (PlatformController c in childPlatforms) {
			c.Postmove(ref globalMoveAmount);
		}

		bool changedTilespaces = false;

		float mag = transform.localPosition.magnitude;
		if (mag > 1 - BaseThreshold) {
			// snap to next spot
			Tilespace next = Space.GetNeighborInDirection(d);		
			if(Space.Tile == this) {
				Space.Tile = null;
			}

			next.SetChildTile(this);
			changedTilespaces = true;
		}

		if (Space.Sticky && transform.localPosition.magnitude < BaseThresholdSquared) {
			this.SetMovable(false);
		}

		return changedTilespaces;
	}

	public bool CanMoveTo(ref Vector3 moveAmount, HashSet<Tile> tilesToMove, Direction d) {
		bool goingTowardCenter = (transform.localPosition + moveAmount).sqrMagnitude < transform.localPosition.sqrMagnitude;// && Vector2.Dot(localPosition, transform.localPosition) < 0;
		bool validTilespace = d.Value.sqrMagnitude < 0.1f || Space.GetNeighborInDirection(d) != null;
		if(validTilespace || goingTowardCenter) {
			Vector3 DebugOriginalMoveAmount = new Vector2(moveAmount.x, moveAmount.y);
			Vector2 size = Mathf.Abs(moveAmount.x) > 0.0001f 
				? new Vector2(moveAmount.magnitude * 0.5f, transform.lossyScale.x * 0.90f)
				: new Vector2(transform.lossyScale.x * 0.90f, moveAmount.magnitude * 0.5f);
			Collider2D[] collisions = 
				Physics2D.OverlapBoxAll(
					this.transform.position + moveAmount.normalized * (transform.lossyScale.x + moveAmount.magnitude + 0.001f) * 0.5f,
					size, 
					0,
					tileMask)
					.Where(r => r.gameObject != this.gameObject).ToArray();

			Vector2 globalMoveAmount = moveAmount * transform.lossyScale.x;
			foreach (PlatformController p in childPlatforms) {
				p.CheckBlocking(ref globalMoveAmount, tilesToMove);
			}
			Vector2 newMove = globalMoveAmount / transform.lossyScale.x;
			if(newMove.sqrMagnitude - moveAmount.sqrMagnitude > 0.1f) {
				// TODO (Audio): Play *dink* metal noise

			}
			moveAmount = newMove;

			if (collisions.Length == 0) {
				return true;
			}
			else if (collisions.Length == 1) {
				Debug.DrawRay(this.transform.position, transform.lossyScale * 0.5f + moveAmount, Color.red);
			
				Tile collidedTile = collisions[0].GetComponent<Tile>();
				bool canMoveInDirection = collidedTile.Centered || Mathf.Abs(Vector2.Dot(moveAmount.normalized, collidedTile.transform.localPosition.normalized)) > 0.1f;
				if (collidedTile.Movable && !collidedTile.Selected && canMoveInDirection) {		
					// Debug.DrawLine(this.transform.position, hit.transform.position, Color.blue, 0.25f);
					tilesToMove.Add(collidedTile);
					return collidedTile.CanMoveTo(ref moveAmount, tilesToMove, d);
				}
			}
		}
		return false;
	}

	public Vector2 GetPositionFromInput(Vector2 mouseMoveSinceSelection) {
		return mouseMoveSinceSelection + (this.PositionWhenSelected - (Vector2)Space.transform.position) / transform.lossyScale.x;
	}

	public bool TryMoveToPosition(Vector2 position, Vector2 delta, bool fromResidual = false) {
		if(Movable) {
			bool centered = Centered;
			float deltaTimeSpeedCap = SpeedCap * Time.fixedDeltaTime;

			if(!centered) {
				if(transform.localPosition.magnitude < BaseThresholdSquared) {
					transform.localPosition = Vector3.zero;
					centered = true;
				}
				else {
					Vector2 normalized = NormalizedPosition;
					var pNorm = position * normalized;
					float pNormMag = pNorm.magnitude;

					Vector2 orthogonalVector = (position - pNorm);
					float orthogonalVectorMag = orthogonalVector.magnitude;

					// if the direction orthogonal to the movement direction is larger, 
					// we'll assume player is trying to change direction and center the tile
					if (pNormMag < 1 
						&& orthogonalVectorMag > pNormMag
						&& orthogonalVectorMag > deltaTimeSpeedCap 
						&& orthogonalVectorMag > BaseThreshold * 2) {
						if((1 - pNormMag) < BaseThreshold * 2) {
							position = normalized;
						}
						else if(pNormMag < BaseThreshold * 2) {
							position = Vector2.zero;
						}
						else {
							position = pNorm;
						}
					}
					else {
						position = pNorm;
					}
				}
			}

			Direction direction = GetDirectionFromPosition(ref position);
			

			if (!fromResidual) {
				if (!centered && transform.localPosition.magnitude < BaseThreshold && Vector2.Dot(position, transform.localPosition) > 0) {
					position = transform.localPosition.normalized * Mathf.Min(transform.localPosition.magnitude, BaseThreshold - 0.001f);
				}
				else if (centered && position.magnitude < BaseThreshold) {
					return false;
				}
			}

			Vector3 moveAmount = position - (Vector2)transform.localPosition;
			float magBeforeLimiting = position.magnitude;
			// limit speed to max movement speed
			if (moveAmount.magnitude > deltaTimeSpeedCap) {
				moveAmount = moveAmount.normalized * deltaTimeSpeedCap;
				position = transform.localPosition + moveAmount;
			}

			// make sure we escape base threshold when uncentering
			float mag = position.magnitude;
			if (centered && magBeforeLimiting > BaseThreshold) {
				mag = Mathf.Max(BaseThreshold + 0.001f, position.magnitude);
				position = mag * position.normalized;
			}

			// if we're going up to the border, make sure we don't overshoot
			if (!centered && Space.GetNeighborInDirection(direction) == null && Vector2.Dot(transform.localPosition, position) < 0) {
				mag = 0f;
				position = Vector2.zero;
			}

			if(position.magnitude >= 1) {
				position = position.normalized;
			}

			moveAmount = position - (Vector2)transform.localPosition;
			bool moved = false;
			float scale = transform.lossyScale.x;
			HashSet<Tile> tilesToMove = new HashSet<Tile>();
			bool canMove = CanMoveTo(ref moveAmount, tilesToMove, direction);
			if (canMove) {
				moved = this.Move(moveAmount, direction);
				foreach (Tile t in tilesToMove) {
					var tMoveAmount = moveAmount;
					t.Move(tMoveAmount, direction);
					if(fromResidual) {
						t.ResidualVelocity = this.ResidualVelocity;
					}
				}
			}

			// if movement is off by 90 degrees, it's changed direction from the residual velocity - throw it out
			if(!canMove || Mathf.Abs(Vector2.Dot(direction.Value, ResidualVelocity)) < 0.25f) {
				ResidualVelocity = Vector2.zero;
			}
			return moved;
		}
		return false;
	}

	public Direction GetDirectionFromPosition(ref Vector2 position) {
		float absX = Mathf.Abs(position.x);
		float absY = Mathf.Abs(position.y);
		if (absX > absY) {		
			position *= Vector2.right;
			return Mathf.Sign(position.x) > 0 ? Direction.Right : Direction.Left;
		}
		else {
			position *= Vector2.up;
			return Mathf.Sign(position.y) > 0 ? Direction.Up : Direction.Down;
		}
	}

	public void Reset() {
		transform.parent = initialTilespace.transform;
		this.Space = initialTilespace;
		SetMovable(initialMovable, false);
		transform.localPosition = Vector2.zero;
	}

	public void SetMovable(bool movable, bool animate = true) {
		Movable = movable;
		if(!movable) {
			if (Selected) {
				Select(false);
			}

			StopCoroutine(SetImmobileAnimation());
			if(animate) {
				StartCoroutine(SetImmobileAnimation());
			}
			else {
				SetMobileShaderValue(0f);
			}
		}
		else {
			SetMobileShaderValue(1f);
		}
	}

	private void SetMobileShaderValue(float v, Material m = null) {
		m = m ?? new Material(spriteRenderer.sharedMaterial);
		m.SetFloat("_Mobile", v);
		spriteRenderer.sharedMaterial = m;
	}

	private IEnumerator SetImmobileAnimation() {
		float t = 0;
		float animationTime = 0.25f;
		Material m = new Material(spriteRenderer.sharedMaterial);
		while(t < animationTime) {
			SetMobileShaderValue(1 - t / animationTime, m);
			t += Time.deltaTime;
			yield return null;
		}
		SetMobileShaderValue(0, m);
	}

	public bool IsPlayerOnTile() => Physics2D.OverlapBox(transform.position, transform.lossyScale, 0, playerMask) != null;
}
