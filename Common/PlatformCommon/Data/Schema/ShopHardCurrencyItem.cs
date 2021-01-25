using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{



    public abstract class ShopHardCurrencyItem : PurchasableItem
    {
        private int id;
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                //Load relationships here...
            }
        }


        public DisplayTagType DisplayTagType { get; set; }





    }
}
