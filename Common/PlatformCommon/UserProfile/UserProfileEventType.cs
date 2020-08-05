using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.UserProfile
{
    public enum UserProfileEventType
    {
        Undefined = 0,
        UserProfileCreated = 1,
        UserProfileLoaded = 2, 
        UserProfileSaved = 3,
        UserProfileSavedOnline = 4,
        UserProfileError = 5
    }
}
