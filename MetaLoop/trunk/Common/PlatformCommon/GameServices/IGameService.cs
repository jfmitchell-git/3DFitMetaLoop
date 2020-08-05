#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public interface IGameService
    {
        void Init();
        bool IsSignedIn { get; }
        string PlayerName { get; }
        string PlayerId { get; }
        bool IsAchivementCompleted(string achivementId);
        void ReportAchivement(string achievementId, int completion = 100);
        void SubmitScore(string leaderbordId, int score, bool forceUpdate = false);
        int GetScore(string leaderbordId);
        void ShowLeaderbord(string leaderbordId);
        void InitLeaderboard(string leaderbordId);
        List<GameServiceUserInfo> GameServiceFriendsList { get; }

    }
}
#endif