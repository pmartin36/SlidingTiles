using UnityEngine;
using System.Collections.Generic;

public interface IMoveableCollider {
    Tile Parent { get; }
    Vector2 CalculateValidMoveAmount(Vector2 original, Dictionary<Transform, float> tileMoveDelta, float currentDelta, ref Tile extraTileToMove);
}

