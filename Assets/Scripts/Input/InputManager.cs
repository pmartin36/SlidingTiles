using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	private Camera main;
	protected InputPackage last;
	protected InputPackage p;
	protected IInputController InputController;

	public ContextManager ContextManager { get; set; }

	protected void Start() {
		main = Camera.main;
		p = new InputPackage();
		if(Application.isMobilePlatform) {
			InputController = new TouchController();
		}
		else if(Application.isConsolePlatform) {
			InputController = new MouseController(); // obv need to change
		}
		else {
			InputController = new MouseController();
		}
	}

	protected virtual void Update() {
		if(ContextManager.AcceptingInputs) {
			InputController.GetInput(p, last, main);		
		}

		// handle inputs must still be called
		ContextManager.HandleInput(p);
		last = new InputPackage(p);
	}
}

public class InputPackage {
	public Vector3 MousePositionWorldSpace { get; set; }
	public Vector3 MousePositionWorldSpaceDelta { get; set; }

	public bool Touchdown { get; set; }
	public bool TouchdownChange { get; set; }

	public InputPackage() {}
	public InputPackage(InputPackage p) {
		MousePositionWorldSpace = p.MousePositionWorldSpace;
		MousePositionWorldSpaceDelta = p.MousePositionWorldSpaceDelta;

		Touchdown = p.Touchdown;
		TouchdownChange = p.TouchdownChange;
	}
}
