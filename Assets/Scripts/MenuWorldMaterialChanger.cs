using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuWorldMaterialChanger : MonoBehaviour
{
    public Material Material {
		get {
			return image?.material ?? text.material;
		}
	}
	public Color Color {
		get
		{
			return image?.color ?? text.color;
		}
		set {
			if(image != null) {
				image.color = value;
			}
			else {
				text.color = value;
			}
		}
	}
	private Image image;
	private TMP_Text text;

	public void Init(Image i, TMP_Text _text) {
		image = i;
		text = _text;
	}

	public void SetMaterialForWorld(int world) {

	}
}
