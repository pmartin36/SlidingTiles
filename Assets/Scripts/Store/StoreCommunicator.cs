using System.Collections.Generic;
using UnityEngine;

public abstract class StoreCommunicator {
	public bool HasData { get; protected set; }
	public bool IsInitialized { get; protected set; }

	public static StoreCommunicator StoreCommunicatorFactory() {
		switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.LinuxEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:	
			case RuntimePlatform.LinuxPlayer:		
				return new TestCommunicator();
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

	public abstract void DisplayAchievementUI();
	public abstract bool AddAchievement(string name);
	public abstract bool GetLeaderboard(string leaderboardID, bool userHasScore, System.Action<IEnumerable<LeaderboardEntry>> onComplete); 

	public abstract void AddToLeaderboard(string leaderboardID, float score, System.Action<bool> onComplete);
	public abstract bool AddPurchase(string productID);

	public abstract void GoToStore();
	public abstract void SignIn(System.Action<bool> onComplete = null);
	public abstract void SignOut(System.Action<bool> onComplete = null);
}
