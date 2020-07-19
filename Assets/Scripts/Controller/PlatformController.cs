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
        Parent = transform.parent.parent.GetComponent<Tile>();

		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		// sr.material.SetFloat("_Rotation", transform.eulerAngles.z * Mathf.Deg2Rad);
		size = (sr.size * transform.lossyScale);
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
				//Debug.Log($"Moving player from platform: {vel.y}");
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

		DebugExtensions.DrawBoxCast2D(
			transform.position,
			(size - Vector2.one * 2 * skinWidth),
			transform.eulerAngles.z,
			original.normalized,
			original.magnitude + skinWidth,
			Color.cyan
		);

		foreach (RaycastHit2D hit in hits) {
            IPlatformMoveBlocker pass = hit.collider.GetComponent<IPlatformMoveBlocker>();
            if (pass != null) {
				Vector2 passMoveAmount = original - norm * (hit.distance - skinWidth);
				pass.CheckBlocking(ref passMoveAmount, tilesToMove);
				original = passMoveAmount + norm * (hit.distance - skinWidth);
			}
		}
	}

	public RaycastHit2D GetCurrentBlocker() {
		return Physics2D.BoxCast(
			transform.position,
			size - Vector2.one * 2 * skinWidth,
			transform.eulerAngles.z,
			Vector2.zero,
			0,
			passengerMask);
	}

	public void MovePlatformBlockersFromRotation(float diff, Vector2 center) {
		Dictionary<IPlatformMoveBlocker, Vector3> moves = new Dictionary<IPlatformMoveBlocker, Vector3>();
		foreach (var pt in GeneratePoints().All) {
			var end = pt;
			var start = end.RotateAround(-diff, center);
			RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, passengerMask);
			foreach(var h in hits) {
				var blocker = h.collider.GetComponent<IPlatformMoveBlocker>();
				var amt = end - h.point;
				if(moves.ContainsKey(blocker)) {
					var move = moves[blocker];
					if(amt.sqrMagnitude > move.sqrMagnitude) {
						move = amt;
					}
				}
				else {
					moves.Add(blocker, amt);
				}
			}
		}

		foreach(var m in moves) {
			m.Key.MoveFromRotation(m.Value);
		}
	}

	public EdgePoints GeneratePoints() {
		var pts = new EdgePoints(transform.eulerAngles.z);
		Bounds bounds = new Bounds(transform.position, size);
		CalculateRaySpacing(bounds, 0.75f, 1f);

		// left
		// right
		float y = transform.position.y - (size.y / 2f);
		float halfx = size.x / 2f;
		for(int i = 0; i < horizontalRayCount; i++) {
			pts.Left.Add(new Vector2(transform.position.x - halfx, y).RotateAround(transform.eulerAngles.z, transform.position));
			pts.Right.Add(new Vector2(transform.position.x + halfx, y).RotateAround(transform.eulerAngles.z, transform.position));
			y += horizontalRaySpacing;
		}

		// top
		// bottom
		float x = transform.position.x - (size.x / 2f);
		float halfy = size.y / 2f;
		for(int i = 0; i < verticalRayCount; i++) {
			pts.Bottom.Add(new Vector2(x, transform.position.y - halfy).RotateAround(transform.eulerAngles.z, transform.position));
			pts.Top.Add(new Vector2(x, transform.position.y + halfy).RotateAround(transform.eulerAngles.z, transform.position));
			x += verticalRaySpacing;
		}

		return pts;
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
				rayOrigin += raycastOrigins.rotatedRight * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, raycastOrigins.rotatedUp * directionY, rayLength, passengerMask);

				if (hit) {
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
				rayOrigin += raycastOrigins.rotatedUp * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, raycastOrigins.rotatedRight * directionX, rayLength, passengerMask);

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
				Vector2 rayOrigin = raycastOrigins.topLeft + raycastOrigins.rotatedRight * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, raycastOrigins.rotatedUp, rayLength, passengerMask);
				Debug.DrawRay(rayOrigin, raycastOrigins.rotatedUp * rayLength, Color.black, 0.5f);

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

	public override float Angle => 0f;
	public override Bounds GetBounds() {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);
		return bounds;
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

public struct EdgePoints {
	public List<Vector2> Left, Right, Top, Bottom;

	public List<Vector2> All {
		get {
			var list = new List<Vector2>(Left);
			list.AddRange(Right);
			list.AddRange(Top);
			list.AddRange(Bottom);
			return list;
		}
	}


	public EdgePoints(float angle = 0) {
		Left = new List<Vector2>();
		Right = new List<Vector2>();
		Top = new List<Vector2>();
		Bottom = new List<Vector2>();
	}
}
