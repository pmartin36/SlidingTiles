using UnityEngine;

public interface ISpringable {
	float Vx { get; }
	void Spring(Vector2 dir);
}

