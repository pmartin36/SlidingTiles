using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BlurCamera : MonoBehaviour
{
	private static RenderTexture Blurred;

	void OnEnable() {
		Blurred = new RenderTexture(Screen.width >> 1, Screen.height >> 1, 0);
		Camera camera = GetComponent<Camera>();
		camera.targetTexture = Blurred;
		Shader.SetGlobalTexture("_GlowBlurredTex", Blurred);
	}
}
