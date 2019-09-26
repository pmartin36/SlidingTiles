using UnityEngine;

public class MouseController : InputController {
	public MouseController() {
		InputType = InputType.Mouse;
	}

	public override InputPackage GetInput() {
		package.MousePositionWorldSpace = (Vector2)main.ScreenToWorldPoint(Input.mousePosition);
		package.Touchdown = Input.GetButton("LeftMouse");
		package.PointerOverGameObject = EventSystem?.IsPointerOverGameObject(-1) ?? false;
		if (lastFramePackage != null) {
			package.TouchdownChange = package.Touchdown ^ lastFramePackage.Touchdown;
			package.MousePositionWorldSpaceDelta = package.MousePositionWorldSpace - lastFramePackage.MousePositionWorldSpace;
		}
		else {
			package.TouchdownChange = false;
			package.MousePositionWorldSpaceDelta = Vector3.zero;
		}
		lastFramePackage = new InputPackage(package);
		return package;
	}
}

