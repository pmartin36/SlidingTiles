﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller2D : RaycastController {
	private static int canJumpUpIndex = 3;
	public float maxSlopeAngle = 80;

	public CollisionInfo collisions;
	private Vector2 extraMove;

	public float GravityAngle { get; set; }
	public override float Angle => GravityAngle;

	public override void Start() {
		base.Start ();
		collisions.faceDir = 1;
	}

	public void MoveFromPlatform(object sender, Vector3 diff) {
		// Move(diff, true, true);
		extraMove = diff;
	}

	public Vector2 Move(Vector2 moveAmount, bool standingOnPlatform = false) {
		moveAmount += extraMove;
		extraMove = Vector2.zero;
		UpdateRaycastOrigins();

		moveAmount = moveAmount.Rotate(-transform.eulerAngles.z);

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

		// translate is affected by rotation, so don't rotate the moveAmount back to original
		//moveAmount = moveAmount.Rotate(transform.eulerAngles.z);
		transform.Translate (moveAmount);

		if (standingOnPlatform) {
			collisions.below = true;
		}

		return moveAmount;
	}

	public bool CheckForJumpableObjects(Vector2 moveAmount, out float jumpHeight, out float distanceFromObstacle, bool standingOnPlatform = false) {
		UpdateRaycastOrigins();

		jumpHeight = -1f;
		distanceFromObstacle = 1000f;
		if (Mathf.Abs(moveAmount.x) < skinWidth) {
			return false;
		}

		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
		bool hasHitBlockingObject = false;

		Vector2 firstRayLocation = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = firstRayLocation + raycastOrigins.rotatedUp * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, raycastOrigins.rotatedRight * directionX, rayLength, collisionMask);
			Debug.DrawRay(rayOrigin, raycastOrigins.rotatedRight * directionX * rayLength, Color.blue);

			if (hit) {
				if (hit.distance == 0) {
					continue;
				}

				// if there are any hits above the index, jumping isn't possible since it's blocked
				if (i >= canJumpUpIndex) {
					return false;
				}
				else {
					jumpHeight = -1;
				}

				float slopeAngle = Vector2.Angle(hit.normal, raycastOrigins.rotatedUp);
				if (slopeAngle > maxSlopeAngle) {
					distanceFromObstacle = Mathf.Min(distanceFromObstacle, hit.distance - skinWidth);
					hasHitBlockingObject = true;
				}

			}
			else if (hasHitBlockingObject && i <= canJumpUpIndex && jumpHeight < 0) {
				float height = Vector2.Distance(firstRayLocation, rayOrigin) - moveAmount.y;
				if (height > 0) {
					jumpHeight = height;
				}
			}
		}
		return jumpHeight > 0;
	}

	void HorizontalCollisions(ref Vector2 moveAmount) {
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs (moveAmount.x) + skinWidth;

		if (Mathf.Abs(moveAmount.x) < skinWidth) {
			rayLength = 2*skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += raycastOrigins.rotatedUp * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, raycastOrigins.rotatedRight * directionX, rayLength, collisionMask);

			// Debug.DrawRay(rayOrigin, Vector2.right * directionX,Color.red);
			Debug.DrawRay(rayOrigin, raycastOrigins.rotatedRight * directionX * rayLength, Color.red);

			if (hit) {

				if (hit.distance == 0) {
					continue;
				}

				float slopeAngle = Vector2.Angle(hit.normal, raycastOrigins.rotatedUp);

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
			rayOrigin += raycastOrigins.rotatedRight * (verticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, raycastOrigins.rotatedUp * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, raycastOrigins.rotatedUp * directionY,Color.red);

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
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + raycastOrigins.rotatedUp * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, raycastOrigins.rotatedRight * directionX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, raycastOrigins.rotatedUp);
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

		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast (raycastOrigins.bottomLeft, -raycastOrigins.rotatedUp, Mathf.Abs (moveAmount.y) + skinWidth, collisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast (raycastOrigins.bottomRight, -raycastOrigins.rotatedUp, Mathf.Abs (moveAmount.y) + skinWidth, collisionMask);
		if (maxSlopeHitLeft ^ maxSlopeHitRight) {
			SlideDownMaxSlope (maxSlopeHitLeft, ref moveAmount);
			SlideDownMaxSlope (maxSlopeHitRight, ref moveAmount);
		}

		if (!collisions.slidingDownMaxSlope) {
			float directionX = Mathf.Sign (moveAmount.x);
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -raycastOrigins.rotatedUp, Mathf.Infinity, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, raycastOrigins.rotatedUp);
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
			float slopeAngle = Vector2.Angle(hit.normal, raycastOrigins.rotatedUp);
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
