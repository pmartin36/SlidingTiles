using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager> {
    
	public Camera MainCamera { get; private set; }

	public void Create(Camera cameraPrefab, bool isMain) {
		transform.position = Vector3.zero;
		MainCamera = Instantiate(cameraPrefab, cameraPrefab.transform.position, cameraPrefab.transform.rotation, this.transform); 
	}
}
