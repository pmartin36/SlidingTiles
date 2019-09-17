using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityChanger : MonoBehaviour
{
	private bool Up => transform.localScale.y > 0;
	public LayerMask collisionMask;

	private MeshFilter meshMask;
	private PolygonCollider2D poly;

	private Dictionary<Transform, Vector3> stoppers = new Dictionary<Transform, Vector3>();
	private bool shouldRecalculateMesh;

	void Start() {
		meshMask = GetComponent<MeshFilter>();
		poly = GetComponent<PolygonCollider2D>();
		stoppers = new Dictionary<Transform, Vector3>();
		RecalculateMesh();

		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.sortingLayerName = "Front";
		mr.sortingOrder = 2;
    }

    void Update() {
		foreach(var d in stoppers) {
			if((d.Key.transform.position - d.Value).sqrMagnitude > 0.1f) {
				shouldRecalculateMesh = true;
			}
		}

		if(shouldRecalculateMesh) {
			RecalculateMesh();
		}
		shouldRecalculateMesh = false;
	}

	public void OnTriggerEnter2D(Collider2D collision) {
		IGravityChangable c = collision.GetComponent<IGravityChangable>();
		if(c != null) {
			c.ChangeGravity(Up ? 1f : -1f);
		}
		else if((1 << collision.gameObject.layer & collisionMask.value) > 0) {
			if(!stoppers.ContainsKey(collision.transform)) {
				stoppers.Add(collision.transform, collision.transform.position);
				shouldRecalculateMesh = true;
			}
		}
	}

	public void RecalculateMesh() {
		Vector2 castDirection = Up ? Vector2.up : Vector2.down;
		float prevDist = 0;

		List<MeshPoint> points = new List<MeshPoint>();
		HashSet<Transform> transformsHit = new HashSet<Transform>();
		float end = transform.lossyScale.x / 2f + 0.1f;
		for (float i = -transform.lossyScale.x / 2f; i < end; i += 1f) {
			Vector2 start = (Vector2)transform.position + Vector2.right * i;
			RaycastHit2D hit = Physics2D.Raycast(start, castDirection, 50, collisionMask);
			transformsHit.Add(hit.transform);
			if (points.Count > 0) {
				if (prevDist - hit.distance > 0.1f) {
					Vector2 rightCastStart = start + new Vector2(-1f, castDirection.y * (hit.distance + 0.1f));
					RaycastHit2D rightCastHit = Physics2D.Raycast(rightCastStart, Vector2.right, 1f, collisionMask);
					points.Add(new MeshPoint(new Vector2(rightCastHit.point.x, transform.position.y)));
					points.Add(new MeshPoint(new Vector2(rightCastHit.point.x, transform.position.y + castDirection.y * prevDist), true));
					points.Add(new MeshPoint(new Vector2(rightCastHit.point.x, transform.position.y + castDirection.y * hit.distance)));
				}
				else if (hit.distance - prevDist > 0.1f) {
					Vector2 leftCastStart = start + new Vector2(0, castDirection.y * (prevDist + 0.1f));
					RaycastHit2D leftCastHit = Physics2D.Raycast(leftCastStart, Vector2.left, 1f, collisionMask);
					points.Add(new MeshPoint(new Vector2(leftCastHit.point.x, transform.position.y)));
					points.Add(new MeshPoint(new Vector2(leftCastHit.point.x, transform.position.y + castDirection.y * prevDist), true));
					points.Add(new MeshPoint(new Vector2(leftCastHit.point.x, transform.position.y + castDirection.y * hit.distance)));
				}
			}
			points.Add(new MeshPoint(start));
			points.Add(new MeshPoint(hit.point));
			prevDist = hit.distance;
		}

		
		foreach(Transform t in transformsHit) {
			// remove or update stoppers
			if (stoppers.ContainsKey(t)) {
				stoppers[t] = t.transform.position;
			}
			else {
				stoppers.Remove(t);
			}
		}
		
		Vector3[] vertices = new Vector3[points.Count];
		List<int> tris = new List<int>();
		System.Func<MeshPoint, Vector2> transformPoint = (MeshPoint p) => (p.v - (Vector2)transform.position) / transform.lossyScale;

		for(int i = 0; i < points.Count; i++) {
			vertices[i] = transformPoint(points[i]);
		}

		int pDiff = 0;
		int j = 1;
		Vector2[] pcPoints = new Vector2[points.Count];
		while (j < vertices.Length) {
			int i = j / 2;
			pcPoints[i] = vertices[j];

			int bIndex = vertices.Length - Mathf.CeilToInt(j/2f) - pDiff;
			pcPoints[bIndex] = vertices[j - 1];
			if(points[j].CoupledPoint) {
				pDiff++;
				j++;
				pcPoints[i+1] = vertices[i+1];				
			}
			j += 2;
		}
		poly.points = pcPoints;

		for (int i = 0; i < points.Count - 2; i += 2) {
			int bl = i;
			if (points[i + 1].CoupledPoint)
				i++;
			int tl = i + 1;
			int br = i + 2;
			int tr = i + 3;

			tris.AddRange(new int[] {
				bl, tr, tl,
				bl, br, tr
			});
		}

		Mesh m = new Mesh();
		m.vertices = vertices;
		m.triangles = tris.ToArray();
		m.RecalculateNormals();
		meshMask.sharedMesh = m;
	}

	private struct MeshPoint {
		public Vector2 v { get; set; }
		public bool CoupledPoint { get; set; }

		public MeshPoint(Vector2 _v, bool cp = false) {
			v = _v;
			CoupledPoint = cp;
		}
	}
}
