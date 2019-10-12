using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    void Start() {
		Sprite s = GetComponent<SpriteRenderer>().sprite;
		Bounds spriteBounds = s.bounds;
		Debug.Log(spriteBounds.size);

		Camera camera = CameraManager.Instance.MainCamera;
		Vector2 camSize = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);

		transform.position = (Vector2)(camera.transform.position);
		transform.localScale = camSize / spriteBounds.extents;
    }
}
