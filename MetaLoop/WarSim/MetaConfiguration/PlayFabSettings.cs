using GameLogic;
using System;

namespace MetaConfiguration
{
    public class PlayFabSettings
    {
#if STAGING || DEBUG
        public const string DeveloperSecretKey = MetaStateSettingsCore.PlayFabDeveloperSecretKey_Staging;
        public const string TitleId = MetaStateSettingsCore.PlayFabTitleId_Staging;
        public const string PlayFabEnvironment = "Staging";
#else
        public const string DeveloperSecretKey = MetaStateSettings.PlayFabDeveloperSecretKey;
        public const string TitleId = MetaStateSettings.PlayFabTitleId;
        public const string PlayFabEnvironment = "Prod";
#endif
    }
}
