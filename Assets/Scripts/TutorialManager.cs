using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : LevelManager
{
	public SpriteRenderer Finger;
	public GameObject Ghost;

	public GameObject[] TutorialTiles;
	public GameObject[] ActualTiles;

	private Animator anim;
	private Vector3[] TutorialTileLastPositions;

    public override void Start() {
		base.Start();
		foreach(GameObject o in ActualTiles) {
			o.SetActive(false);
		}
		TutorialTileLastPositions = new Vector3[TutorialTiles.Length];
	}

	public override void Init() {
		base.Init();
		anim = GetComponent<Animator>();
		PlayAnimation();
	}

	public override void CreateRespawnManager() {
		RespawnManager = new RespawnManager(false);
	}

	public override void Reset(bool fromButton) {
		Ghost.transform.position = RespawnManager.PlayerSpawnPosition;
		Ghost.SetActive(true);

		for(int i = 0; i < TutorialTiles.Length; i++) {
			GameObject TutorialTile = TutorialTiles[i];
			GameObject ActualTile = ActualTiles[i];
			ActualTile.SetActive(false);
			TutorialTile.transform.position = ActualTile.transform.position;
			TutorialTileLastPositions[i] = ActualTile.transform.position;
			TutorialTile.SetActive(true);
		}	

		Finger.gameObject.SetActive(true);

		PlayAnimation();
		base.Reset(fromButton);
    }

	public override void Respawn() {
		Ghost.SetActive(false);
		Finger.gameObject.SetActive(false);

		for (int i = 0; i < TutorialTiles.Length; i++) {
			ActualTiles[i].SetActive(true);
			TutorialTiles[i].SetActive(false);
		}	

		anim.enabled = false;
		base.Respawn();
	}

	public void EndAnimation() {
		Ghost.SetActive(false);
		Finger.gameObject.SetActive(false);

		for (int i = 0; i < TutorialTiles.Length; i++) {
			ActualTiles[i].SetActive(true);
			TutorialTiles[i].SetActive(false);
		}
		
		RespawnManager.ActionButtons.HighlightSpawn(true);
		anim.enabled = false;
	}

	public void PlayAnimation() {
		anim.enabled = true;
		anim.Play("Tutorial_" + GameManager.Instance.GetSceneName(), -1, 0);
	}
}
