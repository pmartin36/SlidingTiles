using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuCopyManager : MonoBehaviour {
	private int layerMask;
	private List<KeyedMenuCopyComponent> keyedCopyComponents;

	public void Start() {
		layerMask = LayerMask.NameToLayer("Menu UI Level");

		keyedCopyComponents = new List<KeyedMenuCopyComponent>();	
		var original = GameObject.Find("Home Screen");
		var copy = Instantiate(original, original.transform.position, original.transform.rotation, this.transform);
		copy.name = "Copy";
		CopyComponent(original.transform, copy.transform);
		OnLevelChange(GameManager.Instance.LastPlayedWorld);
	}

	public void CopyComponent(Transform original, Transform copy) {
		MenuCopyComponent originalComponent = original.GetComponent<MenuCopyComponent>();
		if(originalComponent != null) {
			MenuCopyComponent copiedComponent = copy.GetComponent<MenuCopyComponent>();

			originalComponent.InitCopy(copiedComponent, false);
			copiedComponent.InitCopy(originalComponent, true);
			if(originalComponent is KeyedMenuCopyComponent) {
				keyedCopyComponents.Add(copiedComponent as KeyedMenuCopyComponent);
			}
		}
		copy.gameObject.layer = layerMask;

		for (int i = 0; i < original.childCount; i++) {
			var originalChild = original.GetChild(i);
			var copyChild = copy.GetChild(i);
			CopyComponent(originalChild, copyChild);
		}
	}

	public void OnLevelChange(int world) {
		foreach(var c in keyedCopyComponents) {
			c.OnWorldChange(world);
		}
	}
}
