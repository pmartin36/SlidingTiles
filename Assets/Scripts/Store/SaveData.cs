using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class SaveData {
	public DateTime SaveTime;
	public int HighestUnlockedLevel;
	public int HighestOwnedWorld;
	public int LastPlayedWorld;
	public bool AdsRemoved;
	public float MusicVolume;
	public float FxVolume;
	public bool ShowTimer;
	public bool HasComparedWithFriends;
	public bool HasClickedToRate;
	public LevelData[,] LevelData;

	public SaveData(int highestUnlockedLevel, int highestOwnedWorld, int lastPlayedWorld, bool adsRemoved, float musicVolume, float fxVolume, bool showms, LevelData[,] ld, bool hasComparedWithFriends, bool hasClickedToRate) {
		HighestUnlockedLevel = highestUnlockedLevel;
		HighestOwnedWorld = highestOwnedWorld;
		LastPlayedWorld = lastPlayedWorld;
		AdsRemoved = adsRemoved;
		MusicVolume = musicVolume;
		FxVolume = fxVolume;
		ShowTimer = showms;
		HasClickedToRate = hasClickedToRate;

		// 12 worlds, 10 levels per world
		const int worlds = 12;
		const int levels = 10;
		LevelData = new LevelData[worlds, levels];
		if(ld != null) {
			for(int i = 0; i < worlds; i++) {
				for(int j = 0; j < levels; j++) {
					LevelData[i,j] = ld[i,j];
				}
			}
		}
		else {
			for (int i = 0; i < worlds; i++) {
				for (int j = 0; j < levels; j++) {
					LevelData[i, j] = new LevelData();
				}
			}
		}

		SaveTime = DateTime.UtcNow;
	}

	public SaveData() : this(SceneHelpers.TutorialLevelStart, 4, 0, false, 1f, 1f, false, null, false, false) {}
	// public SaveData() : this(40, 2, 3, false, 1f, 1f, true, null, false, false) { } // only for testing
}

[Serializable]
public class LevelData {
	public int MaxStarsCollected;
	public float AnyStarCompletionTime;
	public float ThreeStarCompletionTime;

	public LevelData(int stars = -1, float anyTime = -1, float threeTime = -1) {
		MaxStarsCollected = stars;
		AnyStarCompletionTime = anyTime;
		ThreeStarCompletionTime = threeTime;
	}
}

