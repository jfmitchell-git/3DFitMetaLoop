using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using System;
using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Settings;
#if !BACKOFFICE
using MetaLoop.Common.PlatformCommon.Utils;
#endif


namespace MetaLoop.Common.PlatformCommon
{
    public class DataLayer
    {
        private static DataLayer dataLayer = null;

        public DataVersion DataVersion { get; set; }

        /// <summary>
        /// Gets the instance. Property to access Singleton object for database connection and data layer. 
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static DataLayer Instance
        {
            get
            {
                if (dataLayer == null)
                {
                    dataLayer = new DataLayer();
                }
                return dataLayer;
            }
        }

        public SQLiteConnection Connection { get; set; }


        /// <summary>
        /// Initializes the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        public void Init(string database = "")
        {
            if (database == "")
            {

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
                database = Path.GetStreamingAssetsPersistantPath(database);
                Connection = new SQLiteConnection(database, SQLiteOpenFlags.ReadOnly, true);
#endif
            }
            else
            {
                Connection = new SQLiteConnection(database, SQLiteOpenFlags.ReadOnly, true);
            }

            try
            {
                DataVersion = this.GetTable<DataVersion>().First();
            }
            catch { }

            //DataVersion = this.GetTable<DataVersion>().First();
        }

        public void Init(string database, bool readWrite)
        {
            Connection = new SQLiteConnection(database, SQLiteOpenFlags.ReadWrite, true);


        }

        public static bool TestDatabase(string databasePath)
        {
            try
            {
                SQLiteConnection connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
                connection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<Type, object> CachedObjects = new Dictionary<Type, object>();

        public List<T> GetTable<T>() where T : new()
        {


            KeyValuePair<Type, object> result = CachedObjects.Where(y => y.Key == typeof(T)).SingleOrDefault();
            if (!result.Equals(new KeyValuePair<Type, object>()))
            {
                return (List<T>)result.Value;

            }
            else
            {
                List<T> data = Connection.Table<T>().ToList();
                CachedObjects.Add(typeof(T), data);
                return data;
            }
        }

        public void FlushCache()
        {
            CachedObjects.Clear();
        }

        public void Kill()
        {
            if (Connection != null) Connection.Close();
            FlushCache();
            dataLayer = null;
        }

    }
}

