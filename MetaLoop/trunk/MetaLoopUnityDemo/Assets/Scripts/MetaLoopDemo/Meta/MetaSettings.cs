
using MetaLoop.Common.PlatformCommon.Settings;

namespace MetaLoopDemo.Meta
{
    public class MetaSettings : ISettingsProvider
    {

        public static float MAJOR_VERSION = 0.1f;
        public static string GetMajorVersion()
        {
            string result = MAJOR_VERSION.ToString("0.00");
            result = result.Replace(" ", "");
            result = result.Replace(",", ".");
            return result;
        }

        public const string ProductionEndpoint = @"https://metaloopdemo.azurewebsites.net/api/";
        public const string StagingEndpoint = @"https://metaloopdemo.azurewebsites.net/api/";
        public const string DevEndpoint = @"https://metaloopdemo.azurewebsites.net/api/";
        public const string ServerAppVersionUrl = ProductionEndpoint + "AppVersion";

        public const string PlayerXpId = "PlayerXp";
        public const string SoftCurrencyId = "Gold";
        public const string HardCurrencyId = "Gems";
        public const string EnergyId = "Energy";

        public const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        public static string BaseUnityFolder = "";  //Setting applied from publish bot.

        public const string DownloadableFolderPath = @"\Downloadable";
        public const string DownloadableStartupPath = @"\Startup";
        public const string DownloadableOnDemandPath = @"\OnDemand";

        public const string PlayFabDeveloperSecretKey_Staging = "Z3OJCX8JS7ABHMH4O3QTZDU698JTP97KJO4KBBB3GUJRFH9AGX";
        public const string PlayFabTitleId_Staging = "FE17A";

        public const string PlayFabDeveloperSecretKey = "Z3OJCX8JS7ABHMH4O3QTZDU698JTP97KJO4KBBB3GUJRFH9AGX";
        public const string PlayFabTitleId = "FE17A";

        public const string TitleDataKey_EventManager = "EventManager";
        public const string TitleDataKey_CdnManifest = "CdnManifest";
        public const string TitleDataKey_ServerInfo = "ServerInfo";


        public const string AssetManagerVersionString = ".$";
        public const string AssetManagerStartupFolder = @"Startup/";
        public const string AssetManagerOnGameReady = @"OnGameReady/";

        public const int MaxPlayerLevel = 100;

        public const string DatabaseName = "MetaLoopDemo.db";
        public const string DatabaseFileName = @"\Assets\StreamingAssets\" + DatabaseName;

        public const string MetaDataStateFileName = "MetaDataState.json";

    }
}
