#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{


    public class MessagePanel : MonoBehaviour
    {

        public MessageStyle MessageStyle;

        public TextMeshProUGUI Title;
        public TextMeshProUGUI Description;

        public RectTransform InnerBack;

        //public Text Title;
        //public Text Description;
        public Image Icon;
        public Button Button;
        public Button CloseButton;
        public float TransitionTime = 0.25f;

        public TickerEvent OnOpen;
        public UnityEvent OnClose;
        public UnityEvent OnDisappearStart;
        public UnityEvent OnDisappearEnd;

        public bool AutoClose = true;


        [HideInInspector]
        public UnityEvent Close;
        
        public Image[] HideIfNoIcon;

        public Vector2 AddToStartPosition;

        [HideInInspector]
        public UnityEvent OnUpdate;

        void Update()
        {
            OnUpdate.Invoke();
        }

    }
}
#endif