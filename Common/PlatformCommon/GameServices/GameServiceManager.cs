#if !BACKOFFICE

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class GameServiceManager
    {
        private static bool isInit = false;
        private static GameServiceBase gameService = null;
        public static GameServiceBase GameService
        {
            get
            {
                if (gameService == null)
                {
                    CreateGameService();
                }
                return gameService;
            }
        }

       
        private static void CreateGameService()
        {
            if (!isInit)
            {

#if UNITY_EDITOR || UNITY_STANDALONE

                
#if STEAM
    gameService = new SteamGameService();
#endif

#elif UNITY_ANDROID
                // gameService = new GooglePlayGameService();    
#elif UNITY_IOS
                 gameService = new GameCenterGameService();
#endif
            }

            if (gameService == null) gameService = new GameServiceBase();
            isInit = true;
        }
    }
}

#endif
