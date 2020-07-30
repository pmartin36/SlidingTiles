using UnityEngine;

public interface ISpringable {
	float GravityAngle { get; }
	float Vx { get; }
	bool Grounded { get; }
	void Spring(Vector2 dir);
}

