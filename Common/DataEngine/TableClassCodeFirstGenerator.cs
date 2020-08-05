using MetaLoop.Common.PlatformCommon.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MetaLoop.Common.DataEngine
{


    public class TableClassCodeFirstGenerator
    {
        private List<KeyValuePair<PropertyInfo, Type>> _fieldInfo = new List<KeyValuePair<PropertyInfo, Type>>();
        private string _className = String.Empty;

        private Dictionary<Type, String> dataMapper
        {
            get
            {
                // Add the rest of your CLR Types to SQL Types mapping here
                Dictionary<Type, String> dataMapper = new Dictionary<Type, string>();
                dataMapper.Add(typeof(int), "INTEGER");
                dataMapper.Add(typeof(string), "TEXT");
                dataMapper.Add(typeof(bool), "INTEGER");
                dataMapper.Add(typeof(DateTime), "REAL");
                dataMapper.Add(typeof(float), "REAL");

                return dataMapper;
            }
        }

        public List<KeyValuePair<PropertyInfo, Type>> Fields
        {
            get { return this._fieldInfo; }
            set { this._fieldInfo = value; }
        }

        public string ClassName
        {
            get { return this._className; }
            set { this._className = value; }
        }

        public TableClassCodeFirstGenerator(Type t)
        {
            this._className = t.Name;

            foreach (PropertyInfo p in t.GetProperties())
            {
                KeyValuePair<PropertyInfo, Type> field = new KeyValuePair<PropertyInfo, Type>(p, p.PropertyType);

                bool hasIgnoreFlag = Attribute.IsDefined(p, typeof(IgnoreCodeFirst));

                if (!hasIgnoreFlag) this.Fields.Add(field);
            }
        }

        public string CreateTableScript()
        {
            System.Text.StringBuilder script = new StringBuilder();

            script.AppendLine("CREATE TABLE " + this.ClassName);
            script.AppendLine("(");

            for (int i = 0; i < this.Fields.Count; i++)
            {
                KeyValuePair<PropertyInfo, Type> field = this.Fields[i];

                if (dataMapper.ContainsKey(field.Value))
                {
                    script.Append("\t '" + field.Key.Name + "'\t" + dataMapper[field.Value]);
                }
                else
                {
                    // Complex Type? 
                    script.Append("\t '" + field.Key.Name + "' INTEGER");
                }

                if (field.Key.Name == "Id")
                {
                    script.Append(" NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE");

                }

                bool isDefaultZero = Attribute.IsDefined(field.Key, typeof(DefaultZero));

                if (isDefaultZero)
                {
                    script.Append(" NOT NULL DEFAULT 0");
                }

                if (field.Value.IsEnum)
                {
                    script.Append(" NOT NULL DEFAULT 0");
                }

                if (i != this.Fields.Count - 1)
                {
                    script.Append(",");
                }

                script.Append(Environment.NewLine);
            }

            script.AppendLine(");");

            return script.ToString();
        }
    }
}

