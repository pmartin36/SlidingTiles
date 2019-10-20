using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITilePreview : MonoBehaviour
{
	private Image spriteRenderer;

	public Color TargetColor { get; private set; }
	public Vector3 TargetPosition { get; set; }
	public Vector3 WatchedPosition {
		get => spriteRenderer.material.GetVector("_PreviewWorldPosition");
		set => spriteRenderer.material.SetVector("_PreviewWorldPosition", value);
	}
	public bool Active => spriteRenderer.color.a > 0.1f;

	void Start() {
		spriteRenderer = GetComponent<Image>();
		TargetColor = new Color(1,1,1,0);
    }

    void Update() {
		spriteRenderer.color = Color.Lerp(spriteRenderer.color, TargetColor, 0.1f);
    }

	public void Show(bool show, Vector3? target = null) {
		TargetColor = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, show ? 1 : 0);
	}
}
