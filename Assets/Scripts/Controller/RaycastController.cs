using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public LayerMask collisionMask;
	
	public const float skinWidth = .03f;
	const float dstBetweenRays = .25f;
	[HideInInspector]
	public int horizontalRayCount;
	[HideInInspector]
	public int verticalRayCount;

	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	[HideInInspector]
	public BoxCollider2D collider;
	public RaycastOrigins raycastOrigins;

	public LayerMask platformMask;

	public virtual void Awake() {
		collider = GetComponent<BoxCollider2D> ();
		platformMask = 1 << LayerMask.NameToLayer("Level");
	}

	public virtual void Start() {
		
	}

	public void UpdateRaycastOrigins() {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		bounds = new Bounds(
			(Vector2)transform.position + collider.offset * transform.lossyScale.Abs(), 
			collider.size * transform.lossyScale.Abs() + Vector2.one * skinWidth * -2f);

		CalculateRaySpacing(bounds);

		raycastOrigins.rotatedRight = Vector2.right.Rotate(transform.eulerAngles.z);
		raycastOrigins.rotatedUp = Vector2.up.Rotate(transform.eulerAngles.z);

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y).RotateAround(transform.eulerAngles.z, transform.position);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y).RotateAround(transform.eulerAngles.z, transform.position);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y).RotateAround(transform.eulerAngles.z, transform.position);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y).RotateAround(transform.eulerAngles.z, transform.position);
	}
	
	public void CalculateRaySpacing(Bounds bounds) {
		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;
		
		horizontalRayCount = Mathf.RoundToInt (boundsHeight / dstBetweenRays / 2); // we don't need like 20 rays
		verticalRayCount = Mathf.RoundToInt (boundsWidth  / dstBetweenRays);
		
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}
	
	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
		public Vector2 rotatedRight, rotatedUp;
	}
}
