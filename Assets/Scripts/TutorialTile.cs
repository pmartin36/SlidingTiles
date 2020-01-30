using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTile : Tile
{
	public Action OnSelected;
	public Action OnMovedFromCenter;
	public Action<bool> OnPlayerEnteredOrLeft;

	private bool playerInside = false;

	public void TutorialInit(
		Action onselect,
		Action onmove,
		Action<bool> onplayer
	) {
		OnSelected = onselect;
		OnMovedFromCenter = onmove;
		OnPlayerEnteredOrLeft = onplayer;
	}

	public override void Select(bool select) {
		base.Select(select);
		if(Selected) {
			OnSelected?.Invoke();
		}
	}

	public override bool Move(Vector3 moveAmount, Direction d) {
		bool wasCentered = Centered;
		bool returnValue = base.Move(moveAmount, d);
		if(wasCentered && !Centered) {
			OnMovedFromCenter?.Invoke();
		}
		return returnValue;
	}

	public void Update() {
		bool isPlayerInside = IsPlayerOnTile();
		if(isPlayerInside ^ playerInside) {
			playerInside = isPlayerInside;
			OnPlayerEnteredOrLeft?.Invoke(playerInside);
		}
	}

	public void OnDestroy() {
		OnSelected = null;
		OnMovedFromCenter = null;
		OnPlayerEnteredOrLeft = null;
	}
}
