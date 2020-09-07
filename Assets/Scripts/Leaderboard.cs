using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
	private const float baseWidth = 700;
	private const float baseHeight = 600;
	private const float baseRowHeight = 85;
	private float rowHeight;
	private float textRatio;

	public LeaderboardRow LeaderboardRowPrefab;
	public Material userScoreMaterial;
	public Material nonUserScoreMaterial;

	public RectTransform AnyStarLeaderboard;
	public RectTransform ThreeStarLeaderboard;

    void Start() {
		StartCoroutine(InitRoutine());
		Close();
    }

	public void Open() {
		transform.GetChild(0).gameObject.SetActive(true);
	}

	public void Close() {
		transform.GetChild(0).gameObject.SetActive(false);
	}

	public void SetScores(bool isAnyStarScore, IEnumerable<LeaderboardEntry> scores) {
		Transform parent = isAnyStarScore ? AnyStarLeaderboard : ThreeStarLeaderboard;
		var scoresArray = scores.ToArray();
		Debug.Log($"Setting {scoresArray.Length} scores for {(isAnyStarScore ? "Any Star" : "Three Star")} scoreboard");
		StartCoroutine(SetScoresRoutine(parent, scoresArray));
	}

	private IEnumerator InitRoutine() {
		LeaderboardRow basisRow = AnyStarLeaderboard.GetComponentInChildren<LeaderboardRow>();
		yield return new WaitForEndOfFrame();
		Vector2 leaderboardSize = AnyStarLeaderboard.rect.size;
		float heightRatio = leaderboardSize.y / baseHeight;
		float widthRatio = leaderboardSize.x / baseWidth;
		textRatio = Mathf.Min(heightRatio, widthRatio);
		rowHeight = baseRowHeight * heightRatio;

		RectTransform rt = basisRow.GetComponent<RectTransform>();
		var rts = rt.sizeDelta;
		rts.y = rowHeight;
		rt.sizeDelta = rts;
		basisRow.SetSize(textRatio);
	}

	private IEnumerator SetScoresRoutine(Transform parent, LeaderboardEntry[] scores) {
		yield return new WaitUntil(() => rowHeight > 1);

		LeaderboardRow basisRow = AnyStarLeaderboard.GetComponentInChildren<LeaderboardRow>();
		LeaderboardRow row = basisRow;
		for (int i = 0; i < scores.Length; i++) {
			if(!(parent == AnyStarLeaderboard && i == 0)) {
				row = Instantiate(basisRow, parent);
			}
			RectTransform rt = row.GetComponent<RectTransform>();

			LeaderboardEntry s = scores[i];
			var rtp = rt.anchoredPosition;
			rtp.y = -50 - rowHeight * i;
			rt.anchoredPosition = rtp;

			row.SetRow(s.Rank, s.UserName, Utils.SplitTime(s.Score / 1000f, MillisecondDisplay.Normal), s.IsUser ? userScoreMaterial : nonUserScoreMaterial);
		}
	}
}
