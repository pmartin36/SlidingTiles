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

    void Start() {
		transform.localPosition = new Vector2(Position.x, Position.y) * transform.localScale.x;
		Tile = GetComponentInChildren<Tile>();
		if(Tile != null) {
			Tile.Space = this;
		}
    }

	public void Init(Grid g) {
		grid = g;
	}

    public bool CanMoveTo(Direction direction, bool checkHasTile = true) {
		if(checkHasTile) {
			if (!HasTile) {
				return true;
			}
			else {
				// add to the list of tiles that need to move when selected tile moves
				Tile.MoveDirection = direction;
				LevelManager.Move += Tile.MoveEvent;
			}
		}
	
		Tilespace t = grid.GetTilespaceInDirection(Position, direction);
		if(t != null){		
			return t.CanMoveTo(direction);
		}
		else {
			return false;	
		}
	}

	public Tilespace GetNeighborInDirection(Direction d) {
		return grid.GetTilespaceInDirection(this.Position, d);
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
}
