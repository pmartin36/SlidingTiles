using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

public abstract class KeyedMenuCopyComponent : MenuCopyComponent, IRequireResources {
	public CopyKey Key;
	private static Dictionary<CopyKey, LevelMaterial> LoadedLevelForKey;
	public bool Loaded { get; set; } = false;

	public virtual void Start() {
		if(!IsCopy) {
			Loaded = true;
		}
	}

	public void OnWorldChange(int world) {
		if (LoadedLevelForKey == null) {
			LoadedLevelForKey = new Dictionary<CopyKey, LevelMaterial>();
		}
		bool success = LoadedLevelForKey.TryGetValue(Key, out LevelMaterial lm);
		if(!success) {
			lm = new LevelMaterial(0, null);
			LoadedLevelForKey.Add(Key, lm);
		}

		if(world > 0) {
			if (lm.World != world) {
				lm.World = world;
				Addressables.LoadAssetAsync<CopyObject>($"World{world}/{Key.ToString()}").Completed +=
					(obj) =>  {
						SetMaterial(obj.Result, world);
						lm.CopyObject = obj.Result;
						Loaded = true;
					};
			}
			else {
				SetMaterial(lm.CopyObject, world);
			}
		}
	}
	public abstract void SetMaterial(CopyObject m, int world);
}

public struct LevelMaterial {
	public int World { get; set; }
	public CopyObject CopyObject { get; set; }

	public LevelMaterial(int world, CopyObject co) : this() {
		World = world;
		CopyObject = co;
	}
}

public enum CopyKey {
	Back,
	Background,
	Grid,
	Tile,
	TileText
}

