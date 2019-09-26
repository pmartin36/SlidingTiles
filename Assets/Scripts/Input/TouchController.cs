using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TouchController : InputController {
	public TouchController() {
		InputType = InputType.Touch;
	}

	public override InputPackage GetInput() {
		if(Input.touchCount > 0) {
			package.Touchdown = true;
			package.PointerOverGameObject = EventSystem?.IsPointerOverGameObject(0) ?? false;

			Touch t = Input.GetTouch(0);
			package.MousePositionWorldSpace = (Vector2)main.ScreenToWorldPoint(t.position);
		}
		else {
			package.Touchdown = false;
			package.PointerOverGameObject = lastFramePackage?.PointerOverGameObject ?? false;
			package.MousePositionWorldSpace = lastFramePackage?.MousePositionWorldSpace ?? Vector2.zero;
		}

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
