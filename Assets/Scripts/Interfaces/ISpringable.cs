using UnityEngine;

public interface ISpringable {
	float Vx { get; }
	bool Grounded { get; }
	void Spring(Vector2 dir);
}

