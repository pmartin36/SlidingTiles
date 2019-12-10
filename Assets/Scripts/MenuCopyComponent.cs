using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class MenuCopyComponent : MonoBehaviour
{
	public bool IsCopy { get; set; }
	public MenuCopyComponent MirroredComponent { get; set; }
	public virtual void InitCopy(MenuCopyComponent component, bool isCopy) {
		MirroredComponent = component;
		IsCopy = isCopy;
	}
}

public abstract class KeyedMenuCopyComponent : MenuCopyComponent {
	public string Key;
	private static Dictionary<string, LevelMaterial> LoadedLevelForKey;

	public virtual void Start() {
		if (LoadedLevelForKey == null) {
			LoadedLevelForKey = new Dictionary<string, LevelMaterial>();
		}

		OnWorldChange(GameManager.Instance.LastPlayedWorld);
	}

	public void OnWorldChange(int world) {
		if (IsCopy) {
			bool success = LoadedLevelForKey.TryGetValue(Key, out LevelMaterial lm);
			if(!success) {
				lm = new LevelMaterial(0, null);
				LoadedLevelForKey.Add(Key, lm);
			}

			if(world > 0) {
				if (lm.World != world) {
					lm.World = world;
					lm.Material = Resources.Load<Material>($"Materials/World {world}/{Key}");
				}
				SetMaterial(lm.Material, world);
			}
		}
	}
	public abstract void SetMaterial(Material m, int world);
}

public struct LevelMaterial {
	public int World { get; set; }
	public Material Material { get; set; }

	public LevelMaterial(int world, Material material) : this() {
		World = world;
		Material = material;
	}
}

