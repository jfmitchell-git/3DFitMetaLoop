using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.UserProfile
{
    public class UserProfileEvent
    {
        public UserProfileEventType EventType { get; set; }
        public Object Args { get; set; }

        public UserProfileEvent(UserProfileEventType eventType)
        {
            this.EventType = eventType;
        }

        public UserProfileEvent(UserProfileEventType eventType, Object args)
        {
            this.EventType = eventType;
            this.Args = args;
        }
    }
}
