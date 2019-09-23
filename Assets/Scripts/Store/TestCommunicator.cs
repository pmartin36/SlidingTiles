using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TestCommunicator : StoreCommunicator {
	public override void AddAchievement(string name) {
		throw new NotImplementedException();
	}

	public override void AddPurchase(string purchaseType) {
		throw new NotImplementedException();
	}

	public override void AddSaveData() {
		throw new NotImplementedException();
	}

	public override void AddToLeaderboard(string score, int leaderboardType) {
		throw new NotImplementedException();
	}

	public override void GetLeaderboard(int leaderboardType) {
		throw new NotImplementedException();
	}

	public override void GetPurchases() {
		throw new NotImplementedException();
	}
}
