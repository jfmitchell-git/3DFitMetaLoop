using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data
{
    public class MetaSchemaOrder : Attribute
    {
        private int order;
        public int Order
        {
            get { return order; }
        }
        
        public MetaSchemaOrder(int order) { this.order = order; }
    }
}
