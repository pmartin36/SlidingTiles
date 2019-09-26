using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCreator : MonoBehaviour
{
	public Camera MainCameraPrefab;

    void Awake() {
		if(CameraManager.Instance.MainCamera == null) {
			CameraManager.Instance.Create(MainCameraPrefab, true);
		}
		GameManager gm = GameManager.Instance; // create the game manager as well
    }
}
