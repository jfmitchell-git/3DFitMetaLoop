using dryginstudios.bioinc.data.meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{

    public abstract class MetaAttributesGroup
    {
        public List<MetaAttributeField> Fields { get; set; }
        public MetaAttributesGroup()
        {
            LoadFields();
        }

        private void LoadFields()
        {
            Fields = new List<MetaAttributeField>();
            List<FieldInfo> fieldsInfo = this.GetType().GetFields().ToList();
            fieldsInfo.Where(y => y.FieldType == typeof(MetaAttributeField)).ToList().ForEach(m => Fields.Add(m.GetValue(this) as MetaAttributeField));
        }

        public void LoadValues(MetaAttributes attributes)
        {
            foreach (MetaAttributeField field in Fields)
            {
                field.Value = attributes.GetValueByFieldName(field.DataMap);
            }
        }

    }

    public class MetaAttributeField
    {
        public float Value { get; set; }
        public string DataMap { get; set; }

        public MetaAttributeField(string dataMap)
        {
            this.DataMap = dataMap;
        }
        public MetaAttributeField(string dataMap, List<MetaAttributeField> Fields)
        {
            this.DataMap = dataMap;
        }
    }

}
