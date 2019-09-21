using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : LevelManager
{
	public GameObject Arrow;
	public GameObject Ghost;

	public GameObject TutorialTile;
	public GameObject ActualTile;

	private Animator anim;
	private Vector3 TutorialTileLastPosition;

    public override void Start() {
		base.Start();
		ActualTile.SetActive(false);
	}

	public override void Init() {
		base.Init();
		anim = GetComponent<Animator>();
		PlayAnimation();
	}

	public override void Reset(bool fromButton) {
		// Arrow.SetActive(true);

		Ghost.transform.position = RespawnManager.PlayerSpawnPosition;
		Ghost.SetActive(true);

		ActualTile.SetActive(false);
		TutorialTile.transform.position = ActualTile.transform.position;
		TutorialTileLastPosition = ActualTile.transform.position;
		TutorialTile.SetActive(true);

		PlayAnimation();
		base.Reset(fromButton);
    }

	public override void Respawn() {
		Ghost.SetActive(false);
		// Arrow.SetActive(false);

		ActualTile.SetActive(true);
		TutorialTile.SetActive(false);

		anim.enabled = false;
		base.Respawn();
	}

	public void EndAnimation() {
		Reset(true);
	}

	public void PlayAnimation() {
		anim.enabled = true;
		anim.Play("Tutorial_" + GameManager.Instance.GetSceneName(), -1, 0);
	}
}
