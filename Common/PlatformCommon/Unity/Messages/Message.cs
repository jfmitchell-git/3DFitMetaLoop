#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{



    public class Message
    {

        public string MessageId;
        public bool AutoCloseOnButtonClick = true;
        public bool ShowClose = true;

        public MessageEvent ButtonClicked = new MessageEvent();

        public MessageEvent MessageAppear = new MessageEvent();

        public string Title;
        public string Description;
        public Sprite Icon;
        public GameObject IconGameObject;
        public string StyleName;
        public GameObject MessageContent;
        public bool DontDestroyContentOnDismiss = false;
        public bool MessageContentResizeToFit;

        [HideInInspector]
        public string TweenId = "Ticker";

        [HideInInspector]
        public string LayerName = "Default"; 

        [HideInInspector]
        public bool PassInFront = false;

        [HideInInspector]
        public bool ImmediateDispatch = false;

        public List<String> AllButtons = new List<string>();

        [HideInInspector]
        public List<GameObject> AllButtonsPrefabs = new List<GameObject>();

        [HideInInspector]
        public List<Button> AllButtonsObject = new List<Button>();

        //dont touch this
        [HideInInspector]
        public MessagePanel MessagePanel;

        [HideInInspector]
        public DateTime AppearTime;

        [HideInInspector]
        public int Order = -1;

        [HideInInspector]
        public bool BlurCamera;

        public Message()
        {
            BlurCamera = false;
        }

    }
}
#endif