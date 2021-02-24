#if !BACKOFFICE

using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{
    public class DGTooltip : MessagePanel
    {

        //animation offset
        public float AnimationOffset = 10f;

        //not sure neeeded
        public float TipOffset = 10f;

        public float MinimumSideSize = 25f;

        public float MaximumSideOutOfScreen = 0f;

        //all style
        public GameObject TopMiddle;
        public GameObject BottomMiddle;
        public GameObject LeftMiddle;
        public GameObject RightMiddle;





    }

    
}
#endif