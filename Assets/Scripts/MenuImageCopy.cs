using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuImageCopy : KeyedMenuCopyComponent {
	private Image image;

	public override void Start() {
		base.Start();
	}

	public override void SetPropertiesFromObject(ScriptableObject m, int world) {
		if(image == null) {
			image = GetComponent<Image>();
		}
		image.material = (m as CopyObject).Material;
	}
}
