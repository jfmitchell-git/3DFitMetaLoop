using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class GameServiceEvent
    {
        public GameServiceEventType EventType { get; set; }
        public Object Args { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameServiceEvent"/> class.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="args">The arguments.</param>
        public GameServiceEvent(GameServiceEventType eventType, Object args)
        {
            this.EventType = eventType;
            this.Args = args;
        }

        public GameServiceEvent(GameServiceEventType eventType)
        {
            this.EventType = eventType;
        }
    }
}
