using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuImageSpriteCopy : KeyedMenuCopyComponent {
	private Image image;

	public override void Start() {
		base.Start();
	}

	public override void SetPropertiesFromObject(ScriptableObject m, int world) {
		if(image == null) {
			image = GetComponent<Image>();
		}

		CopyImageObject copyObject = m as CopyImageObject;
		image.material = copyObject.Material;
		if (copyObject != null) {
			image.sprite = copyObject.Sprite;
			image.color = copyObject.Color;
		}
	}
}
