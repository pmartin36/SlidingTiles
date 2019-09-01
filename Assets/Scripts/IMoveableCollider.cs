using UnityEngine;
using System.Collections.Generic;

public interface IMoveableCollider {
	Vector2 CalculateValidMoveAmount (Vector2 original, HashSet<IMoveableCollider> checkedColliders);
}

