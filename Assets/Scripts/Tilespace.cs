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

	// Will tiles get stuck if they move onto this tilespace
	public bool Sticky;
	public TileRotator Rotator;

	public Tile initialTile;

	void Awake() {
		transform.localPosition = new Vector2(Position.x, Position.y) * transform.localScale.x;
		Tile = GetComponentInChildren<Tile>();
		Rotator = GetComponentInChildren<TileRotator>();
		if(Tile != null) {
			initialTile = Tile;
			Tile.Init(this);
		}
    } 

	public void Init(Grid g) {
		grid = g;
	}

	public Tilespace GetNeighborInDirection(Direction d) {
		return grid.GetTilespaceInDirection(this.Position, d);
	}

	public void SetChildTile(Tile t) {
		t.Space = this;
		t.transform.parent = this.transform;
		//t.transform.localPosition = local;

		this.Tile = t;
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
		if(initialTile != null) {
			SetChildTile(initialTile);
			this.initialTile.Reset();
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
}
