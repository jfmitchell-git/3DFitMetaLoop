using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public interface ICostObject
    {
        int ConsumableCost_Id { get; set; }
        ConsumableCost Cost { get; }
    }
}
