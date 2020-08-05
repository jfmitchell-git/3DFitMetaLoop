using dryginstudios.bioinc.data.meta;
using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
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

    }

    [Serializable]
    public class AchievementDataState
    {
        public List<AchievementDataStateEntry> Achievements { get; set; }
        public AchievementDataState()
        {
            Achievements = new List<AchievementDataStateEntry>();
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
        //public void RegisterAchievement(MetaDataState playerState, CampaignDataState campaignDataState, AchievementData achievementData)
        //{
        //    if (Achievements.Where(y => y.AchievementType == achievementData.AchievementType && y.Tier == achievementData .Tier).Count() == 0 & achievementData.CanBeClaimed(playerState, campaignDataState))
        //    {
        //        Achievements.Add(new AchievementDataStateEntry() { AchievementType = achievementData.AchievementType, Tier = achievementData.Tier, Claimed = true });
        //        foreach (var reward in achievementData.Rewards.RewardItems)
        //        {
        //            playerState.Consumables.AddConsumable(reward.Consumable, reward.Amount);
        //        }
        //    }
        //}

    }
}
