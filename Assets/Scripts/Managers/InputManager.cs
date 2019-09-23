using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	private Camera main;
	protected InputPackage last;
	protected InputPackage p;

	public ContextManager ContextManager { get; set; }

	protected void Start() {
		main = Camera.main;
		p = new InputPackage();
	}

	protected virtual void Update() {
		if(ContextManager.AcceptingInputs) {
			p.MousePositionWorldSpace = (Vector2)main.ScreenToWorldPoint(Input.mousePosition);
			p.Touchdown = Input.GetButton("LeftMouse");
			if(last != null) {
				p.TouchdownChange = p.Touchdown ^ last.Touchdown;
				p.MousePositionWorldSpaceDelta = p.MousePositionWorldSpace - last.MousePositionWorldSpace;
			}
			else {
				p.TouchdownChange = false;
				p.MousePositionWorldSpaceDelta = Vector3.zero;
			}		
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
