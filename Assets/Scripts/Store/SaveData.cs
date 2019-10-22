using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class SaveData {
	public int HighestUnlockedLevel;
	public int HighestOwnedWorld;
	public bool AdsRemoved;
	public float MusicVolume;
	public float FxVolume;

	public SaveData(int highestUnlockedLevel, int highestOwnedWorld, bool adsRemoved, float musicVolume, float fxVolume) {
		HighestUnlockedLevel = highestUnlockedLevel;
		HighestOwnedWorld = highestOwnedWorld;
		AdsRemoved = adsRemoved;
		MusicVolume = musicVolume;
		FxVolume = fxVolume;
	}

	public SaveData() : this(2, 5, false, 1f, 1f) {}
}

