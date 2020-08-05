using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{

    public enum StateInboxMessageType
    {
        Undefined = 0, //Genernal // Anoucement
        ArenaReward = 1,
        BlitzMilestoneReward = 2,
        BlitzReward = 3,
        RaidReward = 4,
        Support = 5

    }

    public enum StateInboxMessageState
    {
        Unread,
        Read
    }

    [Serializable]
    public class StateInboxMessage
    {
        public const string DefaultCulture = "en";

        public string MessageId { get; set; }
        public StateInboxMessageType MessageType { get; set; }
        public StateInboxMessageState State { get; set; }
        public string RewardId { get; set; }
        public Dictionary<string, string> Message { get; set; }
        public Dictionary<string, string> Subject { get; set; }
        public Dictionary<string, string> Params { get; set; }
        public DateTime Date { get; set; }
        public DateTime ExpireOn { get; set; }

        public StateInboxMessage()
        {
            Message = new Dictionary<string, string>();
            Subject = new Dictionary<string, string>();
            Params = new Dictionary<string, string>();
        }

        public List<KeyValuePair<string, int>> GetRewards()
        {
            if (Params.ContainsKey("Rewards"))
            {
                List<KeyValuePair<string, int>> rewards = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(Params["Rewards"]);
                return rewards;
            }
            else
            {
                return null;
            }

        }

#if !BACKOFFICE

        public string GetLocalizedMessage()
        {
            //switch (MessageType)
            //{
            //    case StateInboxMessageType.Undefined:
            //    case StateInboxMessageType.Support:

            //        if (Message.ContainsKey(ResourceManager.CurrentCulture.ToLower()))
            //        {
            //            return Message[ResourceManager.CurrentCulture.ToLower()];
            //        }
            //        else if ((Message.ContainsKey(DefaultCulture)))
            //        {
            //            return Message[DefaultCulture];
            //        }

            //        break;


            //    case StateInboxMessageType.ArenaReward:

            //        string rank = Params["Rank"];
            //        string result = string.Format(ResourceManager.GetValue("Homescreen.ArenaReward.Body"), rank);
            //        return result;




            //    case StateInboxMessageType.BlitzReward:

            //        string eventId = Params["EventId"];

            //        BlitzData blitzEvent = DataLayer.Instance.GetTable<BlitzData>().Where(y => y.EventId == eventId).SingleOrDefault();
            //        if (blitzEvent != null)
            //        {
            //            eventId = blitzEvent.DisplayName;
            //        }


            //        string rankBlitz = Params["Rank"];
            //        string resultBlitz = string.Format(ResourceManager.GetValue("Homescreen.BlitzReward.Body"), rankBlitz, eventId);
            //        return resultBlitz;




            //}

            return string.Empty;
        }
        public string GetLocalizedSubject()
        {
            //switch (MessageType)
            //{
            //    case StateInboxMessageType.Undefined:
            //    case StateInboxMessageType.Support:

            //        if (Subject.ContainsKey(ResourceManager.CurrentCulture.ToLower()))
            //        {
            //            return Subject[ResourceManager.CurrentCulture.ToLower()];
            //        }
            //        else if ((Subject.ContainsKey(DefaultCulture)))
            //        {
            //            return Subject[DefaultCulture];
            //        }

            //        break;

            //    case StateInboxMessageType.ArenaReward:

            //        string rank = Params["Rank"];
            //        string result = string.Format(ResourceManager.GetValue("Homescreen.ArenaReward.Subjet"), rank);
            //        return result;

            //    case StateInboxMessageType.BlitzReward:

            //        string eventId = Params["EventId"];
            //        var eventData = DataLayer.Instance.GetTable<EventData>().Where(y => y.EventId == eventId).SingleOrDefault();
            //        if (eventData != null)
            //        {
            //            eventId = eventData.DisplayName;
            //        }

            //        string rankBlitz = Params["Rank"];
            //        string resultBlitz = string.Format(ResourceManager.GetValue("Homescreen.BlitzReward.Subjet"), eventId);
            //        return resultBlitz;
            //}
            return string.Empty;
        }
#endif

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}


