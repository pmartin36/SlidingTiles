using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	private Camera main;
	private InputPackage last;

	public ContextManager ContextManager { get; set; }

	private void Start() {
		main = Camera.main;
	}

	private void Update() {
		InputPackage p = new InputPackage();

		if(ContextManager.AcceptingInputs) {
			p.MousePositionWorldSpace = (Vector2)main.ScreenToWorldPoint(Input.mousePosition);
			p.LeftMouse = Input.GetButton("LeftMouse");
			if(last != null) {
				p.LeftMouseChange = p.LeftMouse ^ last.LeftMouse;
				p.MousePositionWorldSpaceDelta = p.MousePositionWorldSpace - last.MousePositionWorldSpace;
			}
			else {
				p.LeftMouseChange = false;
				p.MousePositionWorldSpaceDelta = Vector3.zero;
			}
		

			p.Horizontal = Input.GetAxis("Horizontal");
			p.Vertical = Input.GetAxis("Vertical");		
		}

		// handle inputs must still be called
		ContextManager.HandleInput(p);
		last = p;
	}
}

public class InputPackage {
	public Vector3 MousePositionWorldSpace { get; set; }
	public Vector3 MousePositionWorldSpaceDelta { get; set; }

	public float MouseWheelDelta { get; set; }
	public bool LeftMouse { get; set; }
	public bool LeftMouseChange { get; set; }
	public bool RightMouse { get; set; }
	public bool Shift { get; set; }

	public bool Enter { get; set; }
	public float GasAmount { get; set; }
	public bool Break { get; set; }
	public bool Dash { get; set; }
	public bool Jump { get; set; }
	public float Horizontal { get; set; }
	public float Vertical { get; set; }
}
