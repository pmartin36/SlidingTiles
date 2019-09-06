using UnityEngine;
using System.Collections.Generic;

public interface IMoveableCollider {
    Tile Parent { get; }
    Vector2 CalculateValidMoveAmount(Vector2 original, ref Tile extraTileToMove);
}

public interface IPushable {
	bool Move(Vector2 amount, Direction d);
}

