using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TestCommunicator : StoreCommunicator {
	public override void AddAchievement(string name) {
		throw new NotImplementedException();
	}

	public override void AddPurchase(string purchaseType) {
		Debug.Log(purchaseType);
	}

	public override void AddSaveData(string json) {
		Debug.Log(json);
	}

	public override bool TryLoadSaveData(out string jsonString) {
		jsonString = "{ test: {} }";
		Debug.Log("Trying to load saved data");
		return false;
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
