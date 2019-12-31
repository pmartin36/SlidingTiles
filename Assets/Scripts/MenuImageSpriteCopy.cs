using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuImageSpriteCopy : KeyedMenuCopyComponent {
	private Image image;

	public override void Start() {
		base.Start();
	}

	public override void SetMaterial(CopyObject m, int world) {
		if(image == null) {
			image = GetComponent<Image>();
		}
		image.material = m.Material;

		CopyImageObject copyObject = m as CopyImageObject;
		if (copyObject != null) {
			image.sprite = copyObject.Sprite;
			image.color = copyObject.Color;
		}
	}
}
