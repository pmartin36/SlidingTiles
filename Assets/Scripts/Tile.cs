using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tile : MonoBehaviour
{
	public const float BaseThreshold = 0.1f;
	private const float BaseThresholdSquared = 0.01f;

	private float thresholdSquared;
	private float _threshold;
	public float Threshold {
		get => _threshold;
		set {
			_threshold = value;
			thresholdSquared = value * value;
		}
	}

	private const float SpeedCap = 4f;

	public Tilespace Space { get; set; }

	public bool Movable;
	public bool Moved { get; set; }

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
		Threshold = BaseThreshold;
	}

	public void Update() {
		if (!Centered && transform.localPosition.sqrMagnitude < thresholdSquared) {
			float distToMove = Time.deltaTime * SpeedCap;
			if (distToMove > transform.localPosition.magnitude) {
				transform.localPosition = Vector2.zero;
			}
			else {
				transform.localPosition -= transform.localPosition.normalized * distToMove;
			}
		}
	}

	public void Select(bool select) {
		Selected = select;
		if (Selected) {
			PositionWhenSelected = transform.localPosition;
			if(PositionWhenSelected.magnitude > BaseThreshold * 2) {
				Moved = true;
				Threshold = BaseThreshold * 2f;
			}
		}		
		else {
			Threshold = BaseThreshold;
			Moved = false;
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
		Vector2 diff = (position - (Vector2)transform.localPosition) * transform.lossyScale;

		foreach(PlatformController c in childPlatforms) {
			c.Premove(ref diff);
		}
		transform.localPosition = position;
		foreach (PlatformController c in childPlatforms) {
			c.Postmove(ref diff);
		}

		float mag = position.magnitude;
		if(mag > Threshold) {
			if (!Moved && mag > Threshold * 2f) {
				Threshold = Threshold * 2f;
				Moved = true;
			}
			if (mag > 1 - Threshold) {
				// snap to next spot
				Tilespace next = Space.GetNeighborInDirection(d);		
				if(Space.Tile == this) {
					Space.Tile = null;
				}
			
				next.SetChildTile(this);
				Debug.Log(next);
				return true;
			}	
		}
		return false;
	}

	public bool CanMoveTo(Vector3 localPosition, List<Tile> tilesToMove, Direction d) {
		bool goingTowardCenter = Vector2.Dot(localPosition, transform.localPosition) < 0;
		Tilespace t = Space.GetNeighborInDirection(d);
		if(t != null || goingTowardCenter) {
			Vector3 dir = localPosition - transform.localPosition;
			Vector2 size = Mathf.Abs(localPosition.x) > 0.0001f 
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
				if (!collidedTile.Movable 
					|| (!collidedTile.Centered && Mathf.Abs(Vector2.Dot(d.Value, collidedTile.transform.localPosition.normalized)) < 0.1f)) {
					return false;
				}
				else {		
					// Debug.DrawLine(this.transform.position, hit.transform.position, Color.blue, 0.25f);
					tilesToMove.Add(collidedTile);
					return collidedTile.CanMoveTo(localPosition, tilesToMove, d);
				}
			}
		}
		return false;
	}

	public bool TryMove(Vector2 mouseMoveSinceSelection, Vector2 delta) {
		if(Movable) {
			Vector2 position = mouseMoveSinceSelection + this.PositionWhenSelected;
			bool centered = Centered;

			if(!centered) {
				position *= new Vector2(Mathf.Abs(transform.localPosition.x), Mathf.Abs(transform.localPosition.y)).normalized;
			}

			float absX = Mathf.Abs(position.x);
			float absY = Mathf.Abs(position.y);
			Direction direction = Direction.None;
			if (absX > absY) {
				direction = Mathf.Sign(position.x) > 0 ? Direction.Right : Direction.Left;
				position *= Vector2.right;
			}
			else {
				direction = Mathf.Sign(position.y) > 0 ? Direction.Up : Direction.Down;
				position *= Vector2.up;
			}
			
			if(centered && position.magnitude < Threshold) {
				return false;
			}

			// limit movement
			Vector3 move = (position - (Vector2)transform.localPosition);
			if (centered && position.magnitude > Threshold) {
				position = (Threshold + 0.001f) * position.normalized;
			}
			else if (move.magnitude > SpeedCap * Time.deltaTime) {
				position = transform.localPosition + move.normalized * SpeedCap * Time.deltaTime;
			}

			//if (move.magnitude > SpeedCap * Time.deltaTime) {
			//	position = transform.localPosition + move * SpeedCap * Time.deltaTime;
			//}

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
			if(moved) {
				Debug.Log("----------------");
			}
			return moved;
		}

		return false;
	}
}
