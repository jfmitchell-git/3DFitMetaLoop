using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaLoop.Common.PlatformCommon.Settings;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public abstract class Tag
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

        public string TagName { get; set; }
        public TagType TagType { get; set; }

        public static Tag GetTagById(int id)
        {
            return DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(Tag)]).Cast<Tag>().Where(y => y.Id == id).SingleOrDefault();
        }

        public static Tag GetTagByName(string name)
        {
            return DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(Tag)]).Cast<Tag>().SingleOrDefault();
        }


    }
}
