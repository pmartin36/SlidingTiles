﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MenuInputManager : InputManager
{
	public UnityEngine.EventSystems.StandaloneInputModule EventSystem;

	protected override void Update() {
		p.Touchdown = Input.GetButton("LeftMouse") && !EventSystem.IsPointerOverGameObject(-1);
		if (last != null) {
			p.TouchdownChange = p.Touchdown ^ last.Touchdown;
		}
		ContextManager.HandleInput(p);
		last = new InputPackage(p);
	}
}
