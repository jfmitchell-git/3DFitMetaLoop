#if !BACKOFFICE
using DG.Tweening;
using MetaLoop.Common.PlatformCommon.GameManager;
using MetaLoop.Common.PlatformCommon.HttpClient;
using MetaLoop.Common.PlatformCommon.Settings;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MetaLoop.Common.PlatformCommon.RemoteAssets
{
    public class RemoteAssetInfo
    {
        public AssetFileInfo File;
        public int Version;
        public bool RequireDownload;

    }

    public class RemoteAssetDownloadInfo
    {

        public delegate void OnStatusUpDateEvent(RemoteAssetDownloadInfo remoteAssetDownloadInfo);
        public event OnStatusUpDateEvent OnStatusUpdate;

        public RemoteAssetInfo Asset { get; set; }
        public int Progress { get; set; }
        public bool HasError { get; set; }
        public bool IsCompleted { get; set; }

        public void RaiseStatusEvent()
        {
            if (OnStatusUpdate != null)
            {
                OnStatusUpdate.Invoke(this);
            }
        }


    }


    public class RemoteAssetsManager
    {
        private int lastManifestVersion = 0;
        public delegate void OnStatusUpDateEvent(RemoteAssetDownloadInfo remoteAssetDownloadInfo);
        public event OnStatusUpDateEvent OnStatusUpdate;

        private string currentProgress = string.Empty;

        public string CurrentProgress
        {
            get
            {
                return currentProgress;
            }
        }

   



        private Action StartupFilesOnCompleteCallBack;
        private Action GameReadyFilesOnCompleteCallBack;
        private List<RemoteAssetInfo> currentStartupFilesQueue;
        private List<RemoteAssetInfo> currentGameReadyFilesQueue;
        public List<RemoteAssetInfo> StartupFiles;
        public List<RemoteAssetInfo> OnDemandFiles;
        public List<RemoteAssetInfo> OnGameReadyFiles;

        public bool ShowGameReadyAssetsProgress = false;
        private long totalGameReadySizeMissing = 0;
        private long totalGameReadySizeDownloaded = 0;
        private long interimGameReadySizeDownloaded = 0;

        private bool isStartupQueueRunning = false;
        private bool isGameReadyQueueRunning = false;


        private int CurrentGameAssetDownloadProgress
        {
            get
            {
                int totalProgress = 0;

                try
                {
                    totalProgress = (int)Math.Ceiling(((totalGameReadySizeDownloaded + interimGameReadySizeDownloaded) / (float)totalGameReadySizeMissing) * 100f);
                    if (totalProgress > 100) totalProgress = 100;
                }
                catch { }
                return totalProgress;
            }
        }

        public bool IsGameReadyAssetsDownloading
        {
            get
            {
                return OnGameReadyFiles.Where(y => y.RequireDownload).Count() > 0;
            }
        }
        public bool IsMissingGameReadyAssets()
        {
            bool result = OnGameReadyFiles.Where(y => y.RequireDownload).Count() > 0;
            if (result) ShowGameReadyAssetsProgress = true;
            return result;
        }

        private static RemoteAssetsManager instance = null;
        public static RemoteAssetsManager Instance
        {
            get
            {

                return instance;
            }
        }

        public AssetManifest AssetManifest { get; set; }

        public int DataVersion
        {
            get
            {
                return AssetManifest.DataVersion;
            }
        }



        public static void Init(AssetManifest manifest)
        {
            if (instance == null)
            {
                instance = new RemoteAssetsManager(manifest);
            }
            else
            {
                instance.UpdateManifest(manifest);
            }
        }


        private void UpdateManifest(AssetManifest manifest)
        {
            //if its not a NEW manifest, ignore it.
            if (lastManifestVersion > 0 && manifest.ManifestVersion <= lastManifestVersion)
            {
                OnStartupFilesCompleted();
            }
            else
            {
                this.AssetManifest = manifest;
                this.StartupFiles = new List<RemoteAssetInfo>();
                this.OnDemandFiles = new List<RemoteAssetInfo>();
                this.OnGameReadyFiles = new List<RemoteAssetInfo>();
                ReadManifest();
            }
        }

        public RemoteAssetsManager(AssetManifest manifest)
        {
            UpdateManifest(manifest);
        }

        private void ReadManifest()
        {
            List<RemoteAssetInfo> allFilesToBeSynced = new List<RemoteAssetInfo>();

            string remoteAssetsPath = Application.persistentDataPath + MetaStateSettings._RemoteAssetsPersistantName;

            if (!Directory.Exists(remoteAssetsPath)) Directory.CreateDirectory(remoteAssetsPath);


            var allFiles = this.AssetManifest.Files.ToList();

#if UNITY_IOS
            allFiles.Where(y => y.RelativeName.Contains("Android/")).ToList().ForEach(y => this.AssetManifest.Files.Remove(y));
#endif

#if UNITY_ANDROID
            allFiles.Where(y => y.RelativeName.Contains("iOS/")).ToList().ForEach(y => this.AssetManifest.Files.Remove(y));
#endif

            foreach (AssetFileInfo file in this.AssetManifest.Files)
            {
                bool mustDownloadFile = false;
                string filename = file.RelativeName;
                int version = 1;
                string lastFilePart = "." + filename.Split('.').ToList().Last();

                if (lastFilePart.Contains(MetaStateSettings._AssetManagerVersionString))
                {
                    version = Convert.ToInt32(lastFilePart.Replace(MetaStateSettings._AssetManagerVersionString, string.Empty));
                    filename = filename.Replace(lastFilePart, string.Empty);
                    file.LocalRelativeName = filename;
                }

                string fullFilename = remoteAssetsPath + @"/" + filename;
                file.FileName = fullFilename;

                //Will always be false unless it was a versioned file.
                if (version > 1 && version > GetFileVersion(file.LocalRelativeName))
                {
                    mustDownloadFile = true;
                }


                if (!File.Exists(fullFilename))
                {
                    mustDownloadFile = true;
                    //UnityEngine.Debug.Log(" MUST DOWNLOAD FILE " + fullFilename + " DOES NOT EXIST.");
                }
                else
                {

#if !UNITY_EDITOR
                    FileInfo fileInfo = new FileInfo(fullFilename);
                    if (fileInfo.Length != file.Size)
                    {
                        UnityEngine.Debug.Log(" MUST DOWNLOAD FILE " + fullFilename + " " + fileInfo.Length.ToString() + " VS " + file.Size.ToString());
                        mustDownloadFile = true;
                    }
#endif

                }

                var newFileInfo = new RemoteAssetInfo() { File = file, RequireDownload = mustDownloadFile, Version = version };


                if (file.RelativeName.StartsWith(MetaStateSettings._AssetManagerStartupFolder))
                {
                    StartupFiles.Add(newFileInfo);
                }
                else if (file.RelativeName.StartsWith(MetaStateSettings._AssetManagerOnGameReady))
                {
                    OnGameReadyFiles.Add(newFileInfo);
                }
                else
                {
                    OnDemandFiles.Add(newFileInfo);
                }
            }

            lastManifestVersion = AssetManifest.ManifestVersion;
        }


        public void DownloadStartupAssets(Action OnComplete)
        {
            this.StartupFilesOnCompleteCallBack = OnComplete;
            currentStartupFilesQueue = StartupFiles.Where(y => y.RequireDownload).ToList();
            ProcessNextStartupFile(null);
        }

        public void DownloadGameReadyAssets(Action OnComplete)
        {
            this.GameReadyFilesOnCompleteCallBack = OnComplete;
            currentGameReadyFilesQueue = OnGameReadyFiles.Where(y => y.RequireDownload).ToList();

            totalGameReadySizeMissing = currentGameReadyFilesQueue.Sum(y => y.File.Size);
            totalGameReadySizeDownloaded = 0;

            ProcessNextGameReadyFile(null);
        }


        private void ProcessNextStartupFile(RemoteAssetInfo currentFile, bool reprocess = false)
        {
            RemoteAssetInfo fileToDownload = null;
            if (currentFile == null)
            {
                fileToDownload = currentStartupFilesQueue.FirstOrDefault();
            }
            else
            {
                if (reprocess)
                {
                    fileToDownload = currentStartupFilesQueue.ElementAtOrDefault(currentStartupFilesQueue.IndexOf(currentFile));
                }
                else
                {
                    fileToDownload = currentStartupFilesQueue.ElementAtOrDefault(currentStartupFilesQueue.IndexOf(currentFile) + 1);
                }
            }

            if (fileToDownload != null)
            {
                isStartupQueueRunning = true;
                var downloadInfo = DownloadAsset(fileToDownload);
                downloadInfo.OnStatusUpdate += ProcessNextStartupFile_OnStatusUpdate;
            }
            else
            {
                OnStartupFilesCompleted();
                GameData.Save();
            }

        }


        private void ProcessNextGameReadyFile(RemoteAssetInfo currentFile, bool reprocess = false)
        {
            RemoteAssetInfo fileToDownload = null;
            if (currentFile == null)
            {
                fileToDownload = currentGameReadyFilesQueue.FirstOrDefault();
            }
            else
            {
                if (reprocess)
                {
                    fileToDownload = currentGameReadyFilesQueue.ElementAtOrDefault(currentGameReadyFilesQueue.IndexOf(currentFile));
                }
                else
                {
                    fileToDownload = currentGameReadyFilesQueue.ElementAtOrDefault(currentGameReadyFilesQueue.IndexOf(currentFile) + 1);
                }
            }

            if (fileToDownload != null)
            {
                isGameReadyQueueRunning = true;
                var downloadInfo = DownloadAsset(fileToDownload);
                downloadInfo.OnStatusUpdate += ProcessOnGameReadyFile_OnStatusUpdate;
            }
            else
            {
                OnGameReadyFilesCompleted();
                GameData.Save();
            }

        }

        private void ProcessOnGameReadyFile_OnStatusUpdate(RemoteAssetDownloadInfo remoteAssetDownloadInfo)
        {
            if (remoteAssetDownloadInfo.IsCompleted)
            {
                ProcessNextGameReadyFile(remoteAssetDownloadInfo.Asset);
                remoteAssetDownloadInfo = null;
            }
            else if (remoteAssetDownloadInfo.HasError)
            {
                ProcessNextGameReadyFile(remoteAssetDownloadInfo.Asset, true);
                remoteAssetDownloadInfo = null;
            }
        }

        private void ProcessNextStartupFile_OnStatusUpdate(RemoteAssetDownloadInfo remoteAssetDownloadInfo)
        {
            if (remoteAssetDownloadInfo.IsCompleted)
            {
                ProcessNextStartupFile(remoteAssetDownloadInfo.Asset);
                remoteAssetDownloadInfo = null;
            }
            else if (remoteAssetDownloadInfo.HasError)
            {
                ProcessNextGameReadyFile(remoteAssetDownloadInfo.Asset, true);
                remoteAssetDownloadInfo = null;
            }
        }

        private void OnStartupFilesCompleted()
        {
            currentProgress = string.Empty;
            isStartupQueueRunning = false;
            if (StartupFilesOnCompleteCallBack == null) return;
            this.StartupFilesOnCompleteCallBack.Invoke();
            this.StartupFilesOnCompleteCallBack = null;

        }


        private void OnGameReadyFilesCompleted()
        {
            currentProgress = string.Empty;
            isGameReadyQueueRunning = false;
            if (GameReadyFilesOnCompleteCallBack == null) return;
            this.GameReadyFilesOnCompleteCallBack.Invoke();
            this.GameReadyFilesOnCompleteCallBack = null;
        }


        public RemoteAssetInfo GetRemoteAssetInfo(string filename)
        {

            var myFile = OnDemandFiles.Where(y => y.File.RelativeName == filename).SingleOrDefault();
            if (myFile != null)
                return myFile;

            return OnGameReadyFiles.Where(y => y.File.RelativeName == filename).SingleOrDefault();

            return StartupFiles.Where(y => y.File.RelativeName == filename).SingleOrDefault();
        }


        public RemoteAssetDownloadInfo DownloadAsset(RemoteAssetInfo asset)
        {
            if (asset == null) return null;
            RemoteAssetDownloadInfo downloadInfo = new RemoteAssetDownloadInfo();
            downloadInfo.Asset = asset;
            ProcessFile(downloadInfo);
            return downloadInfo;
        }



        public RemoteAssetDownloadInfo DownloadAsset(string filename)
        {
            return DownloadAsset(GetRemoteAssetInfo(filename));
        }

        private DateTime LastRequest = DateTime.MinValue;
        private void ProcessFile(RemoteAssetDownloadInfo downloadInfo)
        {
            GetContentDownloadUrlRequest request = new GetContentDownloadUrlRequest() { Key = downloadInfo.Asset.File.RelativeName };

            var timeSinceLastRequest = (DateTime.Now - LastRequest).TotalMilliseconds;
            float delay = 0f;
            if (timeSinceLastRequest < 1000)
            {
                delay = 0.6f;
            }

            UnityEngine.Debug.Log("GetContentDownloadUrl DELAY " + timeSinceLastRequest + " ms.");
            DOVirtual.DelayedCall(delay, () =>
                PlayFab.PlayFabClientAPI.GetContentDownloadUrl(request, (GetContentDownloadUrlResult r) => GetContentDownloadUrl_Completed(r, downloadInfo), (PlayFabError e) => GetContentDownloadUrl_Completed(null, downloadInfo)));

            LastRequest = DateTime.Now;
        }

        private void GetContentDownloadUrl_Completed(GetContentDownloadUrlResult result, RemoteAssetDownloadInfo downloadInfo)
        {

            if (result != null)
            {
                string urlToDownloadFile = result.URL;
                ProcessFileDownload(downloadInfo, urlToDownloadFile);
            }
            else
            {
                downloadInfo.Progress = 0;
                downloadInfo.HasError = true;
                downloadInfo.RaiseStatusEvent();
            }
        }

        private void ProcessFileDownload(RemoteAssetDownloadInfo downloadInfo, string url)
        {
            UnityWebRequestHandler.Instance.GetBodyFromHttpWithProgress(url, null, (UnityWebRequest r) => OnFileDownloaded(r, downloadInfo));
        }

        private void OnFileDownloaded(UnityWebRequest www, RemoteAssetDownloadInfo downloadInfo)
        {
            if (www.isNetworkError || www.isHttpError)
            {
                downloadInfo.Progress = 0;
                downloadInfo.HasError = true;
                downloadInfo.RaiseStatusEvent();
                Debug.Log(downloadInfo.Asset.File.FileName + " FILE DOWNLOAD ERROR!");
                www = null;
            }
            else
            {
                if (www.isDone)
                {
                    byte[] results = www.downloadHandler.data;
                    if (File.Exists(downloadInfo.Asset.File.FileName)) File.Delete(downloadInfo.Asset.File.FileName);
                    if (!Directory.Exists(Path.GetDirectoryName(downloadInfo.Asset.File.FileName))) Directory.CreateDirectory(Path.GetDirectoryName(downloadInfo.Asset.File.FileName));

                    File.WriteAllBytes(downloadInfo.Asset.File.FileName, results);
                    if (downloadInfo.Asset.Version > 1) IncrementFileVersion(downloadInfo.Asset.File.LocalRelativeName, downloadInfo.Asset.Version);
                    downloadInfo.IsCompleted = true;
                    downloadInfo.Progress = 100;

                    if (downloadInfo.Asset.File.FileName.ToLower().EndsWith(".zip"))
                    {
                        //ZipArchive zip = ZipFile.OpenRead(downloadInfo.Asset.File.FileName);


                        FileInfo fileInfo = new FileInfo(downloadInfo.Asset.File.FileName);



                        using (ZipArchive archive = ZipFile.OpenRead(downloadInfo.Asset.File.FileName))
                        {
                            string currentFileName = Path.GetFileName(downloadInfo.Asset.File.FileName);
                            string folderName = downloadInfo.Asset.File.FileName.Replace(@"\", @"/").Replace(currentFileName, "");


                            foreach (var entry in archive.Entries)
                            {
                                using (var entryStream = entry.Open())
                                {
                                    string finalFileName = folderName + entry.Name;
                                    //try
                                    //{
                                    if (File.Exists(finalFileName)) File.Delete(finalFileName);
                                    if (!File.Exists(finalFileName))
                                    {
                                        using (FileStream outputFileStream = new FileStream(finalFileName, FileMode.Create))
                                        {
                                            entryStream.CopyTo(outputFileStream);
                                        }
                                    }
                                    //}
                                    //catch {}

                                }
                            }
                        }
                    }

                    downloadInfo.RaiseStatusEvent();
                    downloadInfo.Asset.RequireDownload = false;
                    www = null;
                    totalGameReadySizeDownloaded += downloadInfo.Asset.File.Size;
                    interimGameReadySizeDownloaded = 0;



                    UpdateCurrentProgress(null, downloadInfo);

                }
                else
                {
                    //long totalLenght = Convert.ToInt64((www.downloadProgress * 100f) * www.downloadedBytes);
                    downloadInfo.Progress = Convert.ToInt32(www.downloadProgress * 100);
                    downloadInfo.RaiseStatusEvent();
                    UpdateCurrentProgress(www, downloadInfo);

                }
                Debug.Log(downloadInfo.Asset.File.FileName + " " + downloadInfo.Progress.ToString() + "%");


            }

        }

        private void UpdateCurrentProgress(UnityWebRequest www, RemoteAssetDownloadInfo downloadInfo)
        {
            //if (currentGameReadyFilesQueue != null && currentGameReadyFilesQueue.Contains(downloadInfo.Asset))
            //{
            //    interimGameReadySizeDownloaded = (www == null) ? 0 : Convert.ToInt64(www.downloadedBytes);

            //    if (ShowGameReadyAssetsProgress)
            //    {
            //        currentProgress = string.Format(ResourceManager.GetValue("Homesreen.Loading.DownloadingAssets"), CurrentGameAssetDownloadProgress);
            //    }
            //}
            //else if (currentStartupFilesQueue != null && currentStartupFilesQueue.Contains(downloadInfo.Asset))
            //{
            //    currentProgress = string.Format(ResourceManager.GetValue("Homesreen.Loading.DownloadingDatabase"), downloadInfo.Progress.ToString());
            //}
            //else
            //{
            //    currentProgress = string.Empty;
            //}
        }


        private int GetFileVersion(string key)
        {
            if (GameData.Current.RemoteAssetManagerState.ContainsKey(key))
            {
                return GameData.Current.RemoteAssetManagerState[key];
            }
            return 1;
        }


        private void IncrementFileVersion(string key, int version)
        {
            if (!GameData.Current.RemoteAssetManagerState.ContainsKey(key))
            {
                GameData.Current.RemoteAssetManagerState.Add(key, version);
            }
            GameData.Current.RemoteAssetManagerState[key] = version;
        }



    }
}
#endif