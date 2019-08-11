using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
	public const float Threshold = 0.05f;
	public Tilespace Space { get; set; }

	public bool Movable;
	public Direction MoveDirection { get; set; } = Direction.None;

	public Vector2 GrabPositionOffset { get; set; }

    void Update() {
        
    }

	public void CompleteMove(Tilespace space) {
		this.Space = space;
		space.Tile = this;		
	}

	public void MoveEvent(object sender, Vector2 distMoved) {
		Move(distMoved);
	}

	public bool Move(Vector2 distMoved) {
		distMoved *= MoveDirection.Value;
		if(distMoved.magnitude > 1 - Threshold) {
			// snap to next spot
			Tilespace next = Space.GetNeighborInDirection(MoveDirection);		
			if(Space.Tile == this) {
				Space.Tile = null;
			}
			this.Space = next;
			next.Tile = this;
			return true;
		}
		transform.localPosition = distMoved;
		return false;
	}

	public bool TryMove(Vector2 distMoved) {
		if(Movable) {
			if(MoveDirection) {
				var distMovedInDirection = distMoved * MoveDirection.Value;
				if(distMovedInDirection.magnitude > Threshold) {
					return Move(distMoved);
				}
				else {
					// move towards tile
					float distToMove = Time.deltaTime;
					if(distToMove > transform.localPosition.magnitude) {
						transform.localPosition = Vector2.zero;
						MoveDirection = Direction.None;
					}
					else {
						transform.localPosition -= transform.localPosition.normalized * distToMove;
					}
				}	
			}

			float absX = Mathf.Abs(distMoved.x);
			float absY = Mathf.Abs(distMoved.y);
			if(absX < Threshold && absY < Threshold) {
				return false;
			}

			if (absX > absY) {
				// once moving, tiles will get updated by events
				// once moving, tiles will be updated
				Direction d = Mathf.Sign(distMoved.x) > 0 ? Direction.Right : Direction.Left;
				if(Space.CanMoveTo(d, false)) {
					MoveDirection = d;
				}
				else {
					MoveDirection = Direction.None;
					// can't move, clear list of tiles waiting for move updates
					LevelManager.ClearMovingTiles();
				}				
			}
			else {
				// once moving, tiles will be updated
				Direction d = Mathf.Sign(distMoved.y) > 0 ? Direction.Up : Direction.Down;
				if (Space.CanMoveTo(d, false)) {
					MoveDirection = d;
				}
				else {
					MoveDirection = Direction.None;
					// can't move, clear list of tiles waiting for move updates
					LevelManager.ClearMovingTiles();
				}				
			}
		}

		return false;
	}
}
