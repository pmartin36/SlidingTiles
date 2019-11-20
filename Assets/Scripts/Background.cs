using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    void Start() {
		Sprite s = GetComponent<SpriteRenderer>().sprite;
		Bounds spriteBounds = s.bounds;

		Camera camera = CameraManager.Instance.Camera;
		Vector2 camSize = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize) * 1.2f; // little buffer for screen shake

		transform.position = (Vector2)(camera.transform.position);
		transform.localScale = camSize / spriteBounds.extents;
    }

}
