using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller2D : RaycastController {

	public float maxSlopeAngle = 80;

	public CollisionInfo collisions;
	private Vector2 extraMove;

	public override void Start() {
		base.Start ();
		collisions.faceDir = 1;

	}

	public void MoveFromPlatform(object sender, Vector3 diff) {
		// Move(diff, true, true);
		extraMove = diff;
	}

	public Vector2 Move(Vector2 moveAmount, bool standingOnPlatform = false, bool fromEvent = false) {
		moveAmount += extraMove;
		extraMove = Vector2.zero;
		UpdateRaycastOrigins ();

		CollisionInfo old = new CollisionInfo(collisions);
		collisions.Reset ();
		collisions.moveAmountOld = moveAmount;

		if (moveAmount.y < 0) {
			DescendSlope(ref moveAmount);
		}

		if (moveAmount.x != 0) {
			collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
		}

		HorizontalCollisions (ref moveAmount);
		if (moveAmount.y != 0) {
			VerticalCollisions (ref moveAmount);
		}

		transform.Translate (moveAmount);

		if (standingOnPlatform) {
			collisions.below = true;
		}

		if(fromEvent) {
			Debug.Log($"Old: {collisions.moveAmountOld}, Actual: {moveAmount}"); 
			collisions = old;
		}

		return moveAmount;
	}

	void HorizontalCollisions(ref Vector2 moveAmount) {
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs (moveAmount.x) + skinWidth;

		if (Mathf.Abs(moveAmount.x) < skinWidth) {
			rayLength = 2*skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX,Color.red);

			if (hit) {

				if (hit.distance == 0) {
					continue;
				}

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxSlopeAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						moveAmount = collisions.moveAmountOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance-skinWidth;
						moveAmount.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
					moveAmount.x += distanceToSlopeStart * directionX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle) {
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
					}

					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollisions(ref Vector2 moveAmount, bool fromEvent = false) {
		float directionY = Mathf.Sign (moveAmount.y);
		float rayLength = Mathf.Abs (moveAmount.y) + skinWidth;
		// float castLength = 1f;

		for (int i = 0; i < verticalRayCount; i ++) {

			Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY,Color.red);

			if (hit) {
				if (hit.collider.tag == "Through") {
					if (directionY == 1 || hit.distance == 0) {
						continue;
					}
					if (collisions.fallingThroughPlatform) {
						continue;
					}
				}

				moveAmount.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
				}

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;

				collisions.collisionsBelow.Add(hit.collider.gameObject);
			}
		}

		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign(moveAmount.x);
			rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionX,rayLength,collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
					collisions.slopeNormal = hit.normal;
				}
			}
		}

		if(!fromEvent) {
			List<Platform> toRemove = new List<Platform>();
			foreach (Platform p in collisions.collisionsBelowOld) {
				bool contains = collisions.collisionsBelow.Contains(p.gameObject);
				if(contains) {
					collisions.collisionsBelow.Remove(p.gameObject);
				}
				else {
					toRemove.Add(p);
				}
			}
			foreach(Platform p in toRemove) {
				p.Moved -= MoveFromPlatform;
				collisions.collisionsBelowOld.Remove(p);
			}
			foreach(GameObject c in collisions.collisionsBelow) {
				Platform p = c.GetComponent<Platform>();
				if(p != null) {
					p.Moved += MoveFromPlatform;
					collisions.collisionsBelowOld.Add(p);
				}
			}
		}
	}

	void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal) {
		float moveDistance = Mathf.Abs (moveAmount.x);
		float climbmoveAmountY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (moveAmount.y <= climbmoveAmountY) {
			moveAmount.y = climbmoveAmountY;
			moveAmount.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (moveAmount.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
			collisions.slopeNormal = slopeNormal;
		}
	}

	void DescendSlope(ref Vector2 moveAmount) {

		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast (raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs (moveAmount.y) + skinWidth, collisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast (raycastOrigins.bottomRight, Vector2.down, Mathf.Abs (moveAmount.y) + skinWidth, collisionMask);
		if (maxSlopeHitLeft ^ maxSlopeHitRight) {
			SlideDownMaxSlope (maxSlopeHitLeft, ref moveAmount);
			SlideDownMaxSlope (maxSlopeHitRight, ref moveAmount);
		}

		if (!collisions.slidingDownMaxSlope) {
			float directionX = Mathf.Sign (moveAmount.x);
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
					if (Mathf.Sign (hit.normal.x) == directionX) {
						if (hit.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (moveAmount.x)) {
							float moveDistance = Mathf.Abs (moveAmount.x);
							float descendmoveAmountY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
							moveAmount.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (moveAmount.x);
							moveAmount.y -= descendmoveAmountY;

							collisions.slopeAngle = slopeAngle;
							collisions.descendingSlope = true;
							collisions.below = true;
							collisions.slopeNormal = hit.normal;
						}
					}
				}
			}
		}
	}

	void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount) {

		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle > maxSlopeAngle) {
				moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs (moveAmount.y) - hit.distance) / Mathf.Tan (slopeAngle * Mathf.Deg2Rad);

				collisions.slopeAngle = slopeAngle;
				collisions.slidingDownMaxSlope = true;
				collisions.slopeNormal = hit.normal;
			}
		}

	}

	void ResetFallingThroughPlatform() {
		collisions.fallingThroughPlatform = false;
	}

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public bool descendingSlope;
		public bool slidingDownMaxSlope;

		public float slopeAngle, slopeAngleOld;
		public Vector2 slopeNormal;
		public Vector2 moveAmountOld;
		public int faceDir;
		public bool fallingThroughPlatform;

		public HashSet<GameObject> collisionsBelow;
		public HashSet<Platform> collisionsBelowOld;

		public CollisionInfo(CollisionInfo info) {
			above = info.above;
			below = info.below;
			left = info.left;
			right = info.right;
			climbingSlope = info.climbingSlope;
			descendingSlope = info.descendingSlope;
			slidingDownMaxSlope = info.slidingDownMaxSlope;
			slopeAngle = info.slopeAngle;
			slopeAngleOld = info.slopeAngleOld;
			slopeNormal = info.slopeNormal;
			moveAmountOld = info.moveAmountOld;
			faceDir = info.faceDir;
			fallingThroughPlatform = info.fallingThroughPlatform;

			collisionsBelow = info.collisionsBelow != null ? new HashSet<GameObject>(info.collisionsBelow) : new HashSet<GameObject>();
			collisionsBelowOld = info.collisionsBelowOld != null ? new HashSet<Platform>(info.collisionsBelowOld) : new HashSet<Platform>();
		}

		public void Reset() {
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			slidingDownMaxSlope = false;
			slopeNormal = Vector2.zero;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;

			if(collisionsBelow != null) {
				collisionsBelow.Clear();
			}
			else {
				collisionsBelow = new HashSet<GameObject>();
				collisionsBelowOld = new HashSet<Platform>();
			}
		}
	}

}
