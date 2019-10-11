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
	private LayerMask playerMask;

	public void TutorialInit(
		Action onselect,
		Action onmove,
		Action<bool> onplayer
	) {
		OnSelected = onselect;
		OnMovedFromCenter = onmove;
		OnPlayerEnteredOrLeft = onplayer;
		playerMask = 1 << LayerMask.NameToLayer("Player");
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

	public override void Update() {
		base.Update();
		
		bool isPlayerInside = Physics2D.OverlapBox(transform.position, transform.lossyScale, 0, playerMask) != null;
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
