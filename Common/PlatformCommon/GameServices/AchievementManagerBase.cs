#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaLoop.Common.PlatformCommon.GameServices
{
    public class AchievementManagerBase
    {

        public int CompletedCount
        {
            get
            {
                return achievements.Where(y => y.IsActive == false).Count();
            }
        }

        public int Count
        {
            get
            {
                return achievements.Count;
            }
        }

        protected List<AchievementBase> achievements;

        public AchievementManagerBase()
        {
            achievements = new List<AchievementBase>();
        }

        public virtual void EvaluateAchievements(int eventType, Object args)
        {
            achievements.Where(y => y.EventType == eventType).ToList().ForEach(y => y.EvaluateAchievement(args));
        }

    }
}
#endif