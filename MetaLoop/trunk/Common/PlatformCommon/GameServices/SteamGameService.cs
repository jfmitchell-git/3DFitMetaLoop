
#if STEAM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steamworks;
using System.Reflection.Emit;
using DG.Tweening;
using UnityEngine;
using dryginstudios.commonscripts.backoffice;

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class SteamGameService : GameServiceBase
    {

        private CGameID m_GameID;
        private Callback<UserStatsReceived_t> m_UserStatsReceived;
        private Callback<UserStatsStored_t> m_UserStatsStored;
        private Callback<UserAchievementStored_t> m_UserAchievementStored;
        //private CallResult<LeaderboardScoreUploaded_t> m_LeaderboardScoreUploaded;

        private Dictionary<string, SteamLeaderboard_t?> leaderbordsStatus = new Dictionary<string, SteamLeaderboard_t?>();
        private List<string> leaderboardGetScoreQueue = new List<string>();

        private List<GameServiceUserInfo> gameServiceFriendsList;
        public override List<GameServiceUserInfo> GameServiceFriendsList
        {
            get
            {
                return gameServiceFriendsList;
            }
        }

        public override bool IsSignedIn
        {
            get
            {
                try
                {
                    return SteamManager.Initialized;
                }
                catch
                {
                    return false;
                }

            }
        }

        public override string PlayerId
        {
            get
            {
                if (IsSignedIn)
                {
                    //Extremly unsure about this. Must be tested.
                    CSteamID steamId = SteamUser.GetSteamID();
                    return steamId.GetAccountID().ToString();

                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public override string PlayerName
        {
            get
            {
                string playerName = string.Empty;
                if (IsSignedIn) playerName = SteamFriends.GetPersonaName();
                return playerName;
            }
        }

        public SteamGameService()
        {


        }

        public override void Init()
        {
            if (IsSignedIn)
            {
                m_GameID = new CGameID(SteamUtils.GetAppID());
                m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
                m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
                m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
                //m_LeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);


                SteamUserStats.RequestCurrentStats();

                //SteamUser.
                RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.SignInSuccess));
                GetSmallAvatar(SteamUser.GetSteamID());

            }
            else
            {
                RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.SingInFailed));
            }
        }



        public override void LoadFriendsList()
        {
            gameServiceFriendsList = new List<GameServiceUserInfo>();

            int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

            for (int i = 0; i < friendCount; ++i)
            {
                CSteamID friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                string friendName = SteamFriends.GetFriendPersonaName(friendSteamId);
                EPersonaState friendState = SteamFriends.GetFriendPersonaState(friendSteamId);
                FriendGameInfo_t currentGame;


                if (SteamFriends.GetFriendGamePlayed(friendSteamId, out currentGame))
                {
                    if (currentGame.m_gameID == m_GameID)
                    {
                        GameServiceUserInfo friend = new GameServiceUserInfo();
                        friend.NickName = friendName;
                        friend.PlayerId = friendSteamId.GetAccountID().m_AccountID.ToString();
                        friend.Status = Status.Available;
                        friend.SetAvatarFromTexture2D(GetSmallAvatar(friendSteamId, false));
                        gameServiceFriendsList.Add(friend);
                    }
                }
            }

            RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.FriendsListReady));
        }

        public override void GetAvatarForPlayerId(GameServiceUserInfo user)
        {
            AccountID_t accountId = new AccountID_t(Convert.ToUInt32(user.PlayerId));
            CSteamID steamId = new CSteamID(accountId, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeIndividual);
            SteamWebApi.GetSteamUserAvatar(steamId.ToString(), user.SetAvatarFromSprite);

        }

        private Texture2D GetSmallAvatar(CSteamID user, bool dispatchReadyEvent = true)
        {



            int FriendAvatar = SteamFriends.GetMediumFriendAvatar(user);
            uint ImageWidth;
            uint ImageHeight;
            bool success = SteamUtils.GetImageSize(FriendAvatar, out ImageWidth, out ImageHeight);
            Texture2D returnTexture = null;

            if (success && ImageWidth > 0 && ImageHeight > 0)
            {
                byte[] Image = new byte[ImageWidth * ImageHeight * 4];
                returnTexture = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                success = SteamUtils.GetImageRGBA(FriendAvatar, Image, (int)(ImageWidth * ImageHeight * 4));
                if (success)
                {
                    returnTexture.LoadRawTextureData(Image);
                    returnTexture.Apply();

                }
                if (dispatchReadyEvent) RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.AvatarTextureReady, FlipTexture(returnTexture)));
            }
            else
            {
                Debug.LogError("Couldn't get avatar.");
            }

            if (returnTexture != null)
            {
                returnTexture = FlipTexture(returnTexture);
            }

            return returnTexture;
        }

        private Texture2D FlipTexture(Texture2D original)
        {
            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    flipped.SetPixel(i, yN - j - 1, original.GetPixel(i, j));
                }
            }

            flipped.Apply();

            return flipped;
        }






        public override bool IsAchivementCompleted(string achivementId)
        {
            if (IsSignedIn && !string.IsNullOrEmpty(achivementId))
            {
                bool result = false;
                SteamUserStats.GetAchievement(achivementId, out result);
                return result;
            }
            else
            {
                return false;
            }
        }

        public override void ReportAchivement(string achievementId, int completion = 100)
        {
            if (IsSignedIn)
            {
                SteamUserStats.SetAchievement(achievementId);
                SteamUserStats.StoreStats();
            }
        }

        public override void SubmitScore(string leaderbordId, int score, bool forceUpdate = false)
        {
            if (IsSignedIn)
            {
                if (leaderbordsStatus.ContainsKey(leaderbordId) && leaderbordsStatus[leaderbordId] != null)
                {
                    ELeaderboardUploadScoreMethod method = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;
                    if (forceUpdate) method = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate;
                    SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(leaderbordsStatus[leaderbordId].Value, method, score, null, 0);
                    CallResult<LeaderboardScoreUploaded_t> submitScoreCallback = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
                    submitScoreCallback.Set(handle);
                }
                else
                {
                    UnityEngine.Debug.Log("Leaderboard Id not found or not initialized.");
                }
            }
        }

        public override int GetScore(string leaderbordId)
        {
            if (IsSignedIn)
            {
                if (leaderbordsStatus.ContainsKey(leaderbordId) && leaderbordsStatus[leaderbordId] != null)
                {
                    CSteamID[] users = new CSteamID[1];
                    users[0] = SteamUser.GetSteamID();

                    SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntriesForUsers(leaderbordsStatus[leaderbordId].Value, users, users.Length);
                    CallResult<LeaderboardScoresDownloaded_t> getScoreCallBack = new CallResult<LeaderboardScoresDownloaded_t>();
                    getScoreCallBack.Set(handle, (LeaderboardScoresDownloaded_t pCallback, bool bIOFailure) => OnLeaderboardScoresDownloaded(pCallback, bIOFailure));


                }
                else
                {
                    if (leaderboardGetScoreQueue.IndexOf(leaderbordId) == -1)
                    {
                        InitLeaderboard(leaderbordId);
                        leaderboardGetScoreQueue.Add(leaderbordId);
                    }
                }
            }

            return -1;
        }

        public override void ShowLeaderbord(string leaderbordId)
        {

        }

        public override void InitLeaderboard(string leaderboardId)
        {
            if (leaderbordsStatus.ContainsKey(leaderboardId)) return;
            leaderbordsStatus.Add(leaderboardId, null);
            SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(leaderboardId);
            CallResult<LeaderboardFindResult_t> m_findResult = new CallResult<LeaderboardFindResult_t>();
            m_findResult.Set(hSteamAPICall, (LeaderboardFindResult_t pCallback, bool failure) => OnLeaderboardFindResult(pCallback, failure, leaderboardId));

        }

        private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool failure, string findId)
        {
            // UnityEngine.Debug.Log("STEAM LEADERBOARDS: Found - " + pCallback.m_bLeaderboardFound + " leaderboardID - " + pCallback.m_hSteamLeaderboard.m_SteamLeaderboard);

            if (!failure && leaderbordsStatus.ContainsKey(findId))
            {
                leaderbordsStatus[findId] = pCallback.m_hSteamLeaderboard;

                if (leaderboardGetScoreQueue.IndexOf(findId) > -1)
                {
                    GetScore(findId);
                    leaderboardGetScoreQueue.Remove(findId);
                }
            }
        }


        private void OnUserStatsReceived(UserStatsReceived_t pCallback)
        {
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.AchievementsReady));
            }
        }

        private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
        {

            LeaderboardScoreEntryArg arg = new LeaderboardScoreEntryArg("", -1, 0);

            if (pCallback.m_hSteamLeaderboard != null)
            {
                KeyValuePair<string, SteamLeaderboard_t?>? entry = leaderbordsStatus.Where(y => y.Value == pCallback.m_hSteamLeaderboard).FirstOrDefault();
                if (entry != null)
                {
                    arg.LeaderboardId = entry.Value.Key;
                }
            }

            if (!bIOFailure)
            {
                SteamLeaderboardEntries_t m_SteamLeaderboardEntries = pCallback.m_hSteamLeaderboardEntries;
                LeaderboardEntry_t LeaderboardEntry;
                bool ret = SteamUserStats.GetDownloadedLeaderboardEntry(m_SteamLeaderboardEntries, 0, out LeaderboardEntry, null, 0);

                if (ret)
                {
                    arg.Rank = LeaderboardEntry.m_nGlobalRank;
                    arg.Score = LeaderboardEntry.m_nScore;
                }
                else
                {
                    arg.Rank = -1;
                }
            }
            else
            {
                arg.Rank = -1;
            }

            if (!string.IsNullOrEmpty(arg.LeaderboardId))
            {
                RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.LeaderboardScoreReady, arg));
            }


        }

        private void OnUserStatsStored(UserStatsStored_t pCallback)
        {
        }

        private void OnAchievementStored(UserAchievementStored_t pCallback)
        {
        }

        private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
        {
            if (!bIOFailure)
            {
                KeyValuePair<string, SteamLeaderboard_t?>? entry = leaderbordsStatus.Where(y => y.Value == pCallback.m_hSteamLeaderboard).FirstOrDefault();
                if (entry != null)
                {
                    string leaderboardId = entry.Value.Key;

                    LeaderboardScoreEntryArg arg = new LeaderboardScoreEntryArg(leaderboardId, pCallback.m_nGlobalRankNew, pCallback.m_nScore);
                    RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.LeaderboardScoreReady, arg));
                }

            }
        }


    }
}
#endif