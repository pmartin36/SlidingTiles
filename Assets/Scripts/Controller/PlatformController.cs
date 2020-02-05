using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController, IMoveableCollider {

	public LayerMask passengerMask;
    public Tile Parent { get; private set; }

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform,Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	private Vector2 size;
	
	public override void Start () {
		base.Start ();
        Parent = transform.parent.GetComponent<Tile>();

		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		// sr.material.SetFloat("_Rotation", transform.eulerAngles.z * Mathf.Deg2Rad);
		size = (sr.size * transform.lossyScale).Rotate(transform.eulerAngles.z);
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
				Vector3 vel = passenger.velocity;
				if (Mathf.Abs(passenger.velocity.x) > 0.001f) {
					var player = passenger.transform.gameObject.GetComponent<Player>();
					Vector3 v = passenger.velocity / Time.deltaTime;
					player.DetermineJump(passenger.velocity, out var modifiedVelocity);
					if(modifiedVelocity.Item1) {
						vel = modifiedVelocity.Item2;
					}
				}
				passengerDictionary[passenger.transform].Move(vel, passenger.standingOnPlatform);
			}

			this.gameObject.layer = layer;
		}
	}

	public void CheckBlocking(ref Vector2 original, HashSet<Tile> tilesToMove) {
        Vector2 largestValidMoveAmount = original;
        Vector2 norm = original.normalized;
		RaycastHit2D[] hits = Physics2D.BoxCastAll(
			transform.position,
			size - Vector2.one * 2 * skinWidth,
			transform.eulerAngles.z,
			original.normalized,
			original.magnitude + skinWidth,
			passengerMask
		);

		foreach(RaycastHit2D hit in hits) {
            IPlatformMoveBlocker pass = hit.collider.GetComponent<IPlatformMoveBlocker>();
            if (pass != null) {
				Vector2 passMoveAmount = original - norm * (hit.distance - skinWidth);
				pass.CheckBlocking(ref passMoveAmount, tilesToMove);
				original = passMoveAmount + norm * (hit.distance - skinWidth);
			}
		}
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
