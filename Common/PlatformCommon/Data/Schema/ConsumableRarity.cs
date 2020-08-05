using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{

    public class ConsumableRarity
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

        public int RarityId { get; set; }
        public string Name { get; set; }
        public int DropRate { get; set; }







    }
}
