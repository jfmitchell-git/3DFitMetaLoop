#if !BACKOFFICE


using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{
    public class MessageProgress : Message
    {

        [HideInInspector]
        public float StartAt;
        [HideInInspector]
        public float EndAt;
    }
}
#endif