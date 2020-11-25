#if UNITY_IOS
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;


namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class GameCenterGameService : GameServiceBase
    {
   
        public override bool IsSignedIn
        {
            get
            {
                return Social.localUser.authenticated;
            }
        }

        public override string PlayerName
        {
            get
            {
                 if (IsSignedIn)
                {
                    return Social.localUser.userName;
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
                return Social.localUser.id;
            }
        }


        public override void Init()
        {
       
            Social.localUser.Authenticate(OnAuthenticateCompleted);
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

#endif