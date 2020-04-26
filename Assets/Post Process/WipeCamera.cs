using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WipeCamera : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	private Camera camera;

    void Start() {
		GenerateRT();
    }

	void GenerateRT() {
		var camera = GetComponent<Camera>();
		camera.SetReplacementShader(Shader.Find("SlidingTiles/MenuLevelReplacement"), "Replace");

		if (camera.targetTexture != null) {
			RenderTexture temp = camera.targetTexture;
			camera.targetTexture = null;
			DestroyImmediate(temp);
		}

		camera.targetTexture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 16);
		camera.targetTexture.filterMode = FilterMode.Bilinear;

		Shader.SetGlobalTexture("_SecondLevelTexture", camera.targetTexture);
	}

    void Update() {
        
    }
}
