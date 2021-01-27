#if !BACKOFFICE
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.Protocol;
using MetaLoop.Common.PlatformCommon.Settings;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.Common.PlatformCommon.Data.Schema;

namespace MetaLoop.Common.PlatformCommon.GameManager
{
    public class RoutineTasksManager
    {
        public bool Running { get; set; }

        private CloudScriptMethod methodPlayerStatus;

        private Timer timerPlayerStatus;
        private int timerPlayerStatus_Interval = 60000;

        private Timer timerServerStatus;
        private int timerServerStatus_Interval = 45000;

        private Timer timerInboxStatus;
        private int timerInboxStatus_Interval = 50000;

        public Action OnInboxUpdate;
        public Action OnPlayerStatusUpdate;
        public Action OnEventManagerUpdate;

        private static RoutineTasksManager instance;
        public static RoutineTasksManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RoutineTasksManager();
                }
                return instance;
            }
        }

        private bool isInit = false;

        private MetaLoopGameManager metaLoopGameManager;
        public void Init(MetaLoopGameManager metaLoopGameManager)
        {
            if (!isInit)
            {
                this.metaLoopGameManager = metaLoopGameManager;

                methodPlayerStatus = new CloudScriptMethod();
                methodPlayerStatus.Method = "PlayerStatus";
                methodPlayerStatus.IgnoreError = true;

                timerPlayerStatus = new Timer(timerPlayerStatus_Interval);
                timerPlayerStatus.Elapsed += TimerStatusCheck_Elapsed;
                timerPlayerStatus.Enabled = true;

                timerServerStatus = new Timer(timerServerStatus_Interval);
                timerServerStatus.Elapsed += timerServerStatus_Elapsed;
                timerServerStatus.Enabled = true;

                timerInboxStatus = new Timer(timerInboxStatus_Interval);
                timerInboxStatus.Elapsed += timerInboxStatus_Elapsed;
                timerInboxStatus.Enabled = true;

                isInit = true;
            }

        }

        private void timerInboxStatus_Elapsed(object sender, ElapsedEventArgs e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => CheckForInboxMessages());
        }

        private void timerServerStatus_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Running) return;
            GetTitleDataRequest getTitleDataRequest = new GetTitleDataRequest();
            getTitleDataRequest.Keys = new List<string>() { MetaStateSettings._TitleDataKey_ServerInfo, MetaStateSettings._TitleDataKey_EventManager };
            UnityMainThreadDispatcher.Instance().Enqueue(() => PlayFabClientAPI.GetTitleData(getTitleDataRequest, GetTitleDataServerStatus_Completed, (PlayFabError obj) => GetTitleDataServerStatus_Completed(null)));
        }
        public void GetTitleDataServerStatus_Completed(GetTitleDataResult obj)
        {
            if (!Running) return;
            this.metaLoopGameManager.ReadTitleData(obj);
        }

        private bool CheckInternetConnection()
        {
            return true;
        }
        public void Stop()
        {
            if (isInit)
            {
                timerPlayerStatus.Enabled = false;
                timerServerStatus.Enabled = false;
                timerInboxStatus.Enabled = false;
                Running = false;
            }
        }

        public void Start()
        {
            timerPlayerStatus.Enabled = true;
            timerServerStatus.Enabled = true;
            timerInboxStatus.Enabled = true;
            Running = true;
        }

        private void TimerStatusCheck_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Running) return;
            UnityMainThreadDispatcher.Instance().Enqueue(() => PlayFabManager.Instance.InvokeBackOffice(methodPlayerStatus, OnPlayerStatusReceived));
        }
        private void OnSyncInboxCompleted(GetUserDataResult result)
        {
            UnityEngine.Debug.Log("ROUTINE TASK MANAGER - INBOX RECEIVED");
            if (!Running) return;
            if (result != null && result.Data.ContainsKey(MetaStateSettings._DataKey_Inbox))
            {
                List<StateInboxMessage> inboxMessages = JsonConvert.DeserializeObject<List<StateInboxMessage>>(result.Data[MetaStateSettings._DataKey_Inbox].Value);
                bool hasNewMail = SyncInboxMessages(inboxMessages);

                if (OnInboxUpdate != null)
                    UnityMainThreadDispatcher.Instance().Enqueue(() => OnInboxUpdate.Invoke());
            }
        }
        public void CheckForInboxMessages()
        {
            List<string> keys = new List<string>() { MetaStateSettings._DataKey_Inbox };
            var request = new GetUserDataRequest { Keys = keys };
            PlayFabClientAPI.GetUserReadOnlyData(request, OnSyncInboxCompleted, (PlayFabError error) => OnSyncInboxCompleted(null));
        }

        private void OnPlayerStatusReceived(CloudScriptResponse arg1, CloudScriptMethod arg2)
        {
            if (!Running) return;
            UnityEngine.Debug.Log("ROUTINE TASK MANAGER - PLAYER STATUS RECEIVED");

            if (arg1.ResponseCode == ResponseCode.Success)
            {
                if (arg1.Params.ContainsKey("EnergyBalance"))
                {
                    int newBalance = Convert.ToInt32(arg1.Params["EnergyBalance"]);
                    MetaDataStateBase.Current.Consumables.SetConsumableAmount(Consumable.GetByName(MetaStateSettings._EnergyId), newBalance);
                }

                MetaDataStateBase.Current.SyncLoginCalendar();

                if (arg1.Params.ContainsKey("UniqueId") && !string.IsNullOrEmpty(arg1.Params["UniqueId"]))
                {
                    if (arg1.Params["UniqueId"] != UnityEngine.SystemInfo.deviceUniqueIdentifier)
                    {
                        this.metaLoopGameManager.ShowUnavailableMessage(GameUnavailableMessageType.LOGIN_MISMATCH);
                    }
                }

                if (arg1.Params.ContainsKey("ApplyDailyReset"))
                {
                    if (arg1.Params["ApplyDailyReset"].ToLower() == "true")
                    {
                        MetaDataStateBase.Current.ApplyDailyReset();
                    }
                }

                if (OnPlayerStatusUpdate != null)
                    UnityMainThreadDispatcher.Instance().Enqueue(() => OnPlayerStatusUpdate.Invoke());
            }

        }


        public void AddMessageToInbox(StateInboxMessage message)
        {
            SyncInboxMessages(new List<StateInboxMessage>() { message });
        }


        public bool SyncInboxMessages(List<StateInboxMessage> fromServer)
        {
            List<string> newMessages = new List<string>();
            bool newMail = false;
            foreach (StateInboxMessage message in fromServer)
            {
                if (GameData.Current.InboxMessages.Where(y => y.MessageId == message.MessageId).Count() == 0)
                {
                    GameData.Current.InboxMessages.Add(message);
                    newMessages.Add(message.MessageId);
                    newMail = true;
                }
            }

            GameData.Current.InboxMessages.Where(y => y.ExpireOn < DateTime.UtcNow).ToList().ForEach(y => GameData.Current.InboxMessages.Remove(y));
            GameData.Save();

            return newMail;
        }

    }

}

#endif