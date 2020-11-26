#if !BACKOFFICE
using System;

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class AchievementBase
    {

        public bool TrigeredInMission { get; set; }
        protected string id = string.Empty;
        public string Id
        {
            get
            {
                return id;
            }
        }

        protected int eventType = 0;
        public int EventType
        {
            get
            {
                return eventType;
            }
        }

        protected bool isActive = false;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }

        public AchievementBase(string id, int eventType)
        {
            this.id = id;
            this.eventType = eventType;
            this.isActive = CheckIfAchievementEnabled();

            if (this.isActive) Console.WriteLine(string.Format("Achievement {0} is ready.", this.id));
        }

        public void EvaluateAchievement(Object args)
        {
            if (isActive) CheckForAchievement(args);
        }

        protected virtual void CheckForAchievement(Object args)
        {
            throw new NotSupportedException("The CheckForAchievement method must be overridden.");
        }

        /// <summary>
        /// Marks as completed.
        /// </summary>
        public void MarkAsCompleted()
        {
            isActive = false;
            //Report with GameServiceManager using PlatformAchievementId
            Console.WriteLine(string.Format("Achievement {0} is completed!", this.id));

            if (GameServiceManager.GameService.IsSignedIn)
            {
                GameServiceManager.GameService.ReportAchivement(PlatformAchievementId);
            }
        }

        private bool CheckIfAchievementEnabled()
        {
            bool isAchivementEnabled = true;
            if (GameServiceManager.GameService.IsSignedIn)
            {
                if (GameServiceManager.GameService.IsAchivementCompleted(PlatformAchievementId))
                {
                    isAchivementEnabled = false;
                }
            }
            return isAchivementEnabled;
        }


        protected virtual string PlatformAchievementId
        {
            get
            {
                return this.id;
            }
        }
    }
}
#endif