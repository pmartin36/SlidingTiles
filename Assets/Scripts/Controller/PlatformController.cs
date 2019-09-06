using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController, IMoveableCollider {

	public LayerMask passengerMask;
    public Tile Parent { get; private set; }
    private LayerMask moveLayerMask;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform,Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();
	
	public override void Start () {
		base.Start ();
        Parent = transform.parent.GetComponent<Tile>();
        moveLayerMask = passengerMask | 1 << LayerMask.NameToLayer("Wall");
	}

	void Update () {
		
	}

	public void Premove(ref Vector2 velocity) {
		UpdateRaycastOrigins();
		CalculatePassengerMovement(velocity);
		MovePassengers(true);
	}

	public void Postmove(ref Vector2 velocity) {
		MovePassengers(false);
	}

	void MovePassengers(bool beforeMovePlatform) {
		foreach (PassengerMovement passenger in passengerMovement) {
			LayerMask layer = this.gameObject.layer;
			if(passenger.IgnoreSource) {
				this.gameObject.layer = 0;
			}

			if (!passengerDictionary.ContainsKey(passenger.transform)) {
				passengerDictionary.Add(passenger.transform,passenger.transform.GetComponent<Controller2D>());
			}

			if (passenger.moveBeforePlatform == beforeMovePlatform) {
				passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
			}

			this.gameObject.layer = layer;
		}
	}

	public Vector2 CalculateValidMoveAmount(Vector2 original, ref Tile extraTileToMove) {
		if(extraTileToMove != this.Parent) {
			extraTileToMove = this.Parent;
		}
		
        Vector2 largestValidMoveAmount = original;
        Vector2 norm = original.normalized;
		RaycastHit2D[] hits = Physics2D.BoxCastAll(
			transform.position,
			(collider.size * transform.lossyScale) - Vector2.one * 2 * skinWidth,
			transform.eulerAngles.z,
			original.normalized,
			original.magnitude + skinWidth,
			moveLayerMask
		);

		foreach(RaycastHit2D hit in hits) {
            IMoveableCollider pass = hit.collider.GetComponent<IMoveableCollider>();
            if (pass != null && (pass.Parent == null || pass.Parent.Movable)) {
                Vector2 moveAmount = pass.CalculateValidMoveAmount(largestValidMoveAmount - norm * (hit.distance - skinWidth), ref extraTileToMove);
                moveAmount += original.normalized * (hit.distance - skinWidth);
                if (moveAmount.sqrMagnitude < largestValidMoveAmount.sqrMagnitude) {
                    largestValidMoveAmount = moveAmount;
                }
            }
            else {
                if (hit.distance * hit.distance < largestValidMoveAmount.sqrMagnitude) {
                    largestValidMoveAmount = (hit.distance - skinWidth) * original.normalized;
                }
            }
		}

		return largestValidMoveAmount;
	}

	void CalculatePassengerMovement(Vector3 velocity) {
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();
		passengerMovement = new List<PassengerMovement> ();

		float directionX = Mathf.Sign (velocity.x);
		float directionY = Mathf.Sign (velocity.y);

		// Vertically moving platform
		if (velocity.y != 0) {
			float rayLength = Mathf.Abs (velocity.y) + skinWidth;
			
			for (int i = 0; i < verticalRayCount; i ++) {
				Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);
						float pushX = (directionY == 1)?velocity.x:0;
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

						passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY), directionY == 1, false, true));
					}
				}
			}
		}

		// Horizontally moving platform
		if (velocity.x != 0) {
			float rayLength = Mathf.Abs (velocity.x) + skinWidth;
			
			for (int i = 0; i < horizontalRayCount; i ++) {
				Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
						float pushY = -skinWidth;
						
						passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY), false, false, true));
					}
				}
			}
		}

		// Passenger on top of a horizontally or downward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
			float rayLength = skinWidth * 2f;
			
			for (int i = 0; i < verticalRayCount; i ++) {
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
				// Debug.DrawRay(rayOrigin, Vector2.up, Color.blue);
				
				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;
						
						passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY), true, false, false));
					}
				}
			}
		}
	}

	struct PassengerMovement {
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;
		public bool IgnoreSource { get; set; }

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform, bool ignoreSource) {
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
			IgnoreSource = ignoreSource;
		}
	}
}
