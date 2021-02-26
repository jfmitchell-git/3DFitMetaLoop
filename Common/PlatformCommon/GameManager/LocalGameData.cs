#if !BACKOFFICE
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.Common.PlatformCommon.UserProfile;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;



namespace MetaLoop.Common.PlatformCommon.GameManager
{
    public enum LoginType
    {
        Undefined,
        Device,
        GameService,
        Facebook,
    }

    [Serializable]
    public partial class GameData
    {
        
        public static Action SaveCallBack { get; set; }


        public delegate void OnGameDataReadyEvent(bool isNewUserProfile = false);
        [field: NonSerialized]
        public static event OnGameDataReadyEvent OnGameDataReady;

        public List<StateInboxMessage> InboxMessages { get; set; }

        [field: NonSerialized]
        public EventManagerState EventManagerState;

        public Dictionary<string, int> RemoteAssetManagerState { get; set; }
        public bool NotificationEnabled { get; set; }

        public LoginType LoginType { get; set; }

        public GameData()
        {

        }
        public void OnUserProfileLoaded()
        {
            if (InboxMessages == null)
            {
                InboxMessages = new List<StateInboxMessage>();
            }
            if (RemoteAssetManagerState == null)
            {
                RemoteAssetManagerState = new Dictionary<string, int>();
            }
        }

        private static bool initialized = false;
        private static GameData current;
        public static GameData Current
        {
            get
            {
                if (current == null)
                {
                    Debug.Log("GameData is null. Make sure to call Load() before using current data.");
                    //throw new NotSupportedException("GameData is null. Make sure to call Load() before using current data.");
                }
                return current;
            }

        }
        public static void Load()
        {
            if (!initialized)
            {
                UserProfileManager.Instance.OnUserProfileEvent += Instance_OnUserProfileEvent;
                initialized = true;
            }

            UserProfileManager.Instance.Load();
        }

        public static void Save()
        {
            UserProfileManager.Instance.SaveLocal();

            if (SaveCallBack != null)
            {
                MetaDataStateBase.Current.Version++;
                SaveCallBack.Invoke();
            }
        }

        private static void Instance_OnUserProfileEvent(UserProfileEvent e)
        {

            Debug.Log("OnUserProfileEvent!!!" + e.EventType);

            switch (e.EventType)
            {
                case UserProfileEventType.UserProfileCreated:
                    //Must create new game data. 
                    current = new GameData();
                    current.OnUserProfileLoaded();

                    UserProfileManager.Instance.UserProfileData.GameData = current;
                    UserProfileManager.Instance.SaveLocal();
                    if (OnGameDataReady != null) OnGameDataReady(true);
                    break;

                case UserProfileEventType.UserProfileLoaded:
                    current = JsonConvert.DeserializeObject<GameData>(UserProfileManager.Instance.UserProfileData.GameData.ToString());
                    current.OnUserProfileLoaded();
                    if (OnGameDataReady != null) OnGameDataReady();
                    break;

                case UserProfileEventType.UserProfileError:
                    Debug.Log("ERROR Instance_OnUserProfileEvent");
                    return;
                    //Do nothing, UserProfileManager will backup the corrupted file and create a new profile.
                    break;

            }
        }
    }
}
#endif