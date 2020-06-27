using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileRotator : MonoBehaviour
{
	[Range(-1, 1)]
	public int Direction;
	public float Downtime;

	private float timeSinceLastRotation;

	private Tilespace tilespace;
	private Tile effectedTile;

	private float rotation;
	private float targetRotation;
	private bool rotating;

	private Animator animator;

    void Start() {
		tilespace = transform.parent.GetComponent<Tilespace>();
		rotation = transform.localEulerAngles.z;

		animator = GetComponent<Animator>();
    }

    void Update() {
        if(Time.time - timeSinceLastRotation > Downtime) {
			rotating = true;
			timeSinceLastRotation = Time.time;
			targetRotation = rotation + 90f * Direction;

			animator.SetFloat("Direction", Direction);
			animator.Play("tile_rotator", -1, Direction > 0 ? 0 : 1);

			var tile = tilespace.Tile;
			if(tile != null && tile.Centered) {
				effectedTile = tile;
				effectedTile.BeginRotation(Direction);
			}
		}

		if(rotating) {
			float add = Direction * 180 * Time.deltaTime;
			rotation += add;
			//rotation += Direction * 45 * Time.deltaTime;
			if( Mathf.Abs(rotation) >= Mathf.Abs(targetRotation) ) {
				if(effectedTile != null) {
					effectedTile.Rotation = (Mathf.Round((effectedTile.Rotation + add) / 90f) * 90f) % 360;
					effectedTile.EndRotation();
					effectedTile = null;
				}
				rotation = targetRotation % 360;
				rotating = false;
			}
			else if(effectedTile != null) {
				effectedTile.Rotation += add;
			}
		}
    }
}
