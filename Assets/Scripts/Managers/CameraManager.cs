using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager> {
    
	public CameraController CameraController { get; private set; }
	public Camera Camera => CameraController.Camera;

	public void Create(CameraController cameraPrefab, bool isMain) {
		transform.position = Vector3.zero;
		CameraController = Instantiate(cameraPrefab, cameraPrefab.transform.position, cameraPrefab.transform.rotation, this.transform); 
	}
}
