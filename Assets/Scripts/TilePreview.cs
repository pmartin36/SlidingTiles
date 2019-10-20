using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePreview : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;

	public Color TargetColor { get; private set; }
	public Vector3 TargetPosition { get; set; }
	public Vector3 WatchedPosition {
		get => spriteRenderer.material.GetVector("_PreviewWorldPosition");
		set => spriteRenderer.material.SetVector("_PreviewWorldPosition", value);
	}
	public bool Active => spriteRenderer.color.a > 0.1f;

    void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		TargetColor = new Color(1,1,1,0);
    }

    void Update() {
		transform.position = Vector3.Lerp(transform.position, TargetPosition, 0.1f);
		spriteRenderer.color = Color.Lerp(spriteRenderer.color, TargetColor, 0.1f);
    }

	public void Show(bool show, Vector3? target = null) {
		if(target.HasValue) {
			TargetPosition = target.Value;
			if(!Active) {
				transform.position = TargetPosition;
			}
		}

		TargetColor = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, show ? 1 : 0);
	}
}
