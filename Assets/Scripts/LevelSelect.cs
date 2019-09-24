using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelect : MonoBehaviour
{
	public bool LevelSelectOpen { get; set; }

    void Start() {
		LevelSelectOpen = false;
    }

    void Update() {
        
    }

	public void WorldSelected(GameObject o) {
		
		// object has world name
	}
}
