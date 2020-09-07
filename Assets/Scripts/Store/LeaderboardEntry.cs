using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LeaderboardEntry {
	public float Score { get; set; }
	public int Rank { get; set; }
	public string UserName { get; set; }
	public bool IsUser { get; set; }

	public LeaderboardEntry() { }

	public LeaderboardEntry(VoxelBusters.NativePlugins.Score score, bool isUser) {
		Rank = score.Rank;
		Score = score.Value / 1000f;
		UserName = score.User.Alias.Length > 0 ? score.User.Alias : score.User.Name;
		IsUser = isUser;
	}
}

