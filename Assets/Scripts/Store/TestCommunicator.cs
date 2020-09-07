using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TestCommunicator : StoreCommunicator {
	public TestCommunicator() {
		IsInitialized = true;
	}

	public override bool AddAchievement(string name) {
		Debug.Log($"Achievement {name} added");
		return true;
	}

	public override bool AddPurchase(string purchaseType) {
		Debug.Log(purchaseType);
		return true;
	}

	public override void AddSaveData(string json) {
		// Debug.Log(json);
	}

	public override bool TryLoadSaveData(Action<string> callback) {
		callback("");
		return false;
	}

	public override void AddToLeaderboard(string leaderboardID, float score, Action<bool> onComplete) {
		Debug.Log($"Score of {score} added to {leaderboardID}");
		onComplete.Invoke(true);
	}

	public override bool GetLeaderboard(string leaderboardID, bool userHasScore, Action<IEnumerable<LeaderboardEntry>> onComplete) {
		List<LeaderboardEntry> lbs = new List<LeaderboardEntry>() {
			new LeaderboardEntry() { UserName = "Me", IsUser = true, Score = 12000, Rank = 3 },
			new LeaderboardEntry() { UserName = "2", IsUser = false, Score = 11000, Rank = 2 },
			new LeaderboardEntry() { UserName = "1", IsUser = false, Score = 7000, Rank = 1 },
			new LeaderboardEntry() { UserName = "4", IsUser = false, Score = 15000, Rank = 4 },
		};
		onComplete(lbs);
		return true;
	}

	public override void GoToStore() {
		Debug.Log("Go to store");
	}

	public override void SignIn(Action<bool> onComplete = null) {
		onComplete.Invoke(true);
	}

	public override void SignOut(Action<bool> onComplete = null) {
		onComplete.Invoke(true);
	}

	public override void DisplayAchievementUI() {
		Debug.Log("Display Achievements");
	}
}
