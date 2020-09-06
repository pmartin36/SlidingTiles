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

		int lastPlayedWorld = Mathf.Max(1, GameManager.Instance.LastPlayedWorld);
		OnLevelChange(lastPlayedWorld, true);
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

	public void OnLevelChange(int world, bool initial = false) {
		MusicManager.Instance.LoadMusicForWorldAndChangeTrack(world, 1f, 0.8f, initial);
		foreach(var c in keyedCopyComponents) {
			c.OnWorldChange(world);
		}
	}
}
