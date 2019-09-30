using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tile : MonoBehaviour
{
	public const float BaseThreshold = 0.10f;
	private const float BaseThresholdSquared = BaseThreshold * BaseThreshold;

	private const float SpeedCap = 4f;

	public Tilespace Space { get; set; }

	public bool Movable;
	[SerializeField]
	private Material SelectedMaterial;
	private Material UnselectedMaterial;

	public bool Selected { get; set; }
	public Vector2 PositionWhenSelected { get; set; }

	public bool Centered { get => transform.localPosition.sqrMagnitude < 0.000002f; } // (skinWidth / scale)^2 

	private LayerMask tileMask;
	private SpriteRenderer spriteRenderer;

	private PlatformController[] childPlatforms;

	private Vector3 lastFrameVelocity;
	private Vector3 lastFramePosition;

	private Tilespace initialTilespace;

	private void Start() {
		tileMask = 1 << LayerMask.NameToLayer("Tile");
		spriteRenderer = GetComponent<SpriteRenderer>();
		if(!Movable) {
			spriteRenderer.color = new Color(0.5f, 0.15f, 0f);
		}
		childPlatforms = GetComponentsInChildren<PlatformController>();
		lastFramePosition = transform.position;
		lastFrameVelocity = Vector3.zero;	

		UnselectedMaterial = spriteRenderer.sharedMaterial;
	}

	public void Init(Tilespace t) {
		this.Space = t;
		this.initialTilespace = t;
	}

	public void Update() {
		if (!Centered && transform.localPosition.sqrMagnitude <= BaseThresholdSquared) {
			float distToMove = Time.deltaTime * SpeedCap;
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

	public void Select(bool select) {
		Selected = select;
		if (Selected) {
			PositionWhenSelected = transform.position;
		}
		spriteRenderer.sharedMaterial = Selected ? SelectedMaterial : UnselectedMaterial;
	}

	public void CompleteMove(Tilespace space) {
		this.Space = space;
		space.Tile = this;		
	}

	public bool Move(Vector3 moveAmount, Direction d) {
		Vector2 globalMoveAmount = moveAmount * transform.lossyScale.x;

		foreach (PlatformController c in childPlatforms) {
			c.CheckAndRemoveSquishables(globalMoveAmount);
			c.Premove(ref globalMoveAmount);
		}
		transform.localPosition += moveAmount;
		foreach (PlatformController c in childPlatforms) {
			c.Postmove(ref globalMoveAmount);
		}

		float mag = transform.localPosition.magnitude;
		if(mag > BaseThreshold) {
			if (mag > 1 - BaseThreshold) {
				// snap to next spot
				Tilespace next = Space.GetNeighborInDirection(d);		
				if(Space.Tile == this) {
					Space.Tile = null;
				}
			
				next.SetChildTile(this);
				// Debug.Log(next);
				return true;
			}	
		}
		return false;
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

			if (collisions.Length == 0) {
				return true;
			}
			else if (collisions.Length == 1) {
				Debug.DrawRay(this.transform.position, transform.lossyScale * 0.5f + moveAmount, Color.red);
			
				Tile collidedTile = collisions[0].GetComponent<Tile>();
				bool canMoveInDirection = collidedTile.Centered || Mathf.Abs(Vector2.Dot(moveAmount.normalized, collidedTile.transform.localPosition.normalized)) > 0.1f;
				if (collidedTile.Movable && canMoveInDirection) {		
					// Debug.DrawLine(this.transform.position, hit.transform.position, Color.blue, 0.25f);
					tilesToMove.Add(collidedTile);
					return collidedTile.CanMoveTo(ref moveAmount, tilesToMove, d);
				}
				else {
					
				}
			}
		}
		return false;
	}

	public bool TryMove(Vector2 mouseMoveSinceSelection, Vector2 delta) {
		if(Movable) {
			Vector2 position = mouseMoveSinceSelection + (this.PositionWhenSelected - (Vector2)Space.transform.position) / transform.lossyScale.x;
			bool centered = Centered;

			if(!centered) {
				position *= new Vector2(Mathf.Abs(transform.localPosition.x), Mathf.Abs(transform.localPosition.y)).normalized;
			}

			Direction direction = GetDirectionFromPosition(ref position);
			
			if(!centered && transform.localPosition.magnitude < BaseThreshold) {
				position = transform.localPosition.normalized * Mathf.Min(transform.localPosition.magnitude, BaseThreshold - 0.001f);
			}
			else if(centered && position.magnitude < BaseThreshold) {
				return false;	
			}

			// limit movement
			Vector3 move = (position - (Vector2)transform.localPosition);
			if (centered && position.magnitude > BaseThreshold) {
				position = (BaseThreshold + 0.001f) * position.normalized;
			}
			else if (move.magnitude > SpeedCap * Time.deltaTime) {
				position = transform.localPosition + move.normalized * SpeedCap * Time.deltaTime;
			}

			float mag = position.magnitude;
			if(position.magnitude >= 1) {
				position = position.normalized;
			}

			bool moved = false;
			float scale = transform.lossyScale.x;
			Vector3 moveAmount = position - (Vector2)transform.localPosition;

			HashSet<Tile> tilesToMove = new HashSet<Tile>();
						
			if (CanMoveTo(ref moveAmount, tilesToMove, direction)) {
				moved = this.Move(moveAmount, direction);
				foreach (Tile t in tilesToMove) {
					var tMoveAmount = moveAmount;
					t.Move(tMoveAmount, direction);
				}
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
		transform.localPosition = Vector2.zero;
	}
}
