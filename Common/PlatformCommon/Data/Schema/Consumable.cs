using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Linq;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    [Serializable]
    public class Consumable : CostObject
    {
      
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string DerivedName { get; set; }
        public int DerivedAmount { get; set; }
 
        bool tryFindDerived = false;

        private Consumable derivedConsumable;

        [IgnoreCodeFirst]
        public Consumable DerivedConsumable
        {
            get
            {
                if (!tryFindDerived)
                {
                    derivedConsumable = DataLayer.Instance.GetTable<Consumable>().Where(y => y.Name == DerivedName).SingleOrDefault();
                    tryFindDerived = true;
                }
                return derivedConsumable;
            }
        }
        public static Consumable GetByName(string name)
        {
            return DataLayer.Instance.GetTable<Consumable>().Where(y => y.Name == name).SingleOrDefault();
        }

        public static Consumable GetById(int id)
        {
            return DataLayer.Instance.GetTable<Consumable>().Where(y => y.Id == id).SingleOrDefault();
        }


#if !BACKOFFICE
        public string DisplayName
        {
            get
            {
                return this.Name;
                //return commonscripts.resourcemanager.ResourceManager.GetValue("Consumable." + this.Name + ".Name");
            }
        }

        public string Description
        {
            get
            {
                return this.Name;
                //return commonscripts.resourcemanager.ResourceManager.GetValue("Consumable." + this.Name + ".Description");
            }
        }
#endif
    }
}
