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

    public enum DailyObjectiveStatus
    {
        Undefined,
        Availabe,
        Unavailabe,
        CanBeClaimed,
        Claimed
    }
    public class DailyObjectiveData : RewardObject
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

        private int countRequired;
        public int CountRequired
        {
            get
            {
                if (countRequired == -1)
                {

                }
                return countRequired;
            }
            set
            {
                countRequired = value;
            }
        }
        public DailyObjectiveType ObjectiveId { get; set; }
        public int LevelRequired { get; set; }

        public ObjectiveStateItem GetObjectiveState(MetaDataStateBase state)
        {
            return state.ObjectiveState.AllObjectiveStateItem.Where(y => y.Id == ObjectiveId).SingleOrDefault();
        }
        public DailyObjectiveStatus GetObjectiveStatus(MetaDataStateBase state)
        {
            var stateItem = GetObjectiveState(state);

            if (stateItem != null)
            {
                if (stateItem.Claimed) return DailyObjectiveStatus.Claimed;
                if (LevelRequired > 0 && state.PlayerLevel.LevelId < LevelRequired) return DailyObjectiveStatus.Unavailabe;

                if (stateItem.Count >= CountRequired)
                {
                    return DailyObjectiveStatus.CanBeClaimed;
                }

                return DailyObjectiveStatus.Availabe;
            }

            return DailyObjectiveStatus.Unavailabe;
        }


#if !BACKOFFICE
        public virtual DailyObjectiveStatus GetObjectiveStatus()
        {
            return GetObjectiveStatus(MetaDataStateBase.Current);
        }

#endif

    }
}
