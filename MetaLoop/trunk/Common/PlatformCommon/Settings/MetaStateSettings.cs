using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Settings
{
    public partial class MetaStateSettings
    {
        public static string SettingsClassName;
        static MetaStateSettings()
        {

            var allProviders = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                          .Where(x => typeof(ISettingsProvider).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                          .Select(x => x.FullName).ToList();


            if (allProviders.Count == 0)
            {
                throw new Exception("MetaStateSettings could not locate ISettingsProvider.");
            } else if (allProviders.Count > 1)
            {
                throw new Exception("MetaStateSettings found multiples ISettingsProvider.");
            } else
            {
                SettingsClassName = allProviders.First();
            }

            var allFields = new Dictionary<string, object>();
            var customMetaStateSettings = Type.GetType(SettingsClassName);
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

        public static string _ProductionEndpoint = "";
        public static string _StagingEndpoint = "";
        public static string _DevEndpoint = "";

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


        public static string _AssetManagerVersionString = "";
        public static string _AssetManagerStartupFolder = "";
        public static string _AssetManagerOnGameReady = "";

        public static int _MaxPlayerLevel = 0;

        public static string _DatabaseName = "";


    }
}
