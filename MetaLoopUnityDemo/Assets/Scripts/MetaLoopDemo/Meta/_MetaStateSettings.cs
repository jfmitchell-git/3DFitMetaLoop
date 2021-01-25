using MetaLoop.Common.PlatformCommon.Settings;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.GameLogic.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.GameLogic
{
    public partial class _MetaStateSettings
    {
        public static void Init()
        {
            new MetaSettings();
            MetaStateSettings.PolymorhTypes.Add(typeof(MetaDataStateBase), typeof(MetaDataState));
            MetaStateSettings.PolymorhTypes.Add(typeof(MetaLoop.Common.PlatformCommon.Data.Schema.Consumable), typeof(Consumable));
        }

    }
}
