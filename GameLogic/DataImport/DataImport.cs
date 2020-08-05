﻿using MetaLoop.Common.DataEngine;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using MetaLoopDemo.Meta.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;

namespace MetaLoop.GameLogic.DataImport
{
    public class DataImport
    {

        public static void ImportData(DataSet data, ManualResetEvent resetEvent)
        {
            Console.WriteLine("Importing data...");

            ImportDataStatus.Reset();

            CodeFirtsImport.ObjectsMemoryCache = new Dictionary<string, List<object>>();

            ImportCustomData(data, false);

            CodeFirtsImport.ImportAutoGeneratedObjects(data, Schema.CodeFirstTypes, false);

            if (MetaDataImportExeption.SchemaErrors.Count > 0)
            {
                MetaDataImportExeption.SchemaErrors.ForEach(y => Console.WriteLine(y));
            }
            else
            {

                DataVersion dataVersioRow = DataLayer.Instance.Connection.Table<DataVersion>().FirstOrDefault();
                int currentVersion = dataVersioRow == null ? 0 : dataVersioRow.Version;


                Console.WriteLine("Schema validated with no errors.");

                foreach (Type t in Schema.CodeFirstTypes)
                {
                    string dropTableScript = string.Format("DROP Table {0};", t.Name);
                    TableClassCodeFirstGenerator codeGen = new TableClassCodeFirstGenerator(t);
                    string createTableScript = codeGen.CreateTableScript();

                    try
                    {
                        int resultDrop = DataLayer.Instance.Connection.Execute(dropTableScript);
                    }
                    catch
                    {
                    }

                    int resultCreate = DataLayer.Instance.Connection.Execute(createTableScript);
                }


                CodeFirtsImport.ObjectsMemoryCache = new Dictionary<string, List<object>>();

                ImportCustomData(data, true);

                CodeFirtsImport.ImportAutoGeneratedObjects(data, Schema.CodeFirstTypes, true);

                dataVersioRow = new DataVersion() { Version = currentVersion + 1 };
                DataLayer.Instance.Connection.Insert(dataVersioRow);


                ImportDataStatus.WriteStatus("Updating Database Version to " + dataVersioRow.Version.ToString() + "...");
                DataLayer.Instance.Connection.Update(dataVersioRow);

                ImportDataStatus.WriteStatus("Import completed! Bravo Soldier! Total Import time: " + (ImportDataStatus.TotalSeconds / 60).ToString() + " min.");

                //if (Main.IsDatabaseInRam)
                //{
                //    try
                //    {
                //        File.Copy(Main.RamDbPath, Main.DbPath, true);
                //        WriteStatus("Unloaded database from RAM to DISK...Success");
                //    }
                //    catch
                //    {
                //        WriteStatus("ERROR: Could not unload databse from RAM to DISK...");
                //    }
                //}

                DataLayer.Instance.Kill();
            }

            resetEvent.Set();
        }



        public static void ImportCustomData(DataSet data, bool updateDatabase)
        {

        }
    }
}
