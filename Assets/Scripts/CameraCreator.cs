using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraCreator : MonoBehaviour
{
	public CameraController MainCameraPrefab;

    void Awake() {
		Screen.orientation = ScreenOrientation.Portrait;

		if(CameraManager.Instance.CameraController == null) {
			CameraManager.Instance.Create(MainCameraPrefab, true);
		}
		GameManager gm = GameManager.Instance; // create the game and music managers as well
		MusicManager mm = MusicManager.Instance;

		var pp = GameObject.FindObjectOfType<PostProcessVolume>();
		if(pp != null) {
			CameraManager.Instance.CameraController.PostProcessVolume = pp;
		}
    }
}
