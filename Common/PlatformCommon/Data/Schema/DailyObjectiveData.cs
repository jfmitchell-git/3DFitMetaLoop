using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{

    public class DailyObjectiveData
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

        public string ObjectiveId { get; set; }
        public int CountRequired { get; set; }
        public DailyObjectiveType ObjectiveType { get; set; }
        public DailyObjectiveAvailabilityType DailyObjectiveAvailabilityType { get; set; }
        public int RewardData_Id { get; set; }

        public int LevelRequired { get; set; }
        public string MissionRequired { get; set; }


        private RewardData rewardData;
        [IgnoreCodeFirst, Ignore]
        public RewardData Rewards
        {
            get
            {
                if (rewardData == null && RewardData_Id > 0)
                    rewardData = DataLayer.Instance.GetTable<RewardData>().Where(y => y.Id == this.RewardData_Id).SingleOrDefault();

                return rewardData;
            }

        }


        //public bool IsObjectiveAvailable(MetaDataStateBase metaDataState, CampaignDataState campaignDataState)
        //{

        //    if (LevelRequired > 0)
        //    {
        //        if (metaDataState.PlayerLevel.LevelId < LevelRequired)
        //        {
        //            return false;
        //        }
        //    }

        //    if (!string.IsNullOrEmpty(MissionRequired))
        //    {
        //        var missionData = campaignDataState.StageHistory.Where(y => y.StageId == this.MissionRequired).FirstOrDefault();

        //        if (missionData == null || (int)missionData.Score < 1)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;

        //}

        public static DailyObjectiveData GetObjective(DailyObjectiveType type)
        {
            return DataLayer.Instance.GetTable<DailyObjectiveData>().Where(y => y.ObjectiveType == type).SingleOrDefault();
        }

    }
}
