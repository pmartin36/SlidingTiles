using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;

public class MenuTextCopy : KeyedMenuCopyComponent {
	private TMP_Text text;

	public override void Start() {
		if (!IsCopy) {
			Addressables.LoadAssetAsync<CopyObject>($"World1/{Key.ToString()}").Completed +=
				(obj) => {
					SetMaterial(obj.Result, 1);
					Loaded = true;
				};
		}
		else {
			Loaded = true;
		}
	}

	public override void SetMaterial(CopyObject m, int world) {
		if(text == null) {
			text = GetComponent<TMP_Text>();
		}
		CopyTextObject cto = m as CopyTextObject;
		if(cto != null) {
			text.fontSharedMaterial = m.Material;
			text.font = cto.Font;
			text.fontStyle = cto.FontStyles;
			text.fontSize = cto.FontSize;
		}
	}
}
