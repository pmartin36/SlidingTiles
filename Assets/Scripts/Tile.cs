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

	public bool Selected { get; set; }
	public Vector2 PositionWhenSelected { get; set; }

	public bool Centered { get => transform.localPosition.sqrMagnitude < 0.0001f; }

	private LayerMask tileMask;
	private SpriteRenderer spriteRenderer;

	private PlatformController[] childPlatforms;

	private void Start() {
		tileMask = 1 << LayerMask.NameToLayer("Tile");
		spriteRenderer = GetComponent<SpriteRenderer>();
		if(!Movable) {
			spriteRenderer.color = new Color(0.15f, 0, 0);
		}
		childPlatforms = GetComponentsInChildren<PlatformController>();
	}

	public void Update() {
		if (!Centered && transform.localPosition.sqrMagnitude <= BaseThresholdSquared) {
			float distToMove = Time.deltaTime * SpeedCap;
			Vector2 position;
			if (distToMove > transform.localPosition.magnitude) {
				position = Vector2.zero;
			}
			else {
				position = transform.localPosition - transform.localPosition.normalized * distToMove;
			}

			var noDirection = Direction.None;
			List<Tile> tilesToMove = new List<Tile>() { this };
			if (CanMoveTo(position, tilesToMove, noDirection)) {
				foreach (Tile t in tilesToMove) {
					t.Move(position, noDirection);
				}
			}
		}
	}

	public void Select(bool select) {
		Selected = select;
		if (Selected) {
			PositionWhenSelected = transform.position;
		}

		if(Movable) {
			spriteRenderer.color = Selected ? new Color(0.5f, 0, 0) : new Color(0.3f, 0, 0);
		}
	}

	public void CompleteMove(Tilespace space) {
		this.Space = space;
		space.Tile = this;		
	}

	public bool Move(Vector2 position, Direction d) {
		Vector2 diff = (position - (Vector2)transform.localPosition) * transform.lossyScale.x;

		foreach(PlatformController c in childPlatforms) {
			c.Premove(ref diff);
		}
		transform.localPosition = position;
		foreach (PlatformController c in childPlatforms) {
			c.Postmove(ref diff);
		}

		float mag = position.magnitude;
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

	public bool CanMoveTo(Vector3 localPosition, List<Tile> tilesToMove, Direction d) {
		bool goingTowardCenter = localPosition.sqrMagnitude < transform.localPosition.sqrMagnitude;// && Vector2.Dot(localPosition, transform.localPosition) < 0;
		bool validTilespace = d.Value.sqrMagnitude < 0.1f || Space.GetNeighborInDirection(d) != null;
		if(validTilespace || goingTowardCenter) {
			Vector3 dir = localPosition - transform.localPosition;
			Vector2 size = Mathf.Abs(dir.x) > 0.0001f 
				? new Vector2(dir.magnitude * 0.5f, transform.lossyScale.x * 0.90f)
				: new Vector2(transform.lossyScale.x * 0.90f, dir.magnitude * 0.5f);
			Collider2D[] collisions = 
				Physics2D.OverlapBoxAll(
					this.transform.position + dir.normalized * (transform.lossyScale.x + dir.magnitude + 0.001f) * 0.5f,
					size, 
					0,
					tileMask)
					.Where(r => r.gameObject != this.gameObject).ToArray();

			if(collisions.Length == 0) {
				return true;
			}
			else if (collisions.Length == 1) {
				Debug.DrawRay(this.transform.position, transform.lossyScale * 0.5f + dir, Color.red);
			
				Tile collidedTile = collisions[0].GetComponent<Tile>();
				bool canMoveInDirection = collidedTile.Centered || Mathf.Abs(Vector2.Dot(dir.normalized, collidedTile.transform.localPosition.normalized)) > 0.1f;
				if (collidedTile.Movable && canMoveInDirection) {		
					// Debug.DrawLine(this.transform.position, hit.transform.position, Color.blue, 0.25f);
					tilesToMove.Add(collidedTile);
					return collidedTile.CanMoveTo(localPosition, tilesToMove, d);
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
			List<Tile> tilesToMove = new List<Tile>();
			if (CanMoveTo(position, tilesToMove, direction)) {
				moved = this.Move(position, direction);
				foreach (Tile t in tilesToMove) {
					t.Move(position, direction);
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
}
