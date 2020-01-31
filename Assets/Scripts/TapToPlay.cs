using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TapToPlay : MonoBehaviour
{
	private TMP_Text text;
	public float Power;

    void Start() {
		text = GetComponent<TMP_Text>();
    }

    void Update() {
		text.color = new Color(1, 1, 1, Power + 0.1f);
		text.fontMaterial.SetFloat("_GlowOuter", Power - 0.3f);
    }
}
