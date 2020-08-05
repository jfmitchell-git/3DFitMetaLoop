using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Protocol
{
    public enum GameUnavailableMessageType
    {
        Undefined,
        MAINTENANCE,
        HOST_UNREACHABLE,
        BACKOFFICE_ERROR,
        VERSION_MISMATCH,
        INTERNET_ERROR,
        DATA_MISMATCH,
        LOGIN_MISMATCH,
        LINKED_NEW_ACCOUNT_EXIST
    }
}
