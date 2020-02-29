using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraControllerFixedBackground : CameraController
{
	public void Start() {
		Camera.depthTextureMode = DepthTextureMode.Depth;
	}


	protected override void Update() {
		base.Update();
	}
}
