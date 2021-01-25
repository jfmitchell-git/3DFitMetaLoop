using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{


    public class MetaDataStateBase
    {



        private static MetaDataStateBase current;
        public static void LoadData(MetaDataStateBase data)
        {
            current = data;
        }
        public static MetaDataStateBase Current
        {
            get
            {
                return current;
            }

        }

        private TimeSpan serverUTCDiff;
        public DateTime GetServerUTCDatetime()
        {
#if UNITY_EDITOR
            return DateTime.UtcNow;
#endif
            return DateTime.UtcNow + serverUTCDiff;
        }


        public DateTime CreationDate { get; set; }
        public string ProfileName { get; set; }
        public string UniqueId { get; set; }
        public DateTime ServerDateTime { get; set; }
        public DateTime NextDailyReset { get; set; }
        public int LastPlayerLevelClaimed { get; set; }
        public int TotalAdWatchedCount { get; set; }
        public MetaTimeZone MetaTimeZone { get; set; }
        public ConsumableState Consumables { get; set; }
        public OfferDataState OfferDataState { get; set; }
        public AchievementDataState AchievementDataState { get; set; }
        public SettingState SettingState { get; set; }
        public AdState AdState { get; set; }
        public ShopState ShopState { get; set; }
        public ObjectiveState ObjectiveState { get; set; }
        public Dictionary<DateTime, bool> LoginCalendar { get; set; }
        public Dictionary<string, int> Statistics { get; set; }
        public Dictionary<string, int> DailyMissionStats { get; set; }
        public string CountryCode { get; set; }


        public void OnPlayerLogin()
        {
            serverUTCDiff = ServerDateTime - DateTime.UtcNow;
            SyncLoginCalendar();

        }
        public string GetPlayerName()
        {
            if (!string.IsNullOrEmpty(SettingState.UpdatedPlayerName))
            {
                return SettingState.UpdatedPlayerName;
            }
            return ProfileName;
        }
        public int GetLastPlayerLevelClaimed()
        {
            if (LastPlayerLevelClaimed < 1)
            {
                return 1;
            }
            else
            {
                return LastPlayerLevelClaimed;
            }
        }

        public bool SyncLoginCalendar()
        {
            if (!LoginCalendar.ContainsKey(DateTime.UtcNow.Date))
            {
                LoginCalendar.Add(DateTime.UtcNow.Date, false);
                return true;
            }

            return false;
        }
        public int MustShowPlayerLevelUpgrade()
        {
            if (GetLastPlayerLevelClaimed() < PlayerLevel.LevelId)
            {
                return GetLastPlayerLevelClaimed() + 1;
            }
            return 0;
        }

        [JsonIgnore]
        public PlayerLevelData PlayerLevel
        {
            get
            {
                return PlayerLevelData.GetPlayerLevelData(Consumables.GetConsumableAmount(Consumable.GetByName(MetaStateSettings._PlayerXpId)));
            }
        }

        public void IncrementStatistic(string statName)
        {
            if (Statistics.ContainsKey(statName))
            {
                Statistics[statName]++;
            }
            else
            {
                Statistics.Add(statName, 1);
            }
        }

        public int GetStatistic(string statName)
        {
            if (Statistics.ContainsKey(statName))
            {
                return Statistics[statName];
            }
            else
            {
                return 0;
            }
        }
        public MetaDataStateBase()
        {
            Consumables = new ConsumableState();
            ShopState = new ShopState();
            ObjectiveState = new ObjectiveState();
            if (CreationDate == DateTime.MinValue)
            {
                CreationDate = DateTime.UtcNow;
            }
            AdState = new AdState();
            SettingState = new SettingState();
            LastPlayerLevelClaimed = 1;
            LoginCalendar = new Dictionary<DateTime, bool>();
            AchievementDataState = new AchievementDataState();
            Statistics = new Dictionary<string, int>();
            DailyMissionStats = new Dictionary<string, int>();
            OfferDataState = new OfferDataState();
        }

        public virtual void ApplyDailyReset()
        {
        }


    
        public static object FromJson(string json)
        {

            Type polyformType = MetaStateSettings.PolymorhTypes[typeof(MetaDataStateBase)];
            return JsonConvert.DeserializeObject(json, polyformType);
        }

        public static T FromJson<T>(string json) where T : new()
        {
            return (T)JsonConvert.DeserializeObject(json, typeof(T).GetType());
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
