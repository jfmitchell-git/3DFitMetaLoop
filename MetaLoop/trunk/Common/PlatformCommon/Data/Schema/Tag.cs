using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public class Tag
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
            return DataLayer.Instance.GetTable<Tag>().Where(y => y.Id == id).SingleOrDefault();
        }

        //JF: j ai pas trouve rune autre facon intelligente de faire ca, tu le fixeras comme bon te sembleras! :D
        public static Tag GetTagByName(string name)
        {
            return DataLayer.Instance.GetTable<Tag>().Where(y => y.TagName == name).SingleOrDefault();
        }
    }
}
