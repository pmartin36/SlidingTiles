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

				if (SelectedTile != null && SelectedTile.Movable) {
					grabPoint = p.MousePositionWorldSpace;
					SelectedTile.Select(true);
				}
			}
			else if(SelectedTile != null) {
				float scale = SelectedTile.transform.lossyScale.x;
				Vector2 moveAmount = (p.MousePositionWorldSpace - grabPoint);

				Tilespace tileBeforeMove = SelectedTile.Space;
				bool moved = SelectedTile.TryMove(moveAmount, p.MousePositionWorldSpaceDelta);

				if(moved) {
					Vector3 move = (tileBeforeMove.transform.position - SelectedTile.Space.transform.position);
					//grabPoint -= move;
					//if(Mathf.Abs(move.y) > Mathf.Abs(move.x)) {
					//	grabPoint.x = p.MousePositionWorldSpace.x;
					//}
					//else {
					//	grabPoint.y = p.MousePositionWorldSpace.y;
					//}
					// SelectedTile.Select(true);
				}
			}
		}
		else if(SelectedTile != null && !p.LeftMouse && p.LeftMouseChange) {
			SelectedTile.Select(false);
			SelectedTile = null;
		}
	}

	public static void ClearMovingTiles() {
		Move = null;
	}
}
