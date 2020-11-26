using MetaLoop.Common.PlatformCommon.PlayFabClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{
    [Serializable]
    public class SettingState
    {

        public int this[string settingId]
        {
            get
            {
                if (Settings.ContainsKey(settingId)) return Settings[settingId];
                return 0;
            }
            set
            {
                if (this[settingId] != value)
                    SetSetting(settingId, value);
            }
        }

        public string this[string settingId, bool fromKeyValuePair]
        {
            get
            {
                if (KeyValuePairs.ContainsKey(settingId)) return KeyValuePairs[settingId];
                return String.Empty;
            }
            set
            {
                if (this[settingId, fromKeyValuePair] != value)
                    SetKeyValuePairs(settingId, value);
            }
        }



        [JsonIgnore]
        public string UpdatedPlayerName
        {
           
            get
            {
                if (KeyValuePairs.ContainsKey("UpdatedPlayerName")) return KeyValuePairs["UpdatedPlayerName"];
                return String.Empty;
            }
            set
            {
                if (UpdatedPlayerName != value)
                    SetKeyValuePairs("UpdatedPlayerName", value);
            }
        }



        public Dictionary<string, int> Settings { get; set; }
        public Dictionary<string, bool> TutorialProgress { get; set; }
        public Dictionary<string, string> KeyValuePairs { get; set; }



        public SettingState()
        {
            Settings = new Dictionary<string, int>();
            TutorialProgress = new Dictionary<string, bool>();
            KeyValuePairs = new Dictionary<string, string>();
        }

        public void SetSetting(string settingId, int value)
        {
            if (!Settings.ContainsKey(settingId))
            {
                Settings.Add(settingId, value);
            }
            else
            {
                Settings[settingId] = value;
            }


#if !BACKOFFICE
            CloudScriptMethod cloudScriptMethod = new CloudScriptMethod("SetSetting", false);
            cloudScriptMethod.Params.Add("type", "setting");
            cloudScriptMethod.Params.Add("key", settingId);
            cloudScriptMethod.Params.Add("value", value.ToString());
            PlayFabManager.Instance.AddToStack("Settings", cloudScriptMethod);
#endif
        }


        public void SetKeyValuePairs(string settingId, string value)
        {
            if (!KeyValuePairs.ContainsKey(settingId))
            {
                KeyValuePairs.Add(settingId, value);
            }
            else
            {
                KeyValuePairs[settingId] = value;
            }


#if !BACKOFFICE

            CloudScriptMethod cloudScriptMethod = new CloudScriptMethod("SetSetting", false);
            cloudScriptMethod.Params.Add("type", "keyvaluepairs");
            cloudScriptMethod.Params.Add("key", settingId);
            cloudScriptMethod.Params.Add("value", value.ToString());
            PlayFabManager.Instance.AddToStack("Settings", cloudScriptMethod);
#endif

        }

        public string GetKeyValue(string settingId)
        {
            if (!KeyValuePairs.ContainsKey(settingId))
            {
                return string.Empty;
            }
            else
            {
                return KeyValuePairs[settingId];
            }
        }


        public bool GetTutorialProgress(string stepId)
        {

            if (!TutorialProgress.ContainsKey(stepId))
            {
                return false;
            }
            else
            {
                return TutorialProgress[stepId];
            }

        }

        public void SetTutorialProgress(string stepId, bool completed)
        {
            if (!TutorialProgress.ContainsKey(stepId))
            {
                TutorialProgress.Add(stepId, completed);
            }
            else
            {
                TutorialProgress[stepId] = completed;
            }

#if !BACKOFFICE
            CloudScriptMethod cloudScriptMethod = new CloudScriptMethod("SetSetting", false);
            cloudScriptMethod.Params.Add("type", "tutorial");
            cloudScriptMethod.Params.Add("key", stepId);
            cloudScriptMethod.Params.Add("value", completed.ToString());
            PlayFabManager.Instance.AddToStack("Settings", cloudScriptMethod);
#endif

        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }




    }
}
