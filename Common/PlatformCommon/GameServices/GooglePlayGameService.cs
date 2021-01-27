﻿
#if !BACKOFFICE



#if UNITY_ANDROID && USEGOOGLEPLAY
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class GooglePlayGameService : GameServiceBase
    {
        public static void SetGameInfo(string appId, string webClientId)
        {
            //GameInfo.ApplicationId = appId;
            //GameInfo.WebClientId = webClientId;
        }



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
             GameServiceManager.IsInit = true;
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

#else

using System;

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class GooglePlayGameService : GameServiceBase
    {
 


        public override bool IsSignedIn
        {
            get
            {
                return false;
            }
        }

        public override string PlayerName
        {
            get
            {
                return "Guest";
            }
        }


        public override string PlayerId
        {
            get
            {
                return   "Guest";
            }
        }

        public string GetServerAuthCode()
        {
            return string.Empty;
        }


        public void GetAnotherServerAuthCode(Action<string> callback)
        {
    
        }

    }
}

#endif
#endif