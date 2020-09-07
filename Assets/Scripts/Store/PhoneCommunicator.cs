using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoxelBusters.NativePlugins;

public abstract class PhoneCommunicator : StoreCommunicator {
	public PhoneCommunicator() {
		this.SignIn(null);

		Billing.DidFinishProductPurchaseEvent += OnDidFinishTransaction;
		CloudServices.KeyValueStoreDidInitialiseEvent += OnCloudInit;

		NPBinding.CloudServices.Initialise();

		NPBinding.Billing.RequestForBillingProducts(NPSettings.Billing.Products);
	}

	private void OnCloudInit(bool success) {
		IsInitialized = true;
	}

	public override void SignIn(Action<bool> onComplete = null) {
		bool isAuthenticated = NPBinding.GameServices.LocalUser.IsAuthenticated;
		if(isAuthenticated) {
			onComplete?.Invoke(true);
		}
		else {
			NPBinding.GameServices.LocalUser.Authenticate((success, errorMsg) => onComplete?.Invoke(success));
		}
	}

	public override void SignOut(Action<bool> onComplete = null) {
		bool isAuthenticated = NPBinding.GameServices.LocalUser.IsAuthenticated;
		if (!isAuthenticated) {
			onComplete?.Invoke(true);
		}
		else {
			NPBinding.GameServices.LocalUser.SignOut((success, errorMsg) => onComplete?.Invoke(success));
		}
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
			NPBinding.GameServices.ReportProgressWithGlobalID(name, 100d, (success, errorMsg) => {});
			return true;
		}
		return false;
	}

	public override void AddToLeaderboard(string leaderboardID, float score, Action<bool> onComplete) {
		if (NPBinding.GameServices.IsAvailable() && NPBinding.GameServices.LocalUser.IsAuthenticated) {
			long longScore = Mathf.FloorToInt(score * 1000);
			NPBinding.GameServices.ReportScoreWithGlobalID(leaderboardID, longScore, (success, error) => {
				onComplete(success);
				Debug.Log($"Set leaderboard: {leaderboardID}, with score: {longScore}, successful: {success}, with errormsg: {error}");
			});
		}
	}

	private void GetLeaderboardData(string leaderboardID, bool userHasScore, Action<IEnumerable<LeaderboardEntry>> onComplete) {
		VoxelBusters.NativePlugins.Leaderboard lb = NPBinding.GameServices.CreateLeaderboardWithGlobalID(leaderboardID);
		lb.MaxResults = 7;
		lb.UserScope = eLeaderboardUserScope.FRIENDS_ONLY;
		if (userHasScore) {
			lb.LoadPlayerCenteredScores((Score[] _scores, Score _localUserScore, string _error) => {
				Debug.Log($"Got leaderboard: {leaderboardID}, with errormsg: {_error}");
				Debug.Log($"leaderboard me: {_localUserScore.Value}");
				Debug.Log($"leaderboard others: {_scores.Length}");
				var scores = _scores.Select(s => new LeaderboardEntry(s, false));
				if(_localUserScore.Rank > 0) {
					scores = scores.Append(new LeaderboardEntry(_localUserScore, true)).OrderByDescending(s => s.Rank);
				}
				onComplete(scores);
			});
		}
		else {
			lb.LoadTopScores((Score[] _scores, Score _localUserScore, string _error) => {
				onComplete(_scores.Select(s => new LeaderboardEntry(s, false)));
			});
		}
	}

	public override bool GetLeaderboard(string leaderboardID, bool userHasScore, Action<IEnumerable<LeaderboardEntry>> onComplete) {
		if (NPBinding.GameServices.IsAvailable()) {
			bool isAuthenticated = NPBinding.GameServices.LocalUser.IsAuthenticated;
			if (!isAuthenticated) {
				NPBinding.GameServices.LocalUser.Authenticate((success, errorMsg) => {
					if (success) {
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

	public override void DisplayAchievementUI() {
		if (NPBinding.GameServices.IsAvailable()) {
			NPBinding.GameServices.ShowAchievementsUI(error => {});
		}
	}
}

