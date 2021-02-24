#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{

    [System.Serializable]
    public enum MessageType
    {
        Popup = 1,
        Ticker = 2,
        Tooltip = 3

    }
}
#endif