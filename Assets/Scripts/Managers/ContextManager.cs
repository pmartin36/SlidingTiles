﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContextManager : MonoBehaviour
{
	public bool AcceptingInputs { get; set; } = true;
	protected InputController InputController;

	public virtual void Awake() {
		
	}

	public virtual void Start() {
		InputController = InputController.CreateInputController();
	}

	public virtual void Update() {
		HandleInput(
			AcceptingInputs 
			? InputController.GetInput()
			: InputPackage.Empty
		);
	}

	public abstract void HandleInput(InputPackage p);
}
