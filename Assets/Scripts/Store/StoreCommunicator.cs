using UnityEngine;

public abstract class StoreCommunicator {
	public bool HasData { get; protected set; }

	public static StoreCommunicator StoreCommunicatorFactory() {
		switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.LinuxEditor:
				return new TestCommunicator();
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:	
			case RuntimePlatform.LinuxPlayer:		
				return new SteamCommunicator();
			case RuntimePlatform.IPhonePlayer:
				return new AppleCommunicator();
			case RuntimePlatform.Android:
				return new AndroidCommunicator();
			/*
			case RuntimePlatform.PS4:
				break;
			case RuntimePlatform.XboxOne:
				break;
			case RuntimePlatform.Switch:
				break;
			*/
			default:
				throw new System.Exception("Invalid Platform");
		}
	}

	public abstract void AddAchievement(string name);
	public abstract void AddSaveData(string json);
	public abstract bool TryLoadSaveData(out string jsonString);

	public abstract void AddToLeaderboard(string score, int leaderboardType);
	public abstract void AddPurchase(string purchaseType);

	// these will actually return something at some point
	public abstract void GetLeaderboard(int leaderboardType); 
	public abstract void GetPurchases();
}
