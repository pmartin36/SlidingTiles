using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class SaveData {
	public int HighestUnlockedLevel;
	public int HighestOwnedWorld;
	public int LastPlayedWorld;
	public bool AdsRemoved;
	public float MusicVolume;
	public float FxVolume;
	public bool ShowMilliseconds;

	public SaveData(int highestUnlockedLevel, int highestOwnedWorld, int lastPlayedWorld, bool adsRemoved, float musicVolume, float fxVolume, bool showms) {
		HighestUnlockedLevel = highestUnlockedLevel;
		HighestOwnedWorld = highestOwnedWorld;
		LastPlayedWorld = lastPlayedWorld;
		AdsRemoved = adsRemoved;
		MusicVolume = musicVolume;
		FxVolume = fxVolume;
		ShowMilliseconds = showms;
	}

	//public SaveData() : this(2, 5, 0, false, 1f, 1f, false) {}
	public SaveData() : this(15, 2, 2, false, 1f, 1f, true) { } // only for testing
}

