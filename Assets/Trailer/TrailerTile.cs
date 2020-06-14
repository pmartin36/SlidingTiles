using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerTile : Tile
{
	public RectTransform MatchingNumber;
	private Vector2 lastPosition;

	protected override void Start() {
		base.Start();
		lastPosition = transform.position;
	}

	public override void LateUpdate() {
        MatchingNumber.anchoredPosition += (Vector2)transform.position - lastPosition;
		lastPosition = transform.position;
    }
}
