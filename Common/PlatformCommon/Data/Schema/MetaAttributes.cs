using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public class MetaAttributes
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

        [DefaultZero]
        public float Val1 { get; set; }
        [DefaultZero]
        public float Val2 { get; set; }
        [DefaultZero]
        public float Val3 { get; set; }
        [DefaultZero]
        public float Val4 { get; set; }
        [DefaultZero]
        public float Val5 { get; set; }
        [DefaultZero]
        public float Val6 { get; set; }
        [DefaultZero]
        public float Val7 { get; set; }
        [DefaultZero]
        public float Val8 { get; set; }
        [DefaultZero]
        public float Val9 { get; set; }
        [DefaultZero]
        public float Val10 { get; set; }
        [DefaultZero]
        public float Val11 { get; set; }


        public float GetValueByFieldName(string field)
        {
            switch(field.ToLower())
            {
                case "val1":
                    return Val1;

                case "val2":
                    return Val2;

                case "val3":
                    return Val3;

                case "val4":
                    return Val4;

                case "val5":
                    return Val5;

                case "val6":
                    return Val6;

                case "val7":
                    return Val7;

                case "val8":
                    return Val8;

                case "val9":
                    return Val9;

                case "val10":
                    return Val10;

                case "val11":
                    return Val11;

            }

            return 0f;

        }

    }
}
