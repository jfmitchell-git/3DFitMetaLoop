using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon.Settings;
using SQLite4Unity3d;
using System;
using System.Linq;


namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    [Serializable]
    public abstract partial class Consumable : CostObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

        public static Consumable GetByName(string name)
        {
            return DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(Consumable)]).Cast<Consumable>().Where(y => y.Name == name).SingleOrDefault();
        }
        public static Consumable GetById(int id)
        {
            //var okFuck = DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(Consumable)]);
            return DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(Consumable)]).Cast<Consumable>().Where(y => y.Id == id).SingleOrDefault();
        }
        public static T GetById<T>(int id) where T : new()
        {
            return (T)DataLayer.Instance.GetTable<T>().Where(y => ((Consumable)(object)y).Id == id).SingleOrDefault();
        }
        public static T GetByName<T>(string name) where T : new()
        {
            return (T)DataLayer.Instance.GetTable<T>().Where(y => ((Consumable)(object)y).Name == name).SingleOrDefault();
        }


#if !BACKOFFICE
        public virtual string DisplayName
        {
            get
            {
                return this.Name;
                //return commonscripts.resourcemanager.ResourceManager.GetValue("Consumable." + this.Name + ".Name");
            }
        }

        public virtual string Description
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
