#if !BACKOFFICE
using DG.Tweening;
using dryginstudios.commonscripts.gameservices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using PlayFab.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.PlayFabClient
{

    public enum BackOfficeEnvironement
    {
        Prod,
        Staging,
        Dev
    }


    public enum OnDataMissMatchDetectedEventType
    {
        BackofficeError,
        HostUnreachable
    }
    public class CloudScriptStackMethod
    {
        public CloudScriptMethod CloudScriptMethod;
        public string StackId;
    }

    public class PlayFabManager : MonoBehaviour
    {

        public static bool IsImpersonating = false;
        public static string ImpersonateId = "";

        public void SetImpersonate(string id)
        {
#if UNITY_EDITOR
            ImpersonateId = id;
            IsImpersonating = !string.IsNullOrEmpty(id);
#endif
        }

        private bool isWorking = false;

        private bool IsFirtsLogin = true;

        public delegate void OnDataMissMatchDetectedEvent(OnDataMissMatchDetectedEventType type = OnDataMissMatchDetectedEventType.BackofficeError);
        public event OnDataMissMatchDetectedEvent OnDataMissMatchDetected;

        public PlayFabGroupManager GroupManager;

        public BackOfficeEnvironement BackOfficeEnvironement { get; set; }
        private const int retryCountMax = 2;

        public List<string> Segments = new List<string>();

        private List<CloudScriptStackMethod> cloudScriptStackMethods;

        private static PlayFabManager instance;
        public static PlayFabManager Instance
        {
            get
            {
                return instance;
            }
        }



        private EntityTokenResponse lastTokenResponse;

        public string GetContextToken()
        {
            if (lastTokenResponse == null || lastTokenResponse.TokenExpiration < DateTime.UtcNow)
            {
                if (OnDataMissMatchDetected != null) OnDataMissMatchDetected();
                return null;
            }
            return lastTokenResponse.EntityToken;
        }

        private PlayFab.ClientModels.EntityKey entityKey;
        public PlayFab.ClientModels.EntityKey CurrentEntity
        {
            get
            {
                if (lastTokenResponse == null || lastTokenResponse.TokenExpiration < DateTime.UtcNow)
                {
                    if (OnDataMissMatchDetected != null) OnDataMissMatchDetected();
                    return null;
                }



                if (entityKey == null)
                {

                    entityKey = new PlayFab.ClientModels.EntityKey { Id = PlayFabManager.Instance.lastTokenResponse.Entity.Id, Type = lastTokenResponse.Entity.Type };
                }
                return entityKey;
            }

        }

        public PlayFab.GroupsModels.EntityKey GetUserGroupEntity()
        {
            return new PlayFab.GroupsModels.EntityKey() { Id = CurrentEntity.Id, Type = CurrentEntity.Type };
        }


        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
                cloudScriptStackMethods = new List<CloudScriptStackMethod>();
                //StartCoroutine(TimerStack());
            }

        }

        private DateTime timerProcessStackLastTime = DateTime.Now;

        private DateTime timerProcessRealtimeLastTime = DateTime.Now;

        public List<Action> realtimeQueue = new List<Action>();


        public void Update()
        {

            if (DateTime.Now.Subtract(timerProcessStackLastTime).TotalSeconds > 3.7f)
            {
                TimerProcessStacks_Elapsed();
                timerProcessStackLastTime = DateTime.Now;
            }

            if (DateTime.Now.Subtract(timerProcessRealtimeLastTime).TotalMilliseconds > 100)
            {
                TimerProcessRealtime_Elapsed();
                timerProcessRealtimeLastTime = DateTime.Now;
            }


        }

        private void TimerProcessStacks_Elapsed()
        {
            if (isWorking)
            {
                //retry a bit faster if needed.
                timerProcessStackLastTime = DateTime.Now.AddMilliseconds(2500);
                return;
            }

            float callDelay = 0f;
            //UnityEngine.Debug.Log("--------PLAYFAB STACK TIMER GOOD ");
            if (cloudScriptStackMethods.Count > 0)
            {
                List<string> allStacks = cloudScriptStackMethods.Select(y => y.StackId).Distinct().ToList();

                foreach (string stackId in allStacks)
                {
                    List<CloudScriptMethod> allMethods = cloudScriptStackMethods.Where(y => y.StackId == stackId).Select(y => y.CloudScriptMethod).ToList();
                    allMethods.ForEach(y => y.Environement = BackOfficeEnvironement.ToString());

                    ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest() { FunctionName = "invokeBioIncBackOffice", FunctionParameter = allMethods };

                    StartCoroutine(DelayedCall(callDelay, () => PlayFabClientAPI.ExecuteCloudScript(request, onStackCallback, (PlayFabError error) => stackResultCallBack(null))));
                    UnityEngine.Debug.Log("Stacked " + allMethods.Count + " methods for stack " + stackId);
                    callDelay += 0.5f;
                }
            }

            cloudScriptStackMethods.Clear();

        }



        IEnumerator DelayedCall(float delay, Action action)
        {
            isWorking = true;
            yield return new WaitForSecondsRealtime(delay);
            action.Invoke();
        }

        /* IEnumerator TimerStack()
         {
             while (true)
             {
                 yield return new WaitForSeconds(3f);
                 TimerProcessStacks_Elapsed();
             }
         }*/


        private void onStackCallback(ExecuteCloudScriptResult result)
        {
            isWorking = false;
            bool success = false;
            CloudScriptResponse jsonResponse = null;
            if (result.Error == null && result.FunctionResult != null)
            {
                try
                {
                    jsonResponse = JsonConvert.DeserializeObject<CloudScriptResponse>(result.FunctionResult.ToString());
                    if (jsonResponse.ResponseCode == ResponseCode.Success) success = true;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log("CloudScript Serialisation error:" + e.Message);
                }
            }

            if (!success)
            {
                if (OnDataMissMatchDetected != null) OnDataMissMatchDetected();
            }

        }


        private void stackResultCallBack(CloudScriptResponse cloudScriptResponse)
        {
            if (cloudScriptResponse == null)
            {
                if (OnDataMissMatchDetected != null) OnDataMissMatchDetected(OnDataMissMatchDetectedEventType.HostUnreachable);
            }
            else
            {
                if (OnDataMissMatchDetected != null) OnDataMissMatchDetected(OnDataMissMatchDetectedEventType.BackofficeError);
            }

        }

        bool IsLogged = false;
        public string PlayFabId { get; set; }
        public string PlayerName { get; set; }

        public void CreateFakeAccount(Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure)
        {
            string userId = Guid.NewGuid().ToString();
            var request = new LoginWithCustomIDRequest { CustomId = userId, CreateAccount = true };
            PlayFabClientAPI.LoginWithCustomID(request, (LoginResult loginResult) => { loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
        }



        public void Login(Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure, string customId = "")
        {

#if UNITY_EDITOR || UNITY_STANDALONE


            //string testAccount = "32F0924A1B3B36DF";
            string testAccount = "";

            if (IsImpersonating)
            {
                testAccount = ImpersonateId;
            }

            if (string.IsNullOrEmpty(testAccount))
            {
                HandleDefaultLoginMethod(loginSuccess, loginFailure, customId);
            }
            else
            {
                HandleDefaultLoginMethodDevTest(testAccount, loginSuccess, loginFailure);
            }

#elif UNITY_ANDROID

            HandleDefaultLoginMethod(loginSuccess, loginFailure, customId);
            //HandleAndroidLoginMethod(loginSuccess, loginFailure);

#elif UNITY_IOS
           
            HandleiOSLoginMethod(loginSuccess, loginFailure);
#endif

            IsFirtsLogin = false;
        }



        //public void LoginWithKong(string authTicket, long userId, Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure)
        //{
        //    var request = new LoginWithCustomIDRequest { CustomId = userId.ToString(), CreateAccount = false };
        //    PlayFabClientAPI.LoginWithCustomID(request, (LoginResult loginResult) => { OnPlayerLogin(); loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
        //}


        //public void LinkKongregateAccount(string authTicket, long userId, Action<LinkCustomIDResult> callBack)
        //{
        //    var request = new LinkCustomIDRequest { CustomId = userId.ToString(), ForceLink = false };
        //    PlayFabClientAPI.LinkCustomID(request, callBack, (PlayFabError e) => callBack(null));
        //}


        public void LoginWithKong(string authTicket, long userId, Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure)
        {
            var request = new LoginWithKongregateRequest { AuthTicket = authTicket, KongregateId = userId.ToString(), CreateAccount = false };
            PlayFabClientAPI.LoginWithKongregate(request, (LoginResult loginResult) => { OnPlayerLogin(loginResult); loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
        }


        public void LinkKongregateAccount(string authTicket, long userId, Action<LinkKongregateAccountResult> callBack, Action<PlayFabError> error)
        {
            var request = new LinkKongregateAccountRequest { AuthTicket = authTicket, KongregateId = userId.ToString() };
            PlayFabClientAPI.LinkKongregate(request, callBack, error);
        }

        private void HandleDefaultLoginMethod(Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure, string customId)
        {
            var request = new LoginWithCustomIDRequest { CustomId = customId, CreateAccount = true };
            PlayFabClientAPI.LoginWithCustomID(request, (LoginResult loginResult) => { OnPlayerLogin(loginResult); loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
        }

        private void HandleDefaultLoginMethodDevTest(string customId, Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure)
        {
            var request = new LoginWithCustomIDRequest { CustomId = customId, CreateAccount = true };
            PlayFabClientAPI.LoginWithCustomID(request, (LoginResult loginResult) => { OnPlayerLogin(loginResult); loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
        }

        private void HandleAndroidDeviceIdLoginMethod(Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure)
        {
            var request = new LoginWithAndroidDeviceIDRequest { AndroidDeviceId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true };
            PlayFabClientAPI.LoginWithAndroidDeviceID(request, (LoginResult loginResult) => { OnPlayerLogin(loginResult); loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
        }

        private void HandleiOSDeviceIdLoginMethod(Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure)
        {
            var request = new LoginWithIOSDeviceIDRequest { DeviceId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true };
            PlayFabClientAPI.LoginWithIOSDeviceID(request, (LoginResult loginResult) => { OnPlayerLogin(loginResult); loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
        }

        private void HandleAndroidLoginMethod(Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure)
        {
#if UNITY_ANDROID

            if (GameServiceManager.GameService.IsSignedIn)
            {
                string authCode;

                if (IsFirtsLogin)
                {
                    authCode = ((GooglePlayGameService)GameServiceManager.GameService).GetServerAuthCode();
                }
                else
                {
                    ((GooglePlayGameService)GameServiceManager.GameService).GetAnotherServerAuthCode((string code) => OnNewAuthCodeReceived(loginSuccess, loginFailure, code));
                    return;
                }


                var request = new LoginWithGoogleAccountRequest() { ServerAuthCode = authCode, CreateAccount = true };
                PlayFabClientAPI.LoginWithGoogleAccount(request, (LoginResult loginResult) => { OnPlayerLogin(loginResult); loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
            }
            else
            {
                HandleAndroidDeviceIdLoginMethod(loginSuccess, loginFailure);
            }
#endif
        }

        private void OnNewAuthCodeReceived(Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure, string authCode)
        {
#if UNITY_ANDROID
            var request = new LoginWithGoogleAccountRequest() { ServerAuthCode = authCode, CreateAccount = true };
            PlayFabClientAPI.LoginWithGoogleAccount(request, (LoginResult loginResult) => { OnPlayerLogin(loginResult); loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
#endif
        }




        private void HandleiOSLoginMethod(Action<LoginResult> loginSuccess, Action<PlayFabError> loginFailure)
        {
#if UNITY_IOS

            if (GameServiceManager.GameService.IsSignedIn)
            {

                var request = new LoginWithGameCenterRequest() { PlayerId = GameServiceManager.GameService.PlayerId, CreateAccount = true };
                PlayFabClientAPI.LoginWithGameCenter(request, (LoginResult loginResult) => { OnPlayerLogin(loginResult); loginSuccess.Invoke(loginResult); IsLogged = true; PlayFabId = loginResult.PlayFabId; }, (PlayFabError e) => loginFailure.Invoke(e));
            }
            else
            {
                HandleiOSDeviceIdLoginMethod(loginSuccess, loginFailure);
            }
#endif
        }


        private void LoadSegments()
        {
            GetPlayerSegmentsRequest getPlayerSegmentsRequest = new GetPlayerSegmentsRequest();
            PlayFabClientAPI.GetPlayerSegments(getPlayerSegmentsRequest, OnPlayerSegmentsResult, OnPlayerSegmentError);
        }

        private void OnPlayerSegmentError(PlayFabError obj)
        {
        }

        private void OnPlayerSegmentsResult(GetPlayerSegmentsResult obj)
        {
            Segments = obj.Segments.Select(y => y.Name).ToList();
        }

        private void OnPlayerLogin(LoginResult loginResult)
        {
            this.entityKey = null;
            this.lastTokenResponse = loginResult.EntityToken;
            if (!IsImpersonating)
            {
                this.PlayerName = GameServiceManager.GameService.PlayerName;
            }


            this.GroupManager = new PlayFabGroupManager();

            if (!IsImpersonating && PlayerName != string.Empty)
                PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest() { DisplayName = this.PlayerName }, new Action<UpdateUserTitleDisplayNameResult>(OnUpdaterUserDisplayName), new Action<PlayFabError>(OnUpdaterUserDisplayNameError));
        }



        public void UpdateObject(string key, System.Object item, Action<SetObjectsResponse> action = null)
        {
            SetObjectsRequest setObjectsRequest = new SetObjectsRequest();
            setObjectsRequest.Entity = new PlayFab.DataModels.EntityKey() { Id = CurrentEntity.Id, Type = CurrentEntity.Type };
            setObjectsRequest.Objects = new List<SetObject>();
            setObjectsRequest.Objects.Add(new SetObject() { ObjectName = key, DataObject = JsonConvert.SerializeObject(item) });
            PlayFab.PlayFabDataAPI.SetObjects(setObjectsRequest, action, onPlayerObjectError);
        }

        private void onPlayerObjectError(PlayFabError obj)
        {

        }

        public void ReportRevenue()
        {
            //ConfirmPurchaseRequest request = new ConfirmPurchaseRequest() { OrderId }
            //PlayFabClientAPI.ConfirmPurchase();
        }

        private void OnUpdaterUserDisplayNameError(PlayFabError obj)
        {

        }

        private void OnUpdaterUserDisplayName(UpdateUserTitleDisplayNameResult obj)
        {

        }

        public void DownloadPlayerData(List<string> keys, Action<GetUserDataResult> result, Action<PlayFabError> loginFailure)
        {
            var request = new GetUserDataRequest { Keys = keys };
            PlayFabClientAPI.GetUserData(request, result, loginFailure);
        }

        public void AddToStack(string stackId, CloudScriptMethod cloudScriptMethod)
        {
            CloudScriptStackMethod stackMethod = new CloudScriptStackMethod();
            stackMethod.StackId = stackId;
            stackMethod.CloudScriptMethod = cloudScriptMethod;
            this.cloudScriptStackMethods.Add(stackMethod);
        }


        public void TimerProcessRealtime_Elapsed()
        {
            if (!isWorking && realtimeQueue.Count > 0)
            {
                realtimeQueue.First().Invoke();
                realtimeQueue.RemoveAt(0);
            }
        }
        public void InvokeBackOffice(CloudScriptMethod cloudScriptMethod, Action<CloudScriptResponse, CloudScriptMethod> resultCallBack)
        {
            cloudScriptMethod.Entity = CurrentEntity.Id;

            if (isWorking)
            {
                realtimeQueue.Add(() => InvokeBackOffice(cloudScriptMethod, resultCallBack));
                return;
            }

            if (resultCallBack == null)
            {
                //Only use for "shoot in the dark";
                resultCallBack = DummyResultCallback;
            }

            if (cloudScriptMethod.Attempt == -1)
            {
                cloudScriptMethod.Attempt = retryCountMax;
            }
            else
            {
                cloudScriptMethod.Attempt++;
            }

            cloudScriptMethod.Environement = BackOfficeEnvironement.ToString();

            isWorking = true;
            ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest() { FunctionName = "invokeBioIncBackOffice", FunctionParameter = cloudScriptMethod };
            PlayFabClientAPI.ExecuteCloudScript(request, (ExecuteCloudScriptResult r) => FromExecuteCloudScriptResult(r, cloudScriptMethod, resultCallBack), (PlayFabError e) => FromExecuteCloudScriptResult(null, cloudScriptMethod, resultCallBack));
        }


        private void DummyResultCallback(CloudScriptResponse arg1, CloudScriptMethod arg2)
        {
        }


        private void FromExecuteCloudScriptResult(ExecuteCloudScriptResult result, CloudScriptMethod cloudScriptMethod, Action<CloudScriptResponse, CloudScriptMethod> resultCallBack)
        {
            isWorking = false;
            bool success = false;
            CloudScriptResponse jsonResponse = null;
            if (result != null && result.Error == null && result.FunctionResult != null)
            {
                try
                {
                    jsonResponse = JsonConvert.DeserializeObject<CloudScriptResponse>(result.FunctionResult.ToString());

                    if (jsonResponse.ResponseCode != ResponseCode.Success)
                    {
                        if (OnDataMissMatchDetected != null && !cloudScriptMethod.IgnoreError) OnDataMissMatchDetected(OnDataMissMatchDetectedEventType.BackofficeError);
                    }

                    success = true;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log("CloudScript Serialisation error:" + e.Message);
                    UnityEngine.Debug.Log(result.FunctionResult);

                    if (OnDataMissMatchDetected != null && !cloudScriptMethod.IgnoreError) OnDataMissMatchDetected(OnDataMissMatchDetectedEventType.HostUnreachable);

                }
            }
            else
            {
                if (result != null)
                    UnityEngine.Debug.Log("CloudScript Error:" + result.Error.Error + " " + result.Error.Message);
                if (OnDataMissMatchDetected != null && !cloudScriptMethod.IgnoreError) OnDataMissMatchDetected(OnDataMissMatchDetectedEventType.HostUnreachable);
            }

            if (success)
            {
                resultCallBack.Invoke(jsonResponse, cloudScriptMethod);
            }


        }

    }
}
#endif