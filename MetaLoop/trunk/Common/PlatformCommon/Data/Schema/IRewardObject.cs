using dryginstudios.bioinc.data.meta;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public interface IRewardObject
    {
        RewardObjectType RewardObjectType { get; }
        List<RewardDataItem> PotentialRewards { get; }
        bool IsAvailable { get; }
        int ZOrder { get; }
    }
}
