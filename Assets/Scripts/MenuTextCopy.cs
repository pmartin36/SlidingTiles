using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuTextCopy : KeyedMenuCopyComponent {
	private TMP_Text text;

	public override void Start() {
		text = GetComponent<TMP_Text>();
		if(!IsCopy) {
			text.fontSharedMaterial = Resources.Load<Material>($"Materials/World1_{Key}");
		}
		base.Start();
	}

	public override void SetMaterial(Material m) {
		text.fontSharedMaterial = m;
	}
}
