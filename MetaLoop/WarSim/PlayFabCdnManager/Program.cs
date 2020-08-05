﻿
using GameLogic;
using MetaBackend.Common.Data;
using MetaBackend.Common.DataLayer;
using MetaBackend.Common.Protocol;
using MetaBackend.Common.RemoteAssets;
using MetaBackend.Common.Server;
using MetaConfiguration;
using Newtonsoft.Json;
using PlayFab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace PlayFabCdnManager
{
    class Program
    {
        public static string BackOfficeStatusUrl;
        static void Main(string[] args)
        {
            bool isCdnOnlyMode = false;

            List<string> allArgs = args == null ? new List<string>() : args.ToList();
            PlayFab.PlayFabSettings.staticSettings.DeveloperSecretKey = MetaConfiguration.PlayFabSettings.DeveloperSecretKey;
            PlayFab.PlayFabSettings.staticSettings.TitleId = MetaConfiguration.PlayFabSettings.TitleId;

            BackOfficeStatusUrl = AzureSettings.StatusUrl;

            if (allArgs.Contains("-cdnonly"))
            {
                isCdnOnlyMode = true;
                Console.WriteLine("Found -cdnonly switch, will only sync files...");
            }


            if (allArgs.ElementAtOrDefault(0) != null && allArgs.ElementAtOrDefault(0) == "-appversion")
            {
                string version = allArgs.ElementAtOrDefault(1);
                string path = allArgs.ElementAtOrDefault(2);

                if (!string.IsNullOrEmpty(version) && !string.IsNullOrEmpty(path))
                {
                    File.WriteAllText(path, version);
                }

                return;
            }

            if (allArgs.ElementAtOrDefault(0) != null && allArgs.ElementAtOrDefault(0) == "-status")
            {
                bool online = allArgs.ElementAtOrDefault(1) == "online" ? true : false;
                string seconds = allArgs.ElementAtOrDefault(2);

                string version = allArgs.ElementAtOrDefault(3);

                ChangeServerStatus(online, seconds, version);
                return;
            }


            var pathArg = allArgs.Where(y => y.StartsWith("-path:")).SingleOrDefault();
            if (pathArg != null)
            {
                ProjectSettings.BaseUnityFolder = pathArg.Replace("-path:", string.Empty);
            }


            var currentManifestResult = PlayFab.PlayFabServerAPI.GetTitleDataAsync(new PlayFab.ServerModels.GetTitleDataRequest() { Keys = new List<string>() { MetaStateSettingsCore.TitleDataKey_CdnManifest } }).GetAwaiter().GetResult();

            Console.WriteLine("Fetching CDN content...");
            AssetManifest currentManifest = null;
            if (currentManifestResult.Result.Data.ContainsKey(MetaStateSettingsCore.TitleDataKey_CdnManifest) && !string.IsNullOrEmpty(currentManifestResult.Result.Data[MetaStateSettingsCore.TitleDataKey_CdnManifest]))
            {
                currentManifest = JsonConvert.DeserializeObject<AssetManifest>(currentManifestResult.Result.Data[MetaStateSettingsCore.TitleDataKey_CdnManifest]);
            }
            else
            {
                currentManifest = new AssetManifest();
            }

            Console.WriteLine("Loading local files on " + ProjectSettings.BaseUnityFolder + ProjectSettings.DownloadableFolderPath);
            List<AssetFileInfo> allFiles = LocalFileHandler.GetLocalDownloadableFiles();

           var allCdnContent = PlayFabAdminAPI.GetContentListAsync(new PlayFab.AdminModels.GetContentListRequest()).GetAwaiter().GetResult();

          

            int currentDataVersion;
            AssetFileInfo previousDbFileInfo = null;
            if (!isCdnOnlyMode)
            {
                string databasePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\" + MetaStateSettingsCore.DatabaseName;

                DataLayer.Instance.Init(databasePath);
                currentDataVersion = DataLayer.Instance.GetTable<DataVersion>().First().Version;
                DataLayer.Instance.Connection.Close();

                var databaseFileInfo = new AssetFileInfo(databasePath, MetaStateSettingsCore.AssetManagerStartupFolder + Path.GetFileName(databasePath) + MetaStateSettingsCore.AssetManagerVersionString + currentDataVersion.ToString(), true);

                if (currentManifest.DataVersion != currentDataVersion)
                {
                    databaseFileInfo.ForceSync = true;
                }

                allFiles.Add(databaseFileInfo);
            }
            else
            {
                currentDataVersion = currentManifest.DataVersion;
                string dbFilename = MetaStateSettingsCore.AssetManagerStartupFolder + MetaStateSettingsCore.DatabaseName + MetaStateSettingsCore.AssetManagerVersionString + currentDataVersion.ToString();
                previousDbFileInfo = currentManifest.Files.Where(y => y.RelativeName == dbFilename).SingleOrDefault();

                if (previousDbFileInfo != null)
                {
                    Console.WriteLine("Will re-import " + previousDbFileInfo.RelativeName + " to manifest...");
                }

            }


            List<AssetFileInfo> allFilesToBeSynced = new List<AssetFileInfo>();

            foreach (var file in allFiles)
            {
                var cdnFile = allCdnContent.Result.Contents.Where(y => y.Key == file.RelativeName).SingleOrDefault();
                var playFabEntry = currentManifest.GetFile(file.RelativeName);
                if (playFabEntry == null || cdnFile == null || playFabEntry.Size != file.Size || file.ForceSync)
                {
                    allFilesToBeSynced.Add(file);
                    Console.WriteLine(file.RelativeName + " must be synced.");
                }
            }

            if (allFilesToBeSynced.Count > 0)
            {
                Console.WriteLine(string.Format("{0} file(s) must be synced. Press 1 to sync, 0 to cancel:", allFilesToBeSynced.Count.ToString()));


                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine("Starting CDN Sync...");
                        bool hasError = false;
                        int i = 1;
                        foreach (var file in allFilesToBeSynced)
                        {
                            Console.WriteLine(string.Format("Uploading file {0} of {1}. ({2})", i.ToString(), allFilesToBeSynced.Count.ToString(), file.RelativeName));
                            if (!UploadFileToCDN(file.RelativeName, file.ToArray()))
                            {
                                hasError = true;
                                Console.WriteLine(string.Format("ERROR UPLOADING {0} of {1}. ({2})", i.ToString(), allFilesToBeSynced.Count.ToString(), file.RelativeName));
                                break;
                            }

                            i++;
                        }

                        if (!hasError)
                        {
                            Console.WriteLine("All files uploaded...");
                            Console.WriteLine("Creating and uploading manifest...");

                            currentManifest.ManifestVersion++;
                            currentManifest.DataVersion = currentDataVersion;
                            currentManifest.Files = new List<AssetFileInfo>();
                            currentManifest.Files.AddRange(allFiles);

                            if (previousDbFileInfo != null)
                            {
                                currentManifest.Files.Add(previousDbFileInfo);
                            }

                            var updateData = PlayFabServerAPI.SetTitleDataAsync(new PlayFab.ServerModels.SetTitleDataRequest() { Key = MetaStateSettingsCore.TitleDataKey_CdnManifest, Value = currentManifest.ToJson() }).GetAwaiter().GetResult();
                            if (updateData.Error == null)
                            {
                                Console.WriteLine("Mafinest uploaded.");
                                Console.WriteLine("CDN Sync is COMPLETED.");
                            }
                            else
                            {
                                Console.WriteLine("Mafinest uploaded failed.");
                            }

                        }
                        break;
                }
            }
            else
            {
                Console.WriteLine("No files to be synced!");
            }

            //Console.WriteLine("Bruh, press any key to exit program.");
            //Console.ReadKey();
        }

        public static bool UploadFileToCDN(string relativeFileName, byte[] content)
        {
            return false;
        }

        public static void ChangeServerStatus(bool available, string secondsToWait, string versionId = null)
        {

            int waitingTime = Convert.ToInt32(secondsToWait);
            var currentStatus = PlayFab.PlayFabServerAPI.GetTitleDataAsync(new PlayFab.ServerModels.GetTitleDataRequest() { Keys = new List<string>() { MetaStateSettingsCore.TitleDataKey_ServerInfo } }).GetAwaiter().GetResult();
            ServerInfo serverInfo = null;
            if (currentStatus.Result.Data.ContainsKey(MetaStateSettingsCore.TitleDataKey_ServerInfo))
            {
                serverInfo = JsonConvert.DeserializeObject<ServerInfo>(currentStatus.Result.Data[MetaStateSettingsCore.TitleDataKey_ServerInfo]);
            }
            else
            {
                Console.WriteLine("ERROR, COULD NOT READ ServerInfo!");
                Environment.Exit(0);
                return;
            }

            serverInfo.ServerStatus = (available || waitingTime == 0) ? ServerStatus.Online : ServerStatus.Offline;
            serverInfo.MaintenanceMessage = (available || waitingTime == 0) ? string.Empty : GameUnavailableMessageType.MAINTENANCE.ToString();
            serverInfo.CacheVersion++;

            if (!string.IsNullOrEmpty(versionId))
            {
                serverInfo.AppVersion = versionId;
            }


            if (available)
            {
                Console.WriteLine("Waking up Azure....");

                WakeUpAzureRequests(10);

                WaitFor(Convert.ToInt32(secondsToWait));

                Console.WriteLine("Setting server Online...");

                var updateData = PlayFabServerAPI.SetTitleDataAsync(new PlayFab.ServerModels.SetTitleDataRequest() { Key = MetaStateSettingsCore.TitleDataKey_ServerInfo, Value = serverInfo.ToJson() }).GetAwaiter().GetResult();
                if (updateData.Error == null)
                {
                    Console.WriteLine("SUCCESS");
                }


            }
            else
            {
                Console.WriteLine("Shutting Down Servers...");
                var updateData = PlayFabServerAPI.SetTitleDataAsync(new PlayFab.ServerModels.SetTitleDataRequest() { Key = MetaStateSettingsCore.TitleDataKey_ServerInfo, Value = serverInfo.ToJson() }).GetAwaiter().GetResult();
                if (updateData.Error == null)
                {
                    Console.WriteLine("SUCCESS");
                    WaitFor(Convert.ToInt32(secondsToWait));
                }
            }

        }

        public static void WakeUpAzureRequests(int numOfRequest)
        {
            for (int i = 0; i < numOfRequest; i++)
            {
                try
                {
                    WebRequest request = WebRequest.Create(BackOfficeStatusUrl);
                    request.Credentials = CredentialCache.DefaultCredentials;
                    WebResponse response = request.GetResponse();

                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                        Console.WriteLine("REQUEST #" + i.ToString() + " " + responseFromServer);
                    }
                    response.Close();

                }
                catch
                {
                    Console.WriteLine("REQUEST #" + i.ToString() + " Error...");
                }



            }

        }

        private static void WaitFor(int WaitForSeconds)
        {


            int incrementSteps = 10;
            Console.WriteLine("Waiting for " + WaitForSeconds.ToString() + " Seconds...");
            float secondsLefts = WaitForSeconds;

            while (secondsLefts > 0)
            {
                float waitFor;

                if (secondsLefts > incrementSteps)
                {
                    waitFor = Convert.ToInt32(WaitForSeconds) / incrementSteps;
                }
                else
                {
                    waitFor = 1;
                }



                System.Threading.Thread.Sleep((int)waitFor * 1000);
                secondsLefts -= waitFor;
                Console.WriteLine(((int)secondsLefts).ToString() + " seconds left...");
            }
        }

    }
}
