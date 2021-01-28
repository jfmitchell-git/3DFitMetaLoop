using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using System;
using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Settings;
using UnityEngine;
using System.IO;
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

#if !BACKOFFICE
        public void InitFromStreamingAssets(string DatabaseName)
        {
            var dbPath = "";

#if UNITY_EDITOR
            dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
#else
            // check if file exists in Application.persistentDataPath
            var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);
             dbPath = filepath;
            if (!File.Exists(filepath))
            {
                Debug.Log("Database not in Persistent path");
                // if it doesn't ->
                // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
           
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
		var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
		
#elif UNITY_STANDALONE_OSX
		var loadDb = Application.dataPath + "/Resources/Data/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
#else
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                                                                                         // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#endif

                Debug.Log("Database written");



            }
        
            

#endif

            Connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly | SQLiteOpenFlags.Create);

            try
            {
                DataVersion = this.GetTable<DataVersion>().First();
            }
            catch { }
        }
#endif


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
                database = MetaLoop.Common.PlatformCommon.Utils.Path.GetStreamingAssetsPersistantPath(database);
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



        public List<System.Object> GetTable(Type type)
        {
            KeyValuePair<Type, object> result = CachedObjects.Where(y => y.Key == type).SingleOrDefault();
            if (!result.Equals(new KeyValuePair<Type, object>()))
            {
                return (List<System.Object>)result.Value;

            }
            else
            {
                List<System.Object> data = Connection.Table<System.Object>(type).ToList();
                CachedObjects.Add(type, data);
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

