
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
    public partial class AchievementData
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
        public int RewardData_Id { get; set; }

        public int Tier { get; set; }
        public int RequirementCount { get; set; }

        public string Parameter { get; set; }

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

        //[Ignore, IgnoreCodeFirst]
        //        public string Description
        //        {
        //            get
        //            {
        //#if !BACKOFFICE
        //                switch (AchievementType)
        //                {
        //                    case AchievementType.Complete_Death_Campaign:
        //                    case AchievementType.Complete_Life_Campaign:
        //                    case AchievementType.Complete_Global_Campaign:
        //                        return string.Format(commonscripts.resourcemanager.ResourceManager.GetValue("Achievement." + AchievementType.ToString() + ".Text"), Parameter);
        //                    default:
        //                        return string.Format(commonscripts.resourcemanager.ResourceManager.GetValue("Achievement." + AchievementType.ToString() + ".Text"), RequirementCount);
        //                }
        //#endif
        //                return AchievementType.ToString();
        //            }
        //        }
        //        public bool CanBeClaimed(MetaDataState playerState, CampaignDataState campaignDataState)
        //        {
        //            return ValidateAchievementCount(playerState, campaignDataState) >= RequirementCount;
        //        }
        //        public int ValidateAchievementCount(MetaDataState playerState, CampaignDataState campaignDataState)
        //        {
        //            int currentCount = 0;
        //            switch (AchievementType)
        //            {

        //                case AchievementType.Collect_Doctors_Modern:
        //                case AchievementType.Collect_Doctors_Alternative:
        //                case AchievementType.Collect_Doctors_Nanotech:
        //                case AchievementType.Collect_Doctors_Spiritual:

        //                    var allDoctorsWithTag = DataLayer.Instance.GetTable<CardData>().Where(y => y.Tags.Contains(DoctorTag.Get(Parameter))).ToList();
        //                    foreach (var doctorState in playerState.DoctorDataState)
        //                    {
        //                        if (allDoctorsWithTag.Where(y => y.Name == doctorState.ObjectId).Count() > 0)
        //                        {
        //                            currentCount++;
        //                        }
        //                    }

        //                    break;


        //                case AchievementType.Log_In_Days:
        //                    currentCount = playerState.LoginCalendar.Count;
        //                    break;


        //                case AchievementType.Hospital_Level:
        //                    currentCount = playerState.PlayerLevel.LevelId;
        //                    break;


        //                case AchievementType.Spend_Cash:
        //                    currentCount = playerState.Consumables.soft_currency_spent;
        //                    break;


        //                case AchievementType.Spend_Arena:
        //                    currentCount = playerState.Consumables.arena_credits_spent;
        //                    break;


        //                case AchievementType.Spend_Contest:
        //                    currentCount = playerState.Consumables.blitz_credits_spent;
        //                    break;


        //                case AchievementType.Win_Contest:
        //                    currentCount = playerState.GetStatistic(MetaStateSettings.StatisticBlitzWin);

        //                    break;


        //                case AchievementType.Complete_Arena:
        //                    currentCount = playerState.GetStatistic(MetaStateSettings.StatisticArenaWin);

        //                    break;


        //                case AchievementType.Buy_Supplies:
        //                    currentCount = playerState.GetStatistic(MetaStateSettings.StatisticBuySupplies);

        //                    break;


        //                case AchievementType.Rank_Up_Doctors:
        //                    currentCount = playerState.GetStatistic(MetaStateSettings.StatisticRankUp);

        //                    break;

        //                case AchievementType.Learn_Skill:
        //                    currentCount = playerState.GetStatistic(MetaStateSettings.StatisticLearnSkill);

        //                    break;

        //                case AchievementType.Complete_Life_Campaign:
        //                case AchievementType.Complete_Death_Campaign:
        //                case AchievementType.Complete_Global_Campaign:
        //                    currentCount = campaignDataState.StageHistory.Where(y => y.StageId == Parameter).Count() > 0 ? 1 : 0;

        //                    break;
        //            }

        //            return currentCount;

        //        }


        public static List<AchievementData> GetAchivements()
        {
            return DataLayer.Instance.GetTable<AchievementData>();
        }



#if !BACKOFFICE


        public int _Completion;
        public bool _CanBeClaimed;
        public int _CurrentCount;
        public bool IsVisible()
        {
            //int currentTier = MetaDataStateBase.Current.AchievementDataState.GetCurrentTierForAchievement(AchievementType);
            //_CurrentCount = ValidateAchievementCount(MetaDataStateBase.Current, gamedata.GameData.Current.CampaignDataState);

            //if (currentTier == Tier || (_CurrentCount >= RequirementCount && gamedata.GameData.Current.MetaDataState.AchievementDataState.Achievements.Where(y=> y.AchievementType == AchievementType && y.Tier == Tier).Count() == 0))
            //{


            //    if (_CurrentCount >= RequirementCount)
            //    {
            //        _CanBeClaimed = true;
            //        _Completion = 100;
            //    }
            //    else
            //    {
            //        _Completion = (int)((_CurrentCount / ((float)RequirementCount)) * 100);
            //    }

            //    return true;
            //}
            return false;

        }

#endif
    }
}
