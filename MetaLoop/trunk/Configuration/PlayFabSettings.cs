using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Settings;
using System;

namespace MetaLoop.Configuration
{
    public class PlayFabSettings
    {
#if STAGING || DEBUG
        public static string DeveloperSecretKey = MetaStateSettings._PlayFabDeveloperSecretKey_Staging;
        public static string TitleId = MetaStateSettings._PlayFabTitleId_Staging;
        public static string PlayFabEnvironment = "Staging";
#else
        public static string DeveloperSecretKey = MetaStateSettings._PlayFabDeveloperSecretKey;
        public static string TitleId = MetaStateSettings._PlayFabTitleId;
        public static string PlayFabEnvironment = "Prod";
#endif
    }
}
