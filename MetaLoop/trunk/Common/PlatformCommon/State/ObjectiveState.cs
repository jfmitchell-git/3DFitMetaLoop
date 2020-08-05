using dryginstudios.bioinc.data;
using dryginstudios.bioinc.data.meta;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{
    [Serializable]
    public class ObjectiveStateItem
    {
        public DailyObjectiveType Id { get; set; }
        public int Count { get; set; }
        public bool Claimed { get; set; }


        public ObjectiveStateItem()
        {

        }

        public ObjectiveStateItem(DailyObjectiveType id)
        {
            Id = id;
        }

    }


    [Serializable]
    public class ObjectiveState
    {

        public List<ObjectiveStateItem> AllObjectiveStateItem;

        public ObjectiveState()
        {

        }

        public void CreateDefaultVariableCostValues(bool reset = false)
        {
            if (reset) AllObjectiveStateItem = null;
            if (AllObjectiveStateItem == null || AllObjectiveStateItem.Count == 0 || Enum.GetValues(typeof(DailyObjectiveType)).Length != AllObjectiveStateItem.Count)
            {
                AllObjectiveStateItem = new List<ObjectiveStateItem>();
                AllObjectiveStateItem.Clear(); //Odd serialisation bug?

                var values = Enum.GetValues(typeof(DailyObjectiveType)).Cast<DailyObjectiveType>();

                foreach (var objType in values)
                {
                    AllObjectiveStateItem.Add(new ObjectiveStateItem(objType));
                }
            }
        }

        public void RegisterObjective(DailyObjectiveType type, int count = 1)
        {
            CreateDefaultVariableCostValues();
            AllObjectiveStateItem.Where(y => y.Id == type).Single().Count += count;
        }

        public ObjectiveStateItem GetObjectiveDataItem(DailyObjectiveType type)
        {
            CreateDefaultVariableCostValues();
            var debug = AllObjectiveStateItem;
            return AllObjectiveStateItem.Where(y => y.Id == type).Single();
        }

        //public void RegisterMissionBasedObjectives(MissionData mission, bool userWon, bool autoWin, int count = 1, MetaDataState state = null)
        //{

        //    if (!autoWin && mission.CampaignId == 1 && mission.ChapterId == 1 && mission.Number <= 3) RegisterObjective(DailyObjectiveType.LifeCampaignTutorial);

        //    if (state != null && !state.IsTutorialCompleted)
        //    {
        //        return;
        //    }

        //    if (mission.CampaignId == 1 || mission.CampaignId == 4) RegisterObjective(DailyObjectiveType.DeathCampaign, count);
        //    if (mission.CampaignId == 2) RegisterObjective(DailyObjectiveType.LifeCampaign, count);
        //    if (mission.MissionType == MissionType.Campaign && userWon) RegisterObjective(DailyObjectiveType.CampaignMissions, count);
        //    if (mission.MissionType == MissionType.Challenge && userWon) RegisterObjective(DailyObjectiveType.CompleteChallenges, count);
        //    if (autoWin) RegisterObjective(DailyObjectiveType.AutoWin, count);


        //    //if (!autoWin && mission.CampaignId == 0 && mission.ChapterId > 0 && mission.Number > 1) RegisterObjective(DailyObjectiveType.LifeCampaignTutorial);
        //    //if (!autoWin && mission.CampaignId == 0 && mission.ChapterId > 0 && mission.Number > 2) RegisterObjective(DailyObjectiveType.LifeCampaignTutorial2);

        //    //if (mission.CampaignId == 1 || mission.CampaignId == 4) RegisterObjective(DailyObjectiveType.DeathCampaign, count);
        //    //if (mission.CampaignId == 2) RegisterObjective(DailyObjectiveType.LifeCampaign, count);
        //    //if (mission.MissionType == MissionType.Campaign && userWon) RegisterObjective(DailyObjectiveType.CampaignMissions, count);
        //    //if (mission.MissionType == MissionType.Challenge && userWon) RegisterObjective(DailyObjectiveType.CompleteChallenges, count);
        //    //if (autoWin) RegisterObjective(DailyObjectiveType.AutoWin, count);

        //}
    }
}
