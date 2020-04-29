using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraCreator : MonoBehaviour
{
	public CameraController MainCameraPrefab;
	private PostProcessVolume ppv;

    void Awake() {
		Screen.orientation = ScreenOrientation.LandscapeLeft;

		if(CameraManager.Instance.CameraController == null) {
			CameraManager.Instance.Create(MainCameraPrefab, true);
		}
		GameManager gm = GameManager.Instance; // create the game and music managers as well
		MusicManager mm = MusicManager.Instance;

		ppv = GameObject.FindObjectsOfType<PostProcessVolume>().FirstOrDefault(p => 
			LayerMask.LayerToName(p.gameObject.layer) == "PostProcessing" 
			&& p.gameObject.scene.name == this.gameObject.scene.name);
		if(ppv != null) {
			PostProcessInfo info = new PostProcessInfo(ppv);
			if(CameraManager.Instance.CameraController.PostProcessInfo == null
				|| info.Type != CameraManager.Instance.CameraController.PostProcessInfo.Type) {
				CameraManager.Instance.CameraController.RegisterPostProcessVolume(info);
			}
			else {
				var objectToRemove = info.PostProcessObjectsContainer?.gameObject ?? info.Volume.gameObject;
				objectToRemove.Destroy();
			}
		}
    }
}
