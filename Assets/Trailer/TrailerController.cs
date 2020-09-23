using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerController : MonoBehaviour
{
    void Start() {
		StartCoroutine(ShrinkCamera());
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Return)) {
			GameManager.Instance.LevelManager.PlayPauseButtonClicked();
		}
		else if(Input.GetKeyDown(KeyCode.Space)) {
			if(Time.timeScale > 0.9f) {
				GameManager.Instance.SetTimescale(0.4f);
			}
			else {
				GameManager.Instance.SetTimescale(1f);
			}
		}
    }

	IEnumerator ShrinkCamera() {
		yield return new WaitForEndOfFrame();
		CameraManager.Instance.Camera.orthographicSize = 25f;

	}
}
