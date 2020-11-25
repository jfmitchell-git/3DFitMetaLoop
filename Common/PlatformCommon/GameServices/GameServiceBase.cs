#if !BACKOFFICE
using MetaLoop.Common.PlatformCommon.UserProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class GameServiceBase : IGameService
    {
        public const string GuestName = "Guest";
        public delegate void GameServiceEventHandler(GameServiceEvent e);
        public event GameServiceEventHandler OnGameServiceEvent;

        public virtual void Init()
        {

            RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.SignInSuccess));
            RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.AchievementsReady));
            GameServiceManager.IsInit = true;
        }

        public void RaiseGameServiceEvent(GameServiceEvent e)
        {
            if (OnGameServiceEvent != null)
            {
                OnGameServiceEvent(e);
            }
        }

        public virtual void GetAvatarForPlayerId(GameServiceUserInfo user)
        {

        }

        public virtual void LoadFriendsList()
        {

        }

        private string GetDefaultProfileName()
        {
            string playerName = string.Empty;

#if UNITY_EDITOR || UNITY_STANDALONE
            playerName = Environment.UserName + "/" + Environment.MachineName;
#endif

            if (playerName == string.Empty)
            {
                playerName = GuestName + "#" + UserProfileManager.Instance.UserProfileData.Id.ToString().Substring(0, 4);
            }

            return playerName;
        }

        public virtual string PlayerName
        {
            get
            {
                return GetDefaultProfileName();
            }
        }

        public virtual bool IsSignedIn
        {
            get
            {
                return true;
            }
        }

        public virtual string PlayerId
        {
            get
            {
                return UserProfileManager.Instance.UserProfileData.Id.ToString();
            }
        }

        public virtual List<GameServiceUserInfo> GameServiceFriendsList
        {
            get
            {
                return null;
            }
        }

        public virtual bool IsAchivementCompleted(string achivementId)
        {
            return false;
        }

        public virtual void ReportAchivement(string achievementId, int completion = 100)
        {

        }

        public virtual void SubmitScore(string leaderbordId, int score, bool forceUpdate = false)
        {

        }

        public virtual int GetScore(string leaderbordId)
        {
            return 0;
        }

        public virtual void ShowLeaderbord(string leaderbordId)
        {

        }

        public virtual void InitLeaderboard(string leaderboardId)
        {

        }



    }
}
#endif
