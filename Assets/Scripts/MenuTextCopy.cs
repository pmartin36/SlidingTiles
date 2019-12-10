using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuTextCopy : KeyedMenuCopyComponent {
	private TMP_Text text;

	public override void Start() {
		text = GetComponent<TMP_Text>();
		if(!IsCopy) {
			text.fontSharedMaterial = Resources.Load<Material>($"Materials/World 1/{Key}");
			text.font = Resources.Load<TMP_FontAsset>($"Fonts/World 1");
		}
		base.Start();
	}

	public override void SetMaterial(Material m, int world) {
		text.fontSharedMaterial = m;
		text.font = Resources.Load<TMP_FontAsset>($"Fonts/World {world}");
	}
}
