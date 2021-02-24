#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{
    public class MessageHistory
    {
        public DateTime Time;
        public MessageType Type;
        public string StyleName;

    }
}
#endif