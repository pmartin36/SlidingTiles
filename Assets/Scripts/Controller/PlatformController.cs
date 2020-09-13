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
				Vector3 vel = passenger.velocity.Rotate(passenger.transform.eulerAngles.z);
				var player = passenger.transform.gameObject.GetComponent<Player>();
				if (Mathf.Abs(passenger.velocity.x) > 0.001f && player != null) {
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
		var pts = new EdgePoints(0);
		Bounds bounds = new Bounds(transform.position, size);
		CalculateRaySpacing(bounds, 0.75f, 1f);

		// left
		// right
		float y = transform.position.y - (size.y / 2f);
		float halfx = (size.x / 2f) - skinWidth;
		for(int i = 0; i < horizontalRayCount; i++) {
			pts.Left.Add(new Vector2(transform.position.x - halfx, y).RotateAround(transform.eulerAngles.z, transform.position));
			pts.Right.Add(new Vector2(transform.position.x + halfx, y).RotateAround(transform.eulerAngles.z, transform.position));
			y += horizontalRaySpacing;
		}

		// top
		// bottom
		float x = transform.position.x - (size.x / 2f);
		float halfy = (size.y / 2f) - skinWidth;
		for(int i = 0; i < verticalRayCount; i++) {
			pts.Bottom.Add(new Vector2(x, transform.position.y - halfy).RotateAround(transform.eulerAngles.z, transform.position));
			pts.Top.Add(new Vector2(x, transform.position.y + halfy).RotateAround(transform.eulerAngles.z, transform.position));
			x += verticalRaySpacing;
		}

		return pts;
	}

	void CalculatePassengerMovement(Vector3 velocity) {
		Dictionary<Transform, IPlatformMoveBlocker> movedPassengers = new Dictionary<Transform, IPlatformMoveBlocker> ();
		passengerMovement = new List<PassengerMovement> ();

		System.Func<Vector2, RaycastHit2D, PassengerMovement> GetMovement = (dir, hit) => {
			IPlatformMoveBlocker blocker = movedPassengers[hit.transform];
			
			// add 90 to convert from Vector2.down being default direction to Vector2.right
			Vector2 gravityAngle = Utils.AngleToVector(blocker.GravityAngle - 90);

			Vector2 normalizedVelocity = velocity.normalized;
			var rotatedVelocity = velocity.Rotate(-blocker.GravityAngle);
			float dot = Vector2.Dot(gravityAngle, normalizedVelocity);
			bool movingWithDirection = rotatedVelocity.y > 0;

			// we need to check the direction cast in comparison to gravity to determine
			// players position relative to the platform (i.e. player on top)
			float positionDot = Vector2.Dot(gravityAngle, dir);
			bool onTop = positionDot < -0.9f;

			float velocityDot = Vector2.Dot(normalizedVelocity, dir);

			if (dot > 0.5f) {
				// gravity and platform moving in same direction
				if (onTop) {
					// passenger is on top of the platform and should be moved with it
					float pushX = velocity.x;
					float pushY = velocity.y;
					return new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false, false);
				}
				else {
					// passenger is below the platform and will be pushed
					float pushX = 0f;
					float pushY = rotatedVelocity.y - (hit.distance - skinWidth) * Mathf.Sign(rotatedVelocity.y);
					return new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true, true);
				}
			}
			else if (dot < -0.5f) {
				// gravity and platform moving in opposite directions
				float pushX = movingWithDirection ? rotatedVelocity.x : 0;
				float pushY = rotatedVelocity.y - (hit.distance - skinWidth) * Mathf.Sign(rotatedVelocity.y);
				return new PassengerMovement(hit.transform, new Vector3(pushX, pushY), onTop, true, false);
			}
			else if (positionDot < 0.9f) {
				// platform is moving side-to-side relative to gravity
				// only disallow movement is player is below platform
				float pushX = rotatedVelocity.x - (hit.distance - skinWidth) * Mathf.Sign(rotatedVelocity.x);
				float pushY = 0f;
				return new PassengerMovement(hit.transform, new Vector3(pushX, pushY), onTop, false, false);
			}
			return null;
		};

		System.Action<Vector2, Vector2, float> CastForPoint = (pt, dir, dist) => {
			//Debug.DrawRay(pt, dir*dist, new Color(dir.y*0.5f+0.5f, dir.x*0.5f+0.5f, 0), 0.1f);
			RaycastHit2D hit = Physics2D.Raycast(pt, dir, dist, passengerMask);
			if (hit) {
				if (!movedPassengers.ContainsKey(hit.transform)) {
					movedPassengers.Add(hit.transform, hit.transform.GetComponent<IPlatformMoveBlocker>());
					PassengerMovement m = GetMovement(dir, hit);
					if(m != null) {
						passengerMovement.Add(m);
					}
				}
				else {
					PassengerMovement m = GetMovement(dir, hit);
					int index = passengerMovement.FindIndex(pm => pm.transform == hit.transform);
					if (m != null && index >= 0 && m.velocity.sqrMagnitude > passengerMovement[index].velocity.sqrMagnitude) {
						passengerMovement[index] = m;
					}
				}
			}
		};

		var pts = GeneratePoints();

		float topDistance = skinWidth * 2f;
		float bottomDistance = skinWidth * 2f;
		Vector2 rotatedPlatformVelocity = velocity.Rotate(-transform.eulerAngles.z);
		if(rotatedPlatformVelocity.y > 0) {
			topDistance = Mathf.Max(topDistance, Mathf.Abs(rotatedPlatformVelocity.y) + skinWidth);
		}
		else {
			bottomDistance = Mathf.Max(bottomDistance, Mathf.Abs(rotatedPlatformVelocity.y) + skinWidth);
		} 

		Vector2 up = Vector2.up.Rotate(transform.eulerAngles.z);
		Vector2 right = Vector2.right.Rotate(transform.eulerAngles.z);

		foreach(var pt in pts.Top) {
			CastForPoint(pt, up, topDistance);
		}
		foreach(var pt in pts.Bottom) {
			CastForPoint(pt, -up, bottomDistance);
		}

		float leftDistance = skinWidth * 2f;
		float rightDistance = skinWidth * 2f;
		if (rotatedPlatformVelocity.x > 0) {
			rightDistance = Mathf.Max(rightDistance, Mathf.Abs(rotatedPlatformVelocity.x) + skinWidth);
		}
		else {
			leftDistance = Mathf.Max(leftDistance, Mathf.Abs(rotatedPlatformVelocity.x) + skinWidth);
		}

		foreach (var pt in pts.Right) {
			CastForPoint(pt, right, rightDistance);
		}
		foreach (var pt in pts.Left) {
			CastForPoint(pt, -right, leftDistance);
		}
	}

	public override float Angle => 0f;
	public override Bounds GetBounds() {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);
		return bounds;
	}

	class PassengerMovement {
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
