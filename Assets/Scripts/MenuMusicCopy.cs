using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MenuMusicCopy : KeyedMenuCopyComponent {
	public override void Start() {
		if (!IsCopy) {
			Addressables.LoadAssetAsync<CopyObject>($"World1/{Key.ToString()}").Completed +=
				(obj) => {
					SetPropertiesFromObject(obj.Result, 1);
					Loaded = true;
				};
		}
		else {
			Loaded = true;
		}
	}

	public override void SetPropertiesFromObject(ScriptableObject m, int world) {
		CopyMusicObject cmo = m as CopyMusicObject;
		if (cmo != null) {
			MusicManager.Instance.ChangeTrack(cmo.Track, LevelSelect.CameraWipeTime);
		}
	}
}
