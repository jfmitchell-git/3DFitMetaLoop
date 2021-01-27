using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Settings
{
    public partial class MetaStateSettings
    {
        public static Dictionary<Type, Type> PolymorhTypes;
    
        public static float _MAJOR_VERSION = 0.1f;
        public static string GetMajorVersion()
        {
            string result = _MAJOR_VERSION.ToString("0.00");
            result = result.Replace(" ", "");
            result = result.Replace(",", ".");
            return result;
        }

        public static string SettingsClassName;
        static MetaStateSettings()
        {
            PolymorhTypes = new Dictionary<Type, Type>();

            var allProviders = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                          .Where(x => typeof(ISettingsProvider).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                          .Select(x => x.FullName).ToList();


            if (allProviders.Count == 0)
            {
                throw new Exception("MetaStateSettings could not locate ISettingsProvider.");
            }
            else if (allProviders.Count > 1)
            {
                throw new Exception("MetaStateSettings found multiples ISettingsProvider.");
            }
            else
            {
                SettingsClassName = allProviders.First();
            }

            var allFields = new Dictionary<string, object>();

            Type customMetaStateSettings;

#if BACKOFFICE
            customMetaStateSettings = Type.GetType(SettingsClassName + ", MetaLoop.GameLogic");
#else
            customMetaStateSettings = Type.GetType(SettingsClassName);
#endif

            FieldInfo[] fieldInfos = customMetaStateSettings.GetFields(BindingFlags.Static | BindingFlags.Public);
            fieldInfos.ToList().ForEach(y => allFields.Add(y.Name, y.GetValue(null)));
            var baseClassFields = typeof(MetaStateSettings).GetFields(BindingFlags.Static | BindingFlags.Public).ToList();
            foreach (var field in baseClassFields)
            {
                string replaceFieldName = field.Name.Substring(1);
                if (field.Name.StartsWith("_") && allFields.ContainsKey(replaceFieldName))
                {
                    field.SetValue(null, allFields[replaceFieldName]);
                }
            }
        }


        public static string _DataLayerDefaultNameSpace = "";
        public static string _RemoteAssetsPersistantName = "";

        public static string _ProductionEndpoint = "";
        public static string _StagingEndpoint = "";
        public static string _DevEndpoint = "";


        public static string _ServerAppVersionUrl = "AppVersion";


        public static string _PlayerXpId = "";
        public static string _SoftCurrencyId = "";
        public static string _HardCurrencyId = "";
        public static string _EnergyId = "";

        public static string _DateTimeFormat = "";
        public static string _BaseUnityFolder = "";

        public static string _DownloadableFolderPath = "";
        public static string _DownloadableStartupPath = "";
        public static string _DownloadableOnDemandPath = "";

        public static string _PlayFabDeveloperSecretKey_Staging = "";
        public static string _PlayFabTitleId_Staging = "";

        public static string _PlayFabDeveloperSecretKey = "";
        public static string _PlayFabTitleId = "";

        public static string _TitleDataKey_EventManager = "";
        public static string _TitleDataKey_CdnManifest = "";
        public static string _TitleDataKey_ServerInfo = "";
        public static string _TitleDataKey_RemoteConfig = "";

        public static string _AssetManagerVersionString = "";
        public static string _AssetManagerStartupFolder = "";
        public static string _AssetManagerOnGameReady = "";

        public static int _MaxPlayerLevel = 0;

        public static string _DatabaseName = "";
        public static string _DatabaseFileName = @"Assets\StreamingAssets\" + _DatabaseName;
        public static string _MetaDataStateFileName = "";

        public static string _DataKey_Inbox = "";

        public static string _GoogleLicenseKey = "";

        public static List<string> _UserDataToDownload = new List<string>() { _MetaDataStateFileName };

        public static string _AppleAppId = "";
        public static string _GooglePlayPackageId = "";
        public static string GetStoreLink()
        {
#if UNITY_IOS
            return @"http://itunes.apple.com/app/id" + _AppleAppId;
#endif
            return @"https://play.google.com/store/apps/details?id=" + _GooglePlayPackageId;
        }



    }
}
