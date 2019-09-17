using UnityEngine;
using System.Collections.Generic;

public interface IMoveableCollider {
    Tile Parent { get; }
    void CheckAndRemoveSquishables(Vector2 original);
}

public interface ISquishable {
	bool WasSquishedThisFrame { get; set; }
	bool CheckSquishedAndResolve(Vector2 original);
}

