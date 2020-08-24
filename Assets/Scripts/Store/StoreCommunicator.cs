using System.Collections.Generic;
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

	public abstract void AddSaveData(string json);
	public abstract bool TryLoadSaveData(System.Action<string> callback);

	public abstract bool AddAchievement(string name);
	public abstract bool GetLeaderboard(string leaderboardID, bool userHasScore, System.Action<IEnumerable<LeaderboardEntry>> onComplete); 

	public abstract void AddToLeaderboard(float score, string leaderboardID);
	public abstract bool AddPurchase(string productID);

	public abstract void GoToStore();
}
