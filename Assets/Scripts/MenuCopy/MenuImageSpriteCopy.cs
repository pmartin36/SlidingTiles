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

		if(m is CopyImageObject) {
			CopyImageObject copyObject = m as CopyImageObject;
			image.material = copyObject.Material;
			image.color = copyObject.Color;

			if(copyObject.Sprite)
				image.sprite = copyObject.Sprite;
		}
		else if(m is CopyObject) {
			image.material = (m as CopyObject).Material;
			image.color = Color.white;
		}
	}
}
