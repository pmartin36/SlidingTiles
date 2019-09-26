using UnityEngine;

public class MouseController : IInputController {
	public void GetInput(InputPackage p, in InputPackage last, in Camera main) {
		p.MousePositionWorldSpace = (Vector2)main.ScreenToWorldPoint(Input.mousePosition);
		p.Touchdown = Input.GetButton("LeftMouse");
		if (last != null) {
			p.TouchdownChange = p.Touchdown ^ last.Touchdown;
			p.MousePositionWorldSpaceDelta = p.MousePositionWorldSpace - last.MousePositionWorldSpace;
		}
		else {
			p.TouchdownChange = false;
			p.MousePositionWorldSpaceDelta = Vector3.zero;
		}
	}
}

