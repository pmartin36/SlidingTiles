using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Star : MonoBehaviour
{
	public float RotationAmount;
	public bool WasCollected => animator.GetBool("collected");
	private Animator animator;
	private AudioSource audio;

	private ParticleSystem ps;

	private SpriteRenderer collectedSprite;

    void Start() {
		animator = GetComponentInParent<Animator>();
		audio = GetComponent<AudioSource>();
		ps = this.transform.parent.parent.GetComponentInChildren<ParticleSystem>();
		collectedSprite = this.transform.parent.parent.GetComponentsInChildren<SpriteRenderer>(true).First(g => g.gameObject != this.gameObject);
		ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		StartIdleAnimation();
		GetComponent<SpriteRenderer>().material.SetFloat("_Seed", Random.Range(0f, 100f));
	}

    void Update() {
        
    }

	public void Collected(Vector2 direction) {
		int starCount = GameManager.Instance.LevelManager.AddStar();
		GetComponent<PolygonCollider2D>().enabled = false;
		animator.SetBool("collected", true);

		float f = 1f + (starCount-1) * 0.125f;
		audio.volume = 0.25f * GameManager.Instance.SaveData.FxVolume;
		audio.pitch = f;
		audio.Play();

		float angle = Utils.VectorToAngle(direction) - 22.5f;
		ps.transform.parent = null;
		ps.Play();
		ps.transform.localRotation = Quaternion.Euler(0,0,angle);
		StartCoroutine(StopEmitting());
	}

	private IEnumerator StopEmitting() {
		yield return null;
		yield return null;
		ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		ps.transform.parent = this.transform.parent;
	}

	public void Reset() {
		if(WasCollected) {
			GetComponent<PolygonCollider2D>().enabled = true;
			StartIdleAnimation();
		}
	}

	private void StartIdleAnimation() {
		animator.SetBool("collected",false);
		float startTime = Random.Range(0, animator.GetCurrentAnimatorStateInfo(0).length);
		animator.Play("star", 0, startTime);
	}
}
