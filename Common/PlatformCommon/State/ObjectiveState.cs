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

    }
}
