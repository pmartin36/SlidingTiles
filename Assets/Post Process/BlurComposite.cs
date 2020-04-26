using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(BlurCompositeRenderer), PostProcessEvent.AfterStack, "Custom/BlurComposite", allowInSceneView: false)]
public sealed class BlurComposite : PostProcessEffectSettings {
	[Range(0,4)]
	public FloatParameter intensity = new FloatParameter { value = 0f };
	public FloatParameter blurSize = new FloatParameter { value = 1.5f };
	[Range(0, 6)]
	public IntParameter blurIterations = new IntParameter { value = 4 };
}

public sealed class BlurCompositeRenderer : PostProcessEffectRenderer<BlurComposite> {
	Shader shader;
	Shader blur;
	RenderTexture blurred;

	public override void Init() {
		base.Init();
		// attempt 1
		// shader = Shader.Find("Hidden/BlurComposite");

		// attempt 2
		blur = Shader.Find("Hidden/BlurPost");
		shader = Shader.Find("Hidden/BlurComposite");
		blurred = new RenderTexture(Screen.width >> 1, Screen.height >> 1, 0);

		settings.intensity.value = 0f;
	}

	public override void Render(PostProcessRenderContext context) {
		// attempt 1
		//var sheet = context.propertySheets.Get(shader);
		//context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

		// attempt 2
		var sheet = context.propertySheets.Get(shader);
		var blurSheet = context.propertySheets.Get(blur);
		float blurSize = settings.blurSize;
		blurSheet.properties.SetVector("_BlurSize", new Vector2(blurSize / blurred.width, blurSize / blurred.height));
		context.command.BlitFullscreenTriangle(Shader.GetGlobalTexture("_GlowBlurredTex"), blurred);
		for (int i = 0; i < settings.blurIterations; i++) {
			var temp = RenderTexture.GetTemporary(blurred.width, blurred.height);
			context.command.BlitFullscreenTriangle(blurred, temp, blurSheet, 0);
			context.command.BlitFullscreenTriangle(temp, blurred, blurSheet, 1);
			RenderTexture.ReleaseTemporary(temp);
		}
		sheet.properties.SetTexture("_GlowTex", blurred);
		sheet.properties.SetFloat("_Intensity", settings.intensity);

		context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
	}
}