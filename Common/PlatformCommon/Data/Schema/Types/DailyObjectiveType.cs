using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema.Types
{
    public enum DailyObjectiveType
    {
        Undefined,
        Win1Death,
        Win1Life,
        Win3Death,
        Win3Life,
        PlaceBoosters1,
        PlaceBoosters2,
        Attempt3Hard,
        Attempt3Lethal,
        WatchAd,
        Complete1Achievement,
        CompleteAll,
        UpgradeSkills1
    }

    public enum DailyObjectiveAvailabilityType
    {
        Default,
        Tutorial,
    }

}
