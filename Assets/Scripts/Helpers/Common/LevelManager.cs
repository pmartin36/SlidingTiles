using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class LevelManager : ContextManager {

	public static event EventHandler<Vector2> Move;

	private LayerMask tileMask;
	private Tile SelectedTile;
	private Vector3 grabPoint;

	public override void Awake() {
		base.Awake();
		GetComponent<InputManager>().ContextManager = this;
		tileMask = 1 << LayerMask.NameToLayer("Tile");
	}

	public override void HandleInput(InputPackage p) {
		if(p.LeftMouse) { 
			if(p.LeftMouseChange) {
				// clicked
				SelectedTile = Physics2D.OverlapPoint(p.MousePositionWorldSpace, tileMask)?.GetComponent<Tile>();

				if (SelectedTile != null) {
					grabPoint = p.MousePositionWorldSpace;
					SelectedTile.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0, 0);
				}
			}
			else if(SelectedTile != null) {
				Vector2 moveAmount = (p.MousePositionWorldSpace - grabPoint) / SelectedTile.transform.lossyScale.x;
				bool moved = SelectedTile.TryMove(moveAmount);
				Move?.Invoke(this, moveAmount);

				if(moved) {
					grabPoint = p.MousePositionWorldSpace;
				}
			}
		}
		else if(SelectedTile != null && !p.LeftMouse && p.LeftMouseChange) {
			ClearMovingTiles();
			SelectedTile.GetComponent<SpriteRenderer>().color = new Color(0.3f, 0, 0);
			SelectedTile = null;
		}
	}

	public static void ClearMovingTiles() {
		Move = null;
	}
}
