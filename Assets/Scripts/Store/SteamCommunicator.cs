using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SteamCommunicator : StoreCommunicator {
	public override bool AddAchievement(string name) {
		return true;
	}

	public override bool AddPurchase(string purchaseType) {
		return true;
	}

	public override void AddSaveData(string json) {
		
	}

	public override bool TryLoadSaveData(Action<string> callback) {
		callback("");
		return false;
	}

	public override void AddToLeaderboard(float score, string leaderboardID) {
		throw new NotImplementedException();
	}

	public override bool GetLeaderboard(string leaderboardID, bool userHasScore, Action<IEnumerable<LeaderboardEntry>> onComplete) {
		return true;
	}

	public override void GoToStore() {
		throw new NotImplementedException();
	}
}
