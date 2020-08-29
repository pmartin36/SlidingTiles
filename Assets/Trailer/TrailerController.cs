using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerController : MonoBehaviour
{
    void Start() {
		
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Return)) {
			GameManager.Instance.LevelManager.PlayPauseButtonClicked();
		}
    }
}
