using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawnTest : MonoBehaviour
{
	public Vector3 RespawnPosition;
	public Sprite[] sprites;

    void Start() {
		StartCoroutine(Respawn());
    }

    void Update() {
        
    }

	public IEnumerator Respawn() {
		CameraController c = CameraManager.Instance.CameraController;
		BlurComposite blur = c.GetModifiablePostProcessSettings<BlurComposite>();

		Vector3 startPosition = transform.position;
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		
		Vector3 diff = RespawnPosition - startPosition;
		Vector3 cubicPoint = startPosition + new Vector3(-diff.x * 0.2f, diff.y * 1.5f, 0);

		float tShrinkGrow = 0.5f;
		float tMove = 2f;

		var trailRenderer = GetComponentInChildren<TrailRenderer>();
		var ps = GetComponentInChildren<ParticleSystem>();
		var emission = ps.emission;
		int i = 0;

		while(true){
			// should enable blur camera
			// should enable post process volume filter
			trailRenderer.enabled = true;
			float t = 0;
			while(t < tShrinkGrow) {
				float v = t / tShrinkGrow;
				float r = Mathf.Lerp(1f, 0.6f, v);
				sr.material.SetFloat("_DistortRadius", r);

				float b = Mathf.Lerp(1, 2, v*1.1f);
				blur.intensity.value = b;

				float scale = Mathf.Lerp(4, 5, v);
				transform.localScale = scale * Vector3.one;

				t += Time.deltaTime;
				yield return null;
			}
			sr.material.SetFloat("_DistortRadius", 0.6f);
			blur.intensity.value = 2;
			transform.localScale = 5 * Vector3.one;

			// move
			t = 0;
			emission.enabled = true;
			while(t < tMove) {
				float mt = Mathf.SmoothStep(0, 1, t);
				Vector3 pos =	(1-mt) * (1-mt) * startPosition 
								+ 2 * (1-mt) * mt * cubicPoint
								+ mt * mt * RespawnPosition;
				transform.position = pos;

				t += Time.deltaTime;
				yield return null;
			}
			emission.enabled = false;

			sr.sprite = sprites[++i%sprites.Length];

			// grow
			t = 0;
			while (t < tShrinkGrow) {
				float v = t / tShrinkGrow;
				float r = Mathf.Lerp(0.6f, 1f, v);
				sr.material.SetFloat("_DistortRadius", r);

				float b = (1-t) * 2;
				blur.intensity.value = b;

				float scale = Mathf.Lerp(5, 4, v);
				transform.localScale = scale * Vector3.one;

				t += Time.deltaTime;
				yield return null;
			}
			sr.material.SetFloat("_DistortRadius", 1f);
			blur.intensity.value = 0;
			transform.localScale = 4 * Vector3.one;

			// should disable blur camera
			// should disable post process volume filter

			yield return new WaitForSeconds(2f);
			trailRenderer.enabled = false;
			transform.position = startPosition;
			trailRenderer.Clear();
			yield return new WaitForSeconds(1f);
		}
	}
}
