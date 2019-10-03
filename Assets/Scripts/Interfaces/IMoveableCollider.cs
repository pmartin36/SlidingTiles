using UnityEngine;
using System.Collections.Generic;

public interface IMoveableCollider {
    Tile Parent { get; }
    void CheckBlocking (ref Vector2 original, HashSet<Tile> movingTiles);
}

public interface IPlatformMoveBlocker {
	bool WasSquishedThisFrame { get; set; }
	bool CheckBlocking (ref Vector2 original, HashSet<Tile> movingTiles);
}

