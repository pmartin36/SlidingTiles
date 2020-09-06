using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileRotator : MonoBehaviour
{
	[Range(-1, 1)]
	public int Direction;

	private const float downtime = 1.5f;
	private float timeOfLastRotation;

	private Tilespace tilespace;
	private Tile effectedTile;

	private float rotation;
	private float targetRotation;
	private bool rotating;

	private LevelManager lm;

	private Animator animator;
	private AudioSource audio;

    void Start() {
		tilespace = transform.parent.GetComponent<Tilespace>();
		rotation = 0f;
		transform.localRotation = Quaternion.identity;
		transform.localPosition = new Vector2(0, -0.025f);

		foreach(Transform child in transform) {
			if(child.CompareTag("Base")) {
				var t = child.localScale;
				t.x *= Direction;
				child.localScale = t;
			}
		}

		animator = GetComponent<Animator>();
		audio = GetComponent<AudioSource>();
    }

    void Update() {
		if(lm == null || !lm.Won) {
			if(!rotating && Time.time - timeOfLastRotation > downtime) {
				BeginRotating();
			}

			if(rotating) {
				float add = Direction * 180 * Time.deltaTime;
				rotation += add;
				if( Mathf.Abs(rotation) + 0.01f >= Mathf.Abs(targetRotation) ) { // 0.01f buffer
					if(effectedTile != null) {
						effectedTile.Rotation = (Mathf.Round((effectedTile.Rotation + add) / 90f) * 90f) % 360;
						effectedTile.EndRotation();
						ClearEffectedTile();
					}
					rotating = false;
				}
				else if(effectedTile != null) {
					effectedTile.Rotation += add;
				}
			}
		}
    }

	public void ClearEffectedTile() {
		effectedTile = null;
	}

	public void BeginRotating() {
		var tile = tilespace.Tile;
		if(tile != null && tile.Centered) {
			effectedTile = tile;

			if (lm == null) {
				lm = GameManager.Instance.LevelManager;
			}
			rotating = true;
			timeOfLastRotation = Time.time;
			rotation = 0f;
			targetRotation = 90f * Direction;
			audio.volume = 0.5f * GameManager.Instance.SaveData.FxVolume;
			audio.Play();

			animator.SetFloat("Direction", Direction);
			animator.Play("tile_rotator", -1, Direction > 0 ? 0 : 1);

			effectedTile.BeginRotation(Direction, this);
		}
	}

	public void ResetAnimation() {
		rotation = targetRotation;
		timeOfLastRotation = Time.time;
		animator.SetFloat("Direction", 0);
		animator.Play("tile_rotator", -1, 0);
	}

	public void HardStopRotating() {
		effectedTile?.EndRotation(true);
		animator.SetFloat("Direction", 0);
	}

	public void SetTimeOfLastRotation(float time) {
		timeOfLastRotation = time;
	}
}
