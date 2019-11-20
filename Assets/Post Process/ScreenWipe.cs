using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ScreenWipeRenderer), PostProcessEvent.AfterStack, "Custom/Screen Wipe", allowInSceneView: false)]
public sealed class ScreenWipe : PostProcessEffectSettings {
	[Range(0f, 1f)]
	public FloatParameter blend = new FloatParameter { value = 0.5f };
}

public sealed class ScreenWipeRenderer : PostProcessEffectRenderer<ScreenWipe> {
	Shader shader;

	public override void Init() {
		base.Init();
		shader = Shader.Find("SlidingTiles/ScreenWipe");
	}

	public override void Render(PostProcessRenderContext context) {
		var sheet = context.propertySheets.Get(shader);
		sheet.properties.SetFloat("_Percent", settings.blend);
		context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
	}
}