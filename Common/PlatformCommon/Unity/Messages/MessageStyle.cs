#if !BACKOFFICE

using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{
    [System.Serializable]
    public class MessageStyle
    {


        public string StyleName;
        public MessageType Type;

        public Canvas ParentCanvas;
        public RectTransform ParentRectTransform;

        public float ShowTime = 1f;
        public float TimeBetweenMessage = 0.5f;
        public int MaxPanel = 1;

        public float PaddingHeightBetweenTicker = 0f;
        
        public MessageLayout MessageAlign = MessageLayout.Vertical;
        public MessageHoriziontalLayout StartHorizontal = MessageHoriziontalLayout.Right;
        public MessageVerticalLayout StartVertical = MessageVerticalLayout.Top;

        public string GroupName;

        [HideInInspector]
        public Vector2 StartPos;
        [HideInInspector]
        public Vector2 EndPos;
        [HideInInspector]
        public string TweenId;
       
    }
}
#endif