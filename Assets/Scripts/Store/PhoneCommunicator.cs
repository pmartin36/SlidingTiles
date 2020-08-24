using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoxelBusters.NativePlugins;

public abstract class PhoneCommunicator : StoreCommunicator {
	public PhoneCommunicator() {
		Billing.DidFinishProductPurchaseEvent += OnDidFinishTransaction;

		bool isAuthenticated = NPBinding.GameServices.LocalUser.IsAuthenticated;
		if(!isAuthenticated) {
			NPBinding.GameServices.LocalUser.Authenticate((success, errorMsg) => { });
		}

		NPBinding.CloudServices.Initialise();
	}

	public override bool AddPurchase(string productID) {
		if (NPBinding.Billing.IsAvailable()) {
			BillingProduct bp = NPBinding.Billing.GetStoreProduct(productID);
			if (!NPBinding.Billing.IsProductPurchased(bp)) {
				NPBinding.Billing.BuyProduct(bp); // will fire OnDidFinishTransaction Event on complete
			}
			else if (productID == "removeads") {
				GameManager.Instance.RemoveAds();
			}
			return true;
		}
		return false;
	}

	private void OnDidFinishTransaction(BillingTransaction _transaction) {
		if (_transaction != null) {
			if (_transaction.VerificationState == eBillingTransactionVerificationState.SUCCESS) {
				if (_transaction.TransactionState == eBillingTransactionState.PURCHASED) {
					if (_transaction.ProductIdentifier == "removeads") {
						GameManager.Instance.RemoveAds();
					}
				}
			}
		}
	}

	public override void AddSaveData(string json) {
		if(NPBinding.CloudServices.IsInitialised()) {
			NPBinding.CloudServices.SetString("userSettings", json);
		}
	}

	public override bool TryLoadSaveData(Action<string> callback) {
		if(NPBinding.CloudServices.IsInitialised()) {
			callback(NPBinding.CloudServices.GetString("userSettings"));
			return true;
		}
		return false;
	}

	public override bool AddAchievement(string name) {
		if (NPBinding.GameServices.IsAvailable() && NPBinding.GameServices.LocalUser.IsAuthenticated) {
			NPBinding.GameServices.ReportProgressWithGlobalID(name, 1d, (success, errorMsg) => { });
			return true;
		}
		return false;
	}

	public override void AddToLeaderboard(float score, string leaderboardID) {
		if (NPBinding.GameServices.IsAvailable() && NPBinding.GameServices.LocalUser.IsAuthenticated) {
			long longScore = Mathf.FloorToInt(score * 1000);
			NPBinding.GameServices.ReportScoreWithGlobalID(leaderboardID, longScore, null);
		}
	}

	private void GetLeaderboardData(string leaderboardID, bool userHasScore, Action<IEnumerable<LeaderboardEntry>> onComplete) {
		// whatever the score is, we need to divide by 1000 to separate out the milliseconds
		VoxelBusters.NativePlugins.Leaderboard lb = NPBinding.GameServices.CreateLeaderboardWithGlobalID(leaderboardID);
		lb.MaxResults = 10;
		lb.UserScope = eLeaderboardUserScope.FRIENDS_ONLY;
		if(userHasScore) {
			lb.LoadPlayerCenteredScores((Score[] _scores, Score _localUserScore, string _error) => {
				onComplete(_scores.Select(s => 
					new LeaderboardEntry(s) {
						IsUser = _localUserScore.User.Identifier == s.User.Identifier
					}
				));
			});
		}
		else {
			lb.LoadTopScores((Score[] _scores, Score _localUserScore, string _error) => {
				onComplete(_scores.Select(s => new LeaderboardEntry(s)));
			});
		}
	}

	public override bool GetLeaderboard(string leaderboardID, bool userHasScore, Action<IEnumerable<LeaderboardEntry>> onComplete) {
		if (NPBinding.GameServices.IsAvailable()) {
			bool isAuthenticated = NPBinding.GameServices.LocalUser.IsAuthenticated;
			if (!isAuthenticated) {
				NPBinding.GameServices.LocalUser.Authenticate((success, errorMsg) => {
					if(success) {
						GetLeaderboardData(leaderboardID, userHasScore, onComplete);
					}
				});
			}
			else {
				GetLeaderboardData(leaderboardID, userHasScore, onComplete);
				return true;
			}
		}
		return false;
	}
}

