using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraCreator : MonoBehaviour
{
	public CameraController MainCameraPrefab;

    void Awake() {
		Screen.orientation = ScreenOrientation.Landscape;

		if(CameraManager.Instance.CameraController == null) {
			CameraManager.Instance.Create(MainCameraPrefab, true);
		}
		GameManager gm = GameManager.Instance; // create the game manager as well

		var pp = GameObject.FindObjectOfType<PostProcessVolume>();
		if(pp != null) {
			CameraManager.Instance.CameraController.PostProcessVolume = pp;
		}
    }
}
