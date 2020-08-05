using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data
{
    public class MetaDependency : Attribute
    {
        private Type type;
        public Type Type
        {
            get { return type; }
        }

        private bool createRelation;
        public bool CreateRelation
        {
            get { return createRelation; }
        }
        public MetaDependency() { }
        public MetaDependency(Type type) { this.type = type; }

        public MetaDependency(Type type, bool createRelation) { this.type = type; this.createRelation = createRelation; }
    }
}
