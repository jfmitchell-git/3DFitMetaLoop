using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.State;

using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaLoop.Common.PlatformCommon.Settings;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    

    public abstract partial class EventData
    {

        public const int CleanAfterDays = 30;

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

        public string MainImageName
        {
            get
            {
                return string.IsNullOrEmpty(ImageName) ? EventName : ImageName;
            }
        }


        public bool IsLiveOrSoonToBe()
        {
            if (State != null)
            {
                if (!State.Completed && (State.IsLive || (ShowHoursBefore > 0 &&  DateTime.UtcNow > StartTime.AddHours(-ShowHoursBefore))))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ShowAsReference(int delay)
        {
            if (State != null)
            {
                if (State.Completed && DateTime.UtcNow < EndTime.AddHours(delay))
                {
                    return true;
                }
            }
            return false;
        }



        public string ImageName { get; set; }
        public string Image2Name { get; set; }
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
        public bool ActivateOnLocalTime { get; set; }
        public EventType EventType { get; set; }
        public string StatisticSlots { get; set; }
        public int ShowHoursBefore { get; set; }
        public EventTagType EventTagType { get; set; }


        /// <summary>
        /// This section is subject to change. 
        /// </summary>
        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
        public bool RunTaskOnSegment { get; set; }


        [IgnoreCodeFirst, Ignore]
        public bool SendPushNotification
        {
            get
            {
                return (!string.IsNullOrEmpty(NotificationTitle) && !string.IsNullOrEmpty(NotificationMessage));
            }
        }

        [IgnoreCodeFirst, Ignore]
        public DateTime StartTime
        {
            get
            {
                DateTime myDate = DateTime.ParseExact(StartTimeString, MetaStateSettings._DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                return myDate;
            }
        }

        [IgnoreCodeFirst, Ignore]
        public DateTime EndTime
        {
            get
            {
                DateTime myDate = DateTime.ParseExact(EndTimeString, MetaStateSettings._DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                return myDate;
            }
        }


        [IgnoreCodeFirst, Ignore]
        public EventManagerStateItem State { get; set; }

        [IgnoreCodeFirst, Ignore]
        public bool MustBeClosed
        {
            get
            {
                if (State != null)
                {
                    if (State.Started && !State.Completed && DateTime.UtcNow > EndTime)
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        [IgnoreCodeFirst, Ignore]
        public bool MustBeStarted
        {
            get
            {
                if (State != null)
                {
                    if (!State.Started && DateTime.UtcNow > StartTime)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [IgnoreCodeFirst, Ignore]
        public bool MustBeCleaned
        {
            get
            {
                if (State != null)
                {
                    if (RunTaskOnSegment && State.Started && State.Completed && !State.MarkForDeletion && DateTime.UtcNow > EndTime.AddDays(CleanAfterDays))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [IgnoreCodeFirst, Ignore]
        public string TaskId
        {
            get
            {
                return EventId + "_Completed";
            }

        }

        public static List<EventData> GetAllEvents()
        {
            return DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(EventData)]).Cast<EventData>().Where(y => DateTime.UtcNow < ((EventData)y).EndTime.AddDays(CleanAfterDays + 1)).ToList();
        }

        public static List<EventData> GetRecentlyCompletedEvents(int delay = 24)
        {
            return DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(EventData)]).Cast<EventData>().Where(y => DateTime.UtcNow > y.EndTime && DateTime.UtcNow < y.EndTime.AddHours(delay)).ToList();
        }



        public string DisplayName
        {
            get
            {
                return this.EventName;
            }
        }

#if !BACKOFFICE
        public string DisplaySubTitle
        {
            get
            {
                return this.EventName;
            }
        }



        public TimeSpan? TimeLeft
        {
            get
            {
                if (State.IsLive)
                {
                    return EndTime - DateTime.UtcNow;
                }
                else
                {
                    return null;
                }

            }
        }

        public TimeSpan? StartIn
        {
            get
            {
                if (!State.Started)
                {
                    return StartTime - DateTime.UtcNow;
                }
                else
                {
                    return null;
                }

            }
        }
#endif

    }
}
