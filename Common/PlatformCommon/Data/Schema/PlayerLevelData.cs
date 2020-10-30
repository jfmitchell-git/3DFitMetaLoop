using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon.Settings;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public class PlayerLevelData : RewardObject
    {
        private int id;
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                //Load relationships here...
            }
        }

        public int LevelId { get; set; }
        public int XpRequired { get; set; }
        public int EnergyCap { get; set; }

        public PlayerLevelData GetNextLevel()
        {
            int index = DataLayer.Instance.GetTable<PlayerLevelData>().IndexOf(this);
            return DataLayer.Instance.GetTable<PlayerLevelData>().ElementAtOrDefault(index + 1);
        }

        public static PlayerLevelData GetPlayerLevelData(int currentXp)
        {
            var level = DataLayer.Instance.GetTable<PlayerLevelData>().Where(y => currentXp >= y.XpRequired).OrderByDescending(y => y.XpRequired).Take(1).Single();
            if (level.LevelId > MetaStateSettings._MaxPlayerLevel)
            {
                level = DataLayer.Instance.GetTable<PlayerLevelData>().Where(y => y.LevelId == MetaStateSettings._MaxPlayerLevel).Single();
            }
            return level;
        }

    }
}
