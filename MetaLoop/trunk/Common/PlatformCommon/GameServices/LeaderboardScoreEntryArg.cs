using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class LeaderboardScoreEntryArg
    {
        public string LeaderboardId { get; set; }
        public int Rank { get; set; }
        public int Score { get; set; }

        public LeaderboardScoreEntryArg(string leaderboardId, int rank, int score)
        {
            this.LeaderboardId = leaderboardId;
            this.Rank = rank;
            this.Score = score;
        }
    }
}
