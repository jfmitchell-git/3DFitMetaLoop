using dryginstudios.bioinc.data.meta;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MetaLoop.Common.PlatformCommon.Data.Schema;

namespace MetaLoop.Common.PlatformCommon.State { 

    public class EventManagerStateItem
    {
        public string EventId { get; set; }
        public bool Started { get; set; }
        public bool Completed { get; set; }
        public bool MarkForDeletion { get; set; }

        [JsonIgnore]
        public bool IsLive
        {
            get
            {
                return (Started && !Completed);
            }
        }
    }

    public class EventManagerState
    {
        public List<EventManagerStateItem> Events { get; set; }

        public void SyncState(List<EventData> allEvents)
        {
            foreach (EventData eventData in allEvents)
            {
                EventManagerStateItem stateItem = Events.Where(y => y.EventId == eventData.EventId).SingleOrDefault();

                if (stateItem == null)
                {
                    stateItem = new EventManagerStateItem();
                    stateItem.EventId = eventData.EventId;
                    Events.Add(stateItem);
                }

                eventData.State = stateItem;

            }
        }


        public EventManagerState()
        {
            Events = new List<EventManagerStateItem>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
