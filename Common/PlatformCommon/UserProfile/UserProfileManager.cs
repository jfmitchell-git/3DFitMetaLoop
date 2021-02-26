#if !BACKOFFICE
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.UserProfile
{
    public class UserProfileManager
    {

        //private const string baseFileName = "UserProfile.dat";
        private const string baseFileName = "GameData.dat";

        private static UserProfileManager instance;
        public static UserProfileManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserProfileManager();
                }
                return instance;
            }
        }

        public delegate void UserProfileManagerEventHandler(UserProfileEvent e);
        public event UserProfileManagerEventHandler OnUserProfileEvent;

        public UserProfileManager()
        {

        }

        private string Filepath
        {
            get
            {
                string dataFilePath = Application.persistentDataPath + "/" + baseFileName;
                return dataFilePath;
            }
        }
        private UserProfileData userProfileData;
        public UserProfileData UserProfileData
        {
            get
            {
                return userProfileData;
            }
        }

        public void Load()
        {
            bool isSuccess = false;
            if (!File.Exists(Filepath))
            {
                CreateNewUserProfile();
                Debug.Log("Creating new user profile.");
            }
            else
            {
                try
                {
                    userProfileData = LoadLocalData();
                    userProfileData.LastTimeOpen = DateTime.UtcNow;
                   
                    isSuccess = true;
                    //BackupLocalData();
                }
                catch (Exception e)
                {
                    OnUserProfileEvent(new UserProfileEvent(UserProfileEventType.UserProfileError));
                    Debug.Log("Could not load UserProfileData. " + e.Message);
                    if (File.Exists(Filepath + ".corrupted")) File.Delete(Filepath + ".corrupted");
                    File.Move(Filepath, Filepath + ".corrupted");
                    CreateNewUserProfile();
                }

                if (isSuccess)
                {
                    OnUserProfileEvent(new UserProfileEvent(UserProfileEventType.UserProfileLoaded));
                }

            }
        }

        private void CreateNewUserProfile()
        {
            userProfileData = new UserProfileData();
            userProfileData.CreateNew();
            OnUserProfileEvent(new UserProfileEvent(UserProfileEventType.UserProfileCreated));
            SaveLocal();
        }    

        public void Reset()
        {
            CreateNewUserProfile();
        }

        private UserProfileData LoadLocalData()
        {
            StreamReader reader = null;
            UserProfileData data = null;

            try
            {
                reader = new StreamReader(Filepath);
                data = JsonConvert.DeserializeObject<UserProfileData>(reader.ReadToEnd());
            } catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
            }

            return data;
        }

        private void BackupLocalData()
        {
            try
            {
                if (!File.Exists(Filepath + ".corrupted"))
                    File.Copy(Filepath, Filepath + ".bak", true);
            } catch
            {

            }
        }


        public void SaveLocal()
        {
            if (userProfileData != null)
            {
                StreamWriter writer = new StreamWriter(Filepath, false);
                writer.Write(JsonConvert.SerializeObject(userProfileData));
                writer.Close();
                OnUserProfileEvent(new UserProfileEvent(UserProfileEventType.UserProfileSaved));
                Debug.Log("User profile saved.");
            }
            else
            {
                throw new NotSupportedException("Cannot save null UserProfileData");
            }
        }

        //public void SaveLocal()
        //{
        //    if (userProfileData != null)
        //    {
        //        BinaryFormatter bf = new BinaryFormatter();
        //        FileStream file = File.Create(Filepath);
        //        bf.Serialize(file, userProfileData);
        //        file.Close();
        //        OnUserProfileEvent(new UserProfileEvent(UserProfileEventType.UserProfileSaved));
        //        Debug.Log("User profile saved.");
        //    }
        //    else
        //    {
        //        throw new NotSupportedException("Cannot save null UserProfileData");
        //    }
        //}
    }
}
#endif
