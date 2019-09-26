using UnityEngine;

public abstract class InputController {
	public InputType InputType { get; protected set; }
	protected Camera main;
	protected InputPackage package;
	protected InputPackage lastFramePackage;
	protected UnityEngine.EventSystems.StandaloneInputModule EventSystem;

	public static InputController CreateInputController() {
		if (Application.isMobilePlatform) {
			return new TouchController();
		}
		else if (Application.isConsolePlatform) {
			return new MouseController(); // TODO: Replace with ControllerController
		}
		else {
			return new MouseController();
		}
	}

	public InputController() {
		main = Camera.main;
		package = new InputPackage();
		EventSystem = GameObject.FindObjectOfType<UnityEngine.EventSystems.StandaloneInputModule>();
	}

	public abstract InputPackage GetInput();
}

public enum InputType {
	Touch,
	Controller,
	Mouse
}

public class InputPackage {
	public Vector3 MousePositionWorldSpace { get; set; }
	public Vector3 MousePositionWorldSpaceDelta { get; set; }

	public bool Touchdown { get; set; }
	public bool TouchdownChange { get; set; }

	public bool PointerOverGameObject { get; set; }

	public static InputPackage Empty = new InputPackage();

	public InputPackage() { }
	public InputPackage(InputPackage p) {
		MousePositionWorldSpace = p.MousePositionWorldSpace;
		MousePositionWorldSpaceDelta = p.MousePositionWorldSpaceDelta;

		Touchdown = p.Touchdown;
		TouchdownChange = p.TouchdownChange;
		PointerOverGameObject = p.PointerOverGameObject;
	}
}

