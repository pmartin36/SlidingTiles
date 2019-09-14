using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalFlag : MonoBehaviour
{
    public SpriteRenderer Flag;
	public ParticleSystem particles;

	public void Start() {
		particles = GetComponentInChildren<ParticleSystem>(true);
	}

	public void PlayerReached() {
		particles.gameObject.SetActive(true);
		GameManager.Instance.LevelManager.PlayerWin(this);
	}

	public void Reset() {
		particles.gameObject.SetActive(false);
	}
}
