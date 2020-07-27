using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tilespace : MonoBehaviour
{
	private Grid grid;
	public Tile Tile { get; set; }
	public bool HasTile { get => Tile != null; }

	// visible in inspector
	public TilePosition Position;

	public bool Sticky { get; private set; }
	public TileRotator Rotator { get; private set; }

	public Tile InitialTile { get; private set; }

	void Awake() {
		transform.localPosition = new Vector2(Position.x, Position.y) * transform.localScale.x;
		Tile = GetComponentInChildren<Tile>();
		Rotator = GetComponentInChildren<TileRotator>();
		Sticky = GetComponentInChildren<Sticky>() != null;
		if(Tile != null) {
			InitialTile = Tile;
			Tile.Init(this);
		}
    } 

	public void Init(Grid g) {
		grid = g;
	}

	public Tilespace GetNeighborInDirection(Direction d) {
		return grid.GetTilespaceInDirection(this.Position, d);
	}

	public void SetChildTile(Tile t, bool triggerEffects) {
		t.Space = this;
		t.transform.parent = this.transform;
		//t.transform.localPosition = local;
		this.Tile = t;

		if(triggerEffects) {
			if(Rotator != null) {	
				StartCoroutine(CenterThenRotate(t));
			}
		}
	}

	public void TryMoveTile(Direction direction) {
		//Tilespace t = grid.GetTilespaceInDirection(Position, direction, out bool oob);
		//List<Tilespace> tilespaces = new List<Tilespace>() { this };
		//if (!oob && t.CanMoveInto(direction, tilespaces)) {
		//	for (int i = tilespaces.Count - 1; i > 0; i--) {
		//		tilespaces[i - 1].Tile.Move(tilespaces[i]);
		//	}
		//}
	}

	public void Reset() {
		if(InitialTile != null) {
			SetChildTile(InitialTile, false);
			this.InitialTile.Reset();
		}
		else {
			Tile = null;
		}

		if(Rotator != null) {
			Rotator.ResetAnimation();
		}
	}

	public void PlayerWon() {
		if(Rotator != null) {
			Rotator.StopRotating();
		}
	}

	private IEnumerator CenterThenRotate(Tile t) {
		t.SetTemporaryUnmovable(true);
		yield return new WaitUntil(() => t.Centered);
		Rotator.BeginRotating();
	}
}
