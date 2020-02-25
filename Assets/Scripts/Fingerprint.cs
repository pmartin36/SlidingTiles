using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fingerprint : MonoBehaviour
{
	public float Percent;
	private SpriteRenderer spriteRenderer;

	public void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Update() {
		// spriteRenderer.sharedMaterial.SetFloat("_Percent", Percent);
	}

	public void OnEnable() {
		GetComponent<Animator>().playbackTime = 0;
		// spriteRenderer.sharedMaterial.SetFloat("_Percent", 0);
	}
}
