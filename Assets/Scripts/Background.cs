using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
	public SpriteRenderer SpriteRenderer { get; private set; }
	public List<Material> AdditionalMaterials;

	private void Awake() {
		SpriteRenderer = GetComponent<SpriteRenderer>();
		AdditionalMaterials.Add(SpriteRenderer.sharedMaterial);
		SpriteRenderer.sharedMaterials = AdditionalMaterials.ToArray();
	}

	void Start() {
		Sprite s = SpriteRenderer.sprite;
		Bounds spriteBounds = s.bounds;

		Camera camera = CameraManager.Instance.Camera;
		Vector2 camSize = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize) * 1.2f; // little buffer for screen shake

		transform.position = (Vector2)(camera.transform.position);
		Vector2 minDim = camSize / spriteBounds.extents;
		if(minDim.x > minDim.y) {
			transform.localScale = Vector3.one * minDim.x;
		}
		else {
			transform.localScale = Vector3.one * minDim.y;
		}
    }

}
