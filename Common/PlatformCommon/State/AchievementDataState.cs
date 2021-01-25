using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLoop.Common.PlatformCommon.State
{

    [Serializable]
    public class AchievementDataStateEntry
    {
        public AchievementType AchievementType { get; set; }
        public int Tier { get; set; }
        public bool Claimed { get; set; }
        public int Count { get; set; }

    }

    [Serializable]
    public class AchievementDataState
    {
        public List<AchievementDataStateEntry> Achievements { get; set; }
        public AchievementDataState()
        {
            Achievements = new List<AchievementDataStateEntry>();
        }


        public bool RegisterAchievementCount(AchievementType type)
        {
            var achievementData = AchievementData.GetAchievementData(type);
            if (achievementData != null)
            {
                var stateItem = Achievements.Where(y => y.AchievementType == type).SingleOrDefault();

                if (stateItem == null)
                {
                    stateItem = new AchievementDataStateEntry();
                    stateItem.AchievementType = type;
                    stateItem.Tier = 1;
                    Achievements.Add(stateItem);
                }

                stateItem.Count++;

                return true;
            }

            return false;
        }
        public int GetCurrentTierForAchievement(AchievementType type)
        {
            var lastTier = Achievements.Where(y => y.AchievementType == type).OrderBy(y => y.Tier).LastOrDefault();

            if (lastTier == null)
            {
                return 1;
            }
            else
            {
                return lastTier.Tier + 1;
            }
        }


    }
}
