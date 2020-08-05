using System;
using System.Collections.Generic;
using System.Text;

namespace MetaBackend.Common.Data
{
    public class MetaDependency : Attribute
    {
        private Type type;
        public Type Type
        {
            get { return type; }
        }
        public MetaDependency() { }
        public MetaDependency(Type type) { this.type = type; }
    }
}
