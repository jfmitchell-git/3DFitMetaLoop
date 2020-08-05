
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.UserProfile
{
    [Serializable]
    public class UserProfileData
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastTimeOpen { get; set; }
        public DateTime LastTimeSave { get; set; }
        public DateTime LastTimeSaveOnline { get; set; }
        public Object GameData { get; set; }

        public UserProfileData()
        {

        }

        public void CreateNew()
        {
            this.Id = Guid.NewGuid().ToString();
            this.CreatedDate = DateTime.UtcNow;
            this.LastTimeOpen = DateTime.UtcNow;
            this.LastTimeSave = DateTime.UtcNow;
        }

       
    }
}
