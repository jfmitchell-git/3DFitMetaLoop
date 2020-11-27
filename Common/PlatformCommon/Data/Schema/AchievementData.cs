
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
    public enum AchievementStatus
    {
        Undefined,
        Availabe,
        Unavailabe,
        CanBeClaimed,
        Claimed
    }

    public partial class AchievementData : RewardObject
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

        public AchievementType AchievementType { get; set; }
        public int Tier { get; set; }
        public int RequirementCount { get; set; }
        public string Parameter { get; set; }


        public int GetCurrentCount(MetaDataStateBase state)
        {
            var stateItem = GetAchievementStateItem(state);
            if (stateItem == null) return 0;
            return stateItem.Count;
        }

        public int GetCompletion(MetaDataStateBase state)
        {
            float completion = (float) ((float)GetCurrentCount(state)) / ((float)RequirementCount) * 100f;
            if (completion < 0) completion = 0;
            if (completion > 100) completion = 100;

           
            return (int)completion;
        }
        public AchievementDataStateEntry GetAchievementStateItem(MetaDataStateBase state)
        {
            return state.AchievementDataState.Achievements.Where(y => y.AchievementType == AchievementType).SingleOrDefault();
        }
        public AchievementStatus GetAchievementStatus(MetaDataStateBase state)
        {
            var stateItem = GetAchievementStateItem(state);

            if (stateItem != null)
            {
                if (stateItem.Claimed) return AchievementStatus.Claimed;
             
                if (stateItem.Count >= RequirementCount)
                {
                    return AchievementStatus.CanBeClaimed;
                }

                return AchievementStatus.Availabe;
            } else
            {
                return AchievementStatus.Availabe;
            }
        }


#if !BACKOFFICE
        public virtual AchievementStatus GetAchievementStatus()
        {
            return GetAchievementStatus(MetaDataStateBase.Current);
        }

        public virtual int GetCurrentCount()
        {
            return GetCurrentCount(MetaDataStateBase.Current);
        }

        public virtual int GetCompletion()
        {
            return GetCompletion(MetaDataStateBase.Current);
        }

#endif


    }
}
