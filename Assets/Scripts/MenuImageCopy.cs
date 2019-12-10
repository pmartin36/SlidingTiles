using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuImageCopy : KeyedMenuCopyComponent {
	private Image image;

	public override void Start() {
		image = GetComponent<Image>();
		base.Start();
	}

	public override void SetMaterial(Material m, int world) {
		image.material = m;
	}
}
