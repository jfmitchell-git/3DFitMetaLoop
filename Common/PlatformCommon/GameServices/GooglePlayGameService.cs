#if UNITY_ANDROID1
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;

namespace dryginstudios.commonscripts.gameservices
{
    public class GooglePlayGameService : GameServiceBase
    {
        public static void SetGameInfo(string appId, string webClientId)
        {
            //GameInfo.ApplicationId = appId;
            //GameInfo.WebClientId = webClientId;
        }

        private bool isInit = false;

        public override bool IsSignedIn
        {
            get
            {
                return PlayGamesPlatform.Instance.IsAuthenticated();
            }
        }

        public override string PlayerName
        {
            get
            {
                if (IsSignedIn)
                {
                    return PlayGamesPlatform.Instance.GetUserDisplayName();
                }
                else
                {
                    return base.PlayerName;
                }

            }
        }


        public override string PlayerId
        {
            get
            {
                return PlayGamesPlatform.Instance.GetUserId();
            }
        }

        public string GetServerAuthCode()
        {
            return PlayGamesPlatform.Instance.GetServerAuthCode();


        }

        public void GetAnotherServerAuthCode(Action<string> callback)
        {
            PlayGamesPlatform.Instance.GetAnotherServerAuthCode(false, callback);
        }

        public override void Init()
        {
            if (isInit) return;
            //The following grants profile access to the Google Play Games SDK.
            //Note: If you also want to capture the player's Google email, be sure to add
            //.RequestEmail() to the PlayGamesClientConfiguration
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .AddOauthScope("profile")
            .RequestServerAuthCode(false)
            .Build();


            PlayGamesPlatform.InitializeInstance(config);


            // recommended for debugging:
            PlayGamesPlatform.DebugLogEnabled = true;

            // Activate the Google Play Games platform
            PlayGamesPlatform.Activate();


            PlayGamesPlatform.Instance.Authenticate(OnAuthenticateCompleted);
            isInit = true;
        }

        private void OnAuthenticateCompleted(bool success)
        {
            if (success)
            {
                RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.SignInSuccess));
            }
            else
            {
                RaiseGameServiceEvent(new GameServiceEvent(GameServiceEventType.SingInFailed));
            }
        }
    }
}

#endif