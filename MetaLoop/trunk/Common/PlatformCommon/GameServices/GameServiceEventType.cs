using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public enum GameServiceEventType
    {
        Undefined = 0,
        SignInSuccess = 1,
        SingInFailed = 2,
        SignedOut = 3,
        AchievementsReady = 4,
        LeaderbordsReady = 5,
        CloudDataLoaded = 6,
        LeaderboardScoreReady = 7,
        AvatarTextureReady = 8,
        FriendsListReady = 9,
        PlayerAvatarTextureReady = 10,

    }
}
