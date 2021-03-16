#if !BACKOFFICE
using Crystal;
using DG.Tweening;
using MetaLoop.Common.PlatformCommon.Unity.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{

    [System.Serializable]
    public class MessageEvent : UnityEvent<Message, string>
    {
    }



    [System.Serializable]
    public class TickerEvent : UnityEvent<MessagePanel, Message>
    {
    }



    public class MessageManager : MonoBehaviour
    {

        public static MessageManager Instance;

        //All Events///////////////////////
        public MessageEvent OnMessageOpen;
        public MessageEvent OnMessageClose;
        public MessageEvent OnMessageDestroyed;
        public TickerEvent OnTickerOpen;
        public TickerEvent OnTickerClose;

        public UnityEvent OnTooltipOpen;
        public MessageEvent OnTooltipClose;

        //Styles///////////////////////////
        public List<MessagePanel> Styles;

        public Camera MessageCamera;
        private Tween MessagecameraTween;

        //Message////////////////////////////
        private List<Message> waitingMessages;
        private List<Message> currentMessages;
        public List<Message> CurrentMessages
        {
            get
            {
                return currentMessages;
            }
        }
        private List<Message> disappearMessages;
        private List<MessageHistory> historyMessages;



        public List<Camera> CameraBlur;
        public UnityEngine.Rendering.Universal.ForwardRendererData CameraBlurRendererData;

        public Image BackMessage;

        [HideInInspector]
        public bool IsPaused;

        [HideInInspector]
        public bool IsAutoDispatching = true;

     

        public List<MessageLayer> AllLayers;

        [HideInInspector]
        public bool LockCameraOn;

        public MessageManager()
        {
            waitingMessages = new List<Message>();
            currentMessages = new List<Message>();
            historyMessages = new List<MessageHistory>();
            disappearMessages = new List<Message>();
        }


        public void Awake()
        {
            Instance = this;

            //MessageCamera.gameObject.SetActive(false);

          

        }

        void Start()
        {

            //maybe i ll create a system layer later, for now first step overlayer
            foreach (MessageLayer messageLayer in AllLayers)
            {

                messageLayer.Layer = new GameObject();

                RectTransform myRect = messageLayer.Layer.AddComponent<RectTransform>();

                var safeArena = messageLayer.Layer.AddComponent<SafeArea>();
                safeArena.ResizeOnOrientation = true;
                safeArena.ConformX = true;
                safeArena.ConformY = true;


                messageLayer.Layer.transform.SetParent(this.gameObject.transform);
                messageLayer.Layer.transform.localScale = new Vector3(1, 1, 1);
                messageLayer.Layer.transform.localPosition = new Vector3(0, 0, -(messageLayer.Order * 100));
                messageLayer.Layer.transform.localRotation = new Quaternion(0, 0, 0, 0);

                myRect.anchorMin = new Vector2(0, 0);
                myRect.anchorMax = new Vector2(1, 1);
                myRect.offsetMin = new Vector2(0, 0);
                myRect.offsetMax = new Vector2(0, 0);

                //create a coby of the back
                messageLayer.Back = GameObject.Instantiate(BackMessage.gameObject, messageLayer.Layer.transform, true).GetComponent<Image>();

                //for now always OverUI
                messageLayer.Layer.layer = LayerMask.NameToLayer("OverUI");


                messageLayer.Back.color = new Color(0, 0, 0, 0);
                //messageLayer.Back.transform.localScale = new Vector3(1, 1, 1);

                messageLayer.Back.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                messageLayer.Back.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

                messageLayer.Back.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                messageLayer.Back.GetComponent<RectTransform>().offsetMax = new Vector2(Screen.width * 2, Screen.height * 2);
                messageLayer.Back.transform.localPosition = new Vector3(0, 0, 0);

                //PROBLEM WITH THAT
                if (messageLayer.OverwriteCanvas)
                {

                    messageLayer.Back.gameObject.AddComponent<GraphicRaycaster>();
                    //messageLayer.Back.gameObject.AddComponent<Canvas>();

                    Canvas newBackCanvas = messageLayer.Back.gameObject.GetComponent<Canvas>();
                    newBackCanvas.overrideSorting = true;
                    newBackCanvas.sortingLayerName = messageLayer.OverwriteCanvasSortingLayer;
                    //newBackCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent | AdditionalCanvasShaderChannels.TexCoord1;



                }


            }


            //because of the back, we remove the canvas from the back
            //Destroy(BackMessage.GetComponent<Canvas>());

        }


        /// <summary>
        /// Add message style manually
        /// </summary>
        /// <param name="style"></param>
        public void AddStyles(MessagePanel style)
        {
            Styles.Add(style);
        }

        /// <summary>
        /// Add a message by code
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(Message message)
        {
            waitingMessages.Add(message);

            if (message.ImmediateDispatch)
                Update();

        }

        /// <summary>
        /// Show message without passing by the queue
        /// </summary>
        public void ShowMessage(Message message)
        {




            //get the default Panel prefabs
            MessagePanel defaultPanelForStyle = Styles.Where(p => p.MessageStyle.StyleName == message.StyleName).Single();


            //check if max message, we return
            if (currentMessages.Where(p => p.MessagePanel.MessageStyle.GroupName == defaultPanelForStyle.MessageStyle.GroupName).Count() >= defaultPanelForStyle.MessageStyle.MaxPanel && !message.ImmediateDispatch) return;

            //put in currentMessage
            waitingMessages.Remove(message);
            currentMessages.Add(message);

            if (!defaultPanelForStyle)
            {
                Debug.Log("Impossible to find style of the message in MessageManager");
                return;
            }

            //if mesage got  panel, we kill everything on it (for tooltips)
            if (message.MessagePanel)
            {
                if(message.MessagePanel.GetComponent<RectTransform>() != null)
                message.MessagePanel.GetComponent<RectTransform>().DOKill(true);

            }

            //if message with same contnet is currently getting destroyed (esc fast) we need to kill it faster)
            Message disaparingMessageWithSameContent = disappearMessages.Where(p => p.MessageContent != null && p.MessageContent == message.MessageContent).LastOrDefault();
            if(disaparingMessageWithSameContent != null)
            {
                disaparingMessageWithSameContent.MessagePanel.GetComponent<CanvasGroup>().DOKill(true);
            }


            //create panel gameobject
            GameObject messageGameObject = (GameObject)Instantiate(defaultPanelForStyle.gameObject);
            MessagePanel messagePanelForStyle = messageGameObject.GetComponent<MessagePanel>();
            message.MessagePanel = messagePanelForStyle;

            messageGameObject.SetActive(true);


            //kill tween on complete
            MessagecameraTween.Kill(false);
            MessagecameraTween = null;

            //maybe different animaiton ?

            //animate alpha
            //if no canvas group, add it
            CanvasGroup canvasGroup = messagePanelForStyle.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = messagePanelForStyle.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.interactable = true;
            canvasGroup.alpha = 0f;

            canvasGroup.DOKill();
            canvasGroup.DOFade(1f, messagePanelForStyle.TransitionTime).SetUpdate(true);

            //keep when it appeared
            message.AppearTime = DateTime.Now;



            //get next available order for style
            List<Message> allMessageSameType = currentMessages.Where(p => p.MessagePanel.MessageStyle.GroupName == message.MessagePanel.MessageStyle.GroupName).ToList();

            //get first available order
            int messageOrder = 0;

            while (allMessageSameType.Where(p => p.Order == messageOrder).Count() != 0) messageOrder++;
            message.Order = messageOrder;

            //get layer
            MessageLayer useLayer = AllLayers.Where(p => p.Name == message.LayerName).SingleOrDefault();

            //adding it to correct parent
            //set popup on the canvas of MessageManager
            if (defaultPanelForStyle.MessageStyle.ParentCanvas != null)
            {
                if(defaultPanelForStyle.MessageStyle.ParentRectTransform != null)
                {
                    messageGameObject.transform.SetParent(defaultPanelForStyle.MessageStyle.ParentRectTransform, false);
                } else
                {
                    messageGameObject.transform.SetParent(defaultPanelForStyle.MessageStyle.ParentCanvas.transform, false);
                }
               

            }
            else {

                messageGameObject.transform.SetParent(useLayer.Layer.transform, false);
            }


            //test
            if(useLayer != null)
            {

                //PROBLEM WITH THAT
                if (useLayer.OverwriteCanvas)
                {

                    messagePanelForStyle.gameObject.AddComponent<GraphicRaycaster>();

                    if(messagePanelForStyle.gameObject.GetComponent<Canvas>() == null)
                        messagePanelForStyle.gameObject.AddComponent<Canvas>();


                    Canvas newCanvas = messagePanelForStyle.gameObject.GetComponent<Canvas>();
                    newCanvas.overrideSorting = true;
                    newCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent | AdditionalCanvasShaderChannels.TexCoord1;
                    newCanvas.sortingLayerName = useLayer.OverwriteCanvasSortingLayer;

                    newCanvas.sortingOrder = 1;



                }

             

            }

            //testing out better fill
            //set information
            if (messagePanelForStyle.Title != null)
            {

                if (message.Description != null && message.Title.StartsWith("%") && message.Title.EndsWith("%"))
                {
                    message.Title = ResourceManager.GetValue(message.Title.Substring(1, message.Title.Length - 2));
                    message.Title = message.Title.Replace(@"\n", Environment.NewLine);
                }

                if (message.Title == "")
                {
                    messagePanelForStyle.Title.gameObject.SetActive(false);
                }
                else
                {
                    messagePanelForStyle.Title.gameObject.SetActive(true);
                    messagePanelForStyle.Title.text = message.Title;
                }
            }

            //description
            if (messagePanelForStyle.Description != null)
            {

                if (message.Description != null && message.Description.StartsWith("%") && message.Description.EndsWith("%"))
                {
                    message.Description = ResourceManager.GetValue(message.Description.Substring(1, message.Description.Length - 2));
                    message.Description = message.Description.Replace(@"\n", Environment.NewLine);
                }

                if (message.Description == null || message.Description == "")
                {
                    messagePanelForStyle.Description.gameObject.SetActive(false);
                }
                else
                {
                    messagePanelForStyle.Description.gameObject.SetActive(true);
                    messagePanelForStyle.Description.text = message.Description;
                }
            }


            //add all button////////////////////////////
            if (message.AllButtons.Count > 0)
            {
                for (int i = 0; i < message.AllButtons.Count; i++)
                {
                    Button myButton;

                    if(message.AllButtonsPrefabs.Count > i && message.AllButtonsPrefabs[i] != null)
                    {
                        myButton = message.AllButtonsPrefabs[i].GetComponent<Button>();
                        message.AllButtonsPrefabs[i].SetActive(true);
                        myButton.transform.SetParent(messagePanelForStyle.Button.transform.parent, false);

                        //if hide original button if it s the first button
                        if(i == 0)
                        {
                            messagePanelForStyle.Button.gameObject.SetActive(false);
                        }

                    } else if (i == 0)
                    {
                        myButton = messagePanelForStyle.Button;
                    }
                    else
                    {
                        myButton = (Button)Instantiate(messagePanelForStyle.Button, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                        myButton.transform.SetParent(messagePanelForStyle.Button.transform.parent, false);
                    }

                    //set text on button
                    myButton.GetComponentInChildren<TextMeshProUGUI>().text = message.AllButtons[i].ToUpper();

                    //set 
                    myButton.onClick.AddListener(() => MessageButtonClicked(message, myButton.GetComponentInChildren<TextMeshProUGUI>().text));

                    message.AllButtonsObject.Add(myButton);

                }

            }
            else
            {
                if (messagePanelForStyle.Button)
                    messagePanelForStyle.Button.transform.parent.gameObject.SetActive(false);
                //Destroy(messagePanelForStyle.Button.transform.parent.gameObject);
            }
            //////////////////////////////////////////////






            switch (defaultPanelForStyle.MessageStyle.Type)
            {

                case MessageType.Popup:

                    //show back
                    DOTween.Kill(useLayer.Back, true);
                    useLayer.Back.gameObject.SetActive(true);
                    useLayer.Back.DOFade(useLayer.BackAlpha, messagePanelForStyle.TransitionTime).SetUpdate(true);

                    //resize innerBack if no button
                    if (message.AllButtons.Count == 0)
                    {
                        //TODO : check if needed
                        //messagePanelForStyle.InnerBack.offsetMin = new Vector2(messagePanelForStyle.InnerBack.offsetMin.x, messagePanelForStyle.InnerBack.offsetMin.x);
                    }

                    //check if there is a inner message object
                    SetContentInMessage(message, messagePanelForStyle);


                    


                    //problem with textureatlas here
                    if (messagePanelForStyle.Icon && message.Icon)
                    {
                        messagePanelForStyle.Icon.sprite = message.Icon;
                    }
                    else
                    {
                        if (messagePanelForStyle.Icon)
                            GameObject.Destroy(messagePanelForStyle.Icon);

                        //hide image if no icon
                        messagePanelForStyle.HideIfNoIcon.ToList().ForEach(p => p.enabled = false);

                    }

                    if (message.IconGameObject != null)
                    {
                        //message.IconGameObject = GameObject.Instantiate(message.IconGameObject);
                        message.IconGameObject.transform.SetParent(messagePanelForStyle.Icon.transform.parent, false);
                        //message.IconGameObject.transform.SetParent(messagePanelForStyle.Icon.transform.parent);
                    }




                    BlurCamera(true);

                    //add close behaviour if there is a close button (auto-close)
                    if (messagePanelForStyle.CloseButton && message.ShowClose)
                    {

                        messagePanelForStyle.CloseButton.onClick.AddListener(() => MessageButtonClicked(message, ""));
                    } else if(messagePanelForStyle.CloseButton && !message.ShowClose)
                    {
                        messagePanelForStyle.CloseButton.gameObject.SetActive(false);
                    }

                    //invoke open of the message
                    OnMessageOpen.Invoke(message, null);

                    //pause
                    IsPaused = true;

                    //we stop ticker
                    StopTicker();

                    break;



                case MessageType.Ticker:


                    //problem with textureatlas here
                    if (messagePanelForStyle.Icon && message.Icon)
                    {
                        messagePanelForStyle.Icon.sprite = message.Icon;
                    }
                    else
                    {
                        //maybe remove icon auto??
                        messagePanelForStyle.Icon.DOFade(0, 0).SetUpdate(true);
                    }

                    //animate it
                    // DOVirtual.DelayedCall(0.1f, () => AnimateTicker(message));
                    AnimateTicker(message, messagePanelForStyle);

                    //invoke
                    OnTickerOpen.Invoke(messagePanelForStyle, message);

                    //invoke onOpen too
                    message.MessagePanel.OnOpen.Invoke(message.MessagePanel,message);

                    break;


                case MessageType.Tooltip:

                    Debug.Log("APPEAR TOOLTIP " + message.StyleName);

                    //be sure to kill everything on the tooltip
                    message.MessagePanel.GetComponent<RectTransform>().DOKill();

                    //show the tooltip at correct position

                    //((MessageTooltip)message).MessagePanel.gameObject.SetActive(false);

                    SetContentInMessage(message, messagePanelForStyle);

                    ((MessageTooltip)message).Place(MessageCamera);

                    // DOVirtual.DelayedCall(0.25f, () => ((MessageTooltip)message).Place(MessageCamera));


                    break;

            }


            message.MessageAppear.Invoke(message, string.Empty);



            CheckCamera();




        }

        private void RefreshSize(Message message, MessagePanel messagePanelForStyle)
        {

            if (message == null || message.MessageContent == null) return;
            if (disappearMessages.IndexOf(message, 0) != -1) return;

            RectTransform messageContentRect = message.MessageContent.GetComponent<RectTransform>();

            //we need to add a layoutelement the size of the panel
            LayoutElement messageContentLayout = message.MessageContent.GetComponent<LayoutElement>();
            if (messageContentLayout == null)
            {
                messageContentLayout = message.MessageContent.AddComponent<LayoutElement>();
            }
            messageContentLayout.preferredWidth = messageContentRect.rect.width;
            messageContentLayout.preferredHeight = messageContentRect.rect.height;

            //force real size height on the innerback (because of scale)
            if (messagePanelForStyle.InnerBack != null && messagePanelForStyle.InnerBack.GetComponent<LayoutElement>() != null && messagePanelForStyle.InnerBack.GetComponent<LayoutElement>().enabled)
            {
                messagePanelForStyle.InnerBack.GetComponent<LayoutElement>().preferredWidth = message.MessageContent.GetComponent<RectTransform>().rect.width * messagePanelForStyle.InnerBack.localScale.x;
                messagePanelForStyle.InnerBack.GetComponent<LayoutElement>().preferredHeight = message.MessageContent.GetComponent<RectTransform>().rect.height * messagePanelForStyle.InnerBack.localScale.y;
            }

        }

        private void SetContentInMessage(Message message,MessagePanel messagePanelForStyle)
        {
            

            if (message.MessageContent)
            {

                RectTransform messageContentRect = message.MessageContent.GetComponent<RectTransform>();

                //we need to add a layoutelement the size of the panel
                LayoutElement messageContentLayout = message.MessageContent.GetComponent<LayoutElement>();
                if (messageContentLayout == null)
                {
                    messageContentLayout = message.MessageContent.AddComponent<LayoutElement>();
                }
                messageContentLayout.preferredWidth = messageContentRect.rect.width;
                messageContentLayout.preferredHeight = messageContentRect.rect.height;

                //resize to fit
                if (message.MessageContentResizeToFit)
                {

                    Vector2 messageSize = new Vector2(message.MessageContent.GetComponent<RectTransform>().rect.width, message.MessageContent.GetComponent<RectTransform>().rect.height);
                    Vector2 contentSize = new Vector2(messagePanelForStyle.InnerBack.rect.width, messagePanelForStyle.InnerBack.rect.height);

                    RectTransform test = messagePanelForStyle.InnerBack;

                    //correct scale factor
                    float goodScale = Math.Min(contentSize.x / messageSize.x, contentSize.y / messageSize.y);

                    message.MessageContent.GetComponent<RectTransform>().localScale = new Vector3(goodScale, goodScale, 1);



                } else
                {
                    message.MessageContent.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                }


                   

                //set parent
                message.MessageContent.transform.SetParent(messagePanelForStyle.InnerBack.transform, false);
                //activate it
                message.MessageContent.SetActive(true);


                //force real size height on the innerback (because of scale)
                if (messagePanelForStyle.InnerBack != null && messagePanelForStyle.InnerBack.GetComponent<LayoutElement>() != null && messagePanelForStyle.InnerBack.GetComponent<LayoutElement>().enabled)
                {
                    messagePanelForStyle.InnerBack.GetComponent<LayoutElement>().preferredWidth = message.MessageContent.GetComponent<RectTransform>().rect.width * messagePanelForStyle.InnerBack.localScale.x;
                    messagePanelForStyle.InnerBack.GetComponent<LayoutElement>().preferredHeight = message.MessageContent.GetComponent<RectTransform>().rect.height * messagePanelForStyle.InnerBack.localScale.y;      
                }



                //stupid refresh
                List<LayoutGroup> keepList = new List<LayoutGroup>();
                //maybe force a refresh ?
                List<LayoutGroup> allLayout = messagePanelForStyle.InnerBack.transform.parent.GetComponentsInChildren<LayoutGroup>().ToList();
                foreach(LayoutGroup layout in allLayout)
                {
                    if(layout.enabled)
                    {
                        layout.enabled = false;
                        keepList.Add(layout);
                    }
                }

                //refresh layout ?
                 messagePanelForStyle.InnerBack.transform.parent.gameObject.SetActive(false);
                messagePanelForStyle.InnerBack.transform.parent.gameObject.SetActive(true);
                messagePanelForStyle.InnerBack.transform.parent.GetComponentsInChildren<LayoutGroup>().ToList().ForEach(p => { if (p.enabled) { p.enabled = false; p.enabled = true; }; });

                keepList.ForEach(p => p.enabled = true);


                //weird test
                //message.MessagePanel.GetComponentsInChildren<LayoutGroup>().ToList().ForEach(p => { if (p.enabled) { p.enabled = false; p.enabled = true; }; });

                //resize after a certain time because unity is stupid
                DOVirtual.DelayedCall(0.01f, () => RefreshSize(message, messagePanelForStyle));

            }

        }


        private void FillInfo()
        {

        }


        /// <summary>
        /// Animate the ticker
        /// </summary>
        /// <param name="ticker"></param>
        private void AnimateTicker(Message ticker,MessagePanel messagePanel)
        {

            RectTransform tickerRect = ticker.MessagePanel.GetComponent<RectTransform>();

            ticker.MessagePanel.MessageStyle.StartPos = tickerRect.anchoredPosition;
            ticker.MessagePanel.MessageStyle.EndPos = tickerRect.anchoredPosition;

            //get all same style ticker by group
            List<Message> allTickerSameStyle = currentMessages.Where(p => p.MessagePanel.MessageStyle.GroupName == messagePanel.MessageStyle.GroupName).ToList();

            //get all order under to add ;)
            Vector2 addSize = new Vector2(0, 0);
            for (int i = 0; i < ticker.Order; i++)
            {

                Message checkTicker = allTickerSameStyle.Where(p => p.Order == i).LastOrDefault();

                addSize.x += checkTicker.MessagePanel.GetComponent<RectTransform>().sizeDelta.x;
                addSize.y += checkTicker.MessagePanel.GetComponent<RectTransform>().sizeDelta.y + checkTicker.MessagePanel.MessageStyle.PaddingHeightBetweenTicker;

            }




            //adding to the endpos the width of each other

            if (ticker.MessagePanel.MessageStyle.MessageAlign == MessageLayout.Vertical)
            {
                ticker.MessagePanel.MessageStyle.EndPos.y -= addSize.y;
                ticker.MessagePanel.MessageStyle.StartPos.y -= addSize.y;
            }
            else
            {
                ticker.MessagePanel.MessageStyle.EndPos.x += addSize.x;
            }


            //x start
            switch (ticker.MessagePanel.MessageStyle.StartHorizontal)
            {

                case MessageHoriziontalLayout.Left:

                    // ticker.MessagePanel.MessageStyle.StartPos.x = -ticker.MessagePanel.GetComponent<RectTransform>().sizeDelta.x;


                    break;

                case MessageHoriziontalLayout.Center:

                    ticker.MessagePanel.MessageStyle.StartPos.y = ticker.MessagePanel.MessageStyle.EndPos.y + ticker.MessagePanel.AddToStartPosition.y;


                    break;

                case MessageHoriziontalLayout.Right:

                    ticker.MessagePanel.MessageStyle.StartPos.x = ticker.MessagePanel.GetComponent<RectTransform>().sizeDelta.x + ticker.MessagePanel.AddToStartPosition.x;
                    ticker.MessagePanel.MessageStyle.StartPos.y = ticker.MessagePanel.MessageStyle.EndPos.y + ticker.MessagePanel.AddToStartPosition.y;
                    break;

            }

            //set start position
            tickerRect.anchoredPosition = ticker.MessagePanel.MessageStyle.StartPos;

            //do the motion, and oncomplete we wait
            tickerRect.DOAnchorPos(ticker.MessagePanel.MessageStyle.EndPos, ticker.MessagePanel.TransitionTime).OnComplete(() => WaitMessage(ticker)).SetId(ticker.TweenId);

        }

        public void StopTicker()
        {
            DOTween.Pause("Ticker");
        }
        public void ResumeTicker()
        {
            DOTween.Play("Ticker");
        }

        /// <summary>
        /// Remove the message from the list (important for ticker so the other ticker can start)
        /// </summary>
        /// <param name="message"></param>
        private void RemoveMessageFromCurrentList(Message message)
        {

            if (currentMessages.IndexOf(message, 0) == -1) return;

            //stop everything in the message
            message.MessagePanel.OnDisappearStart.Invoke();

            disappearMessages.Add(message);
            currentMessages.Remove(message);


            //we now add the alpha on the canvas group too here and we add it here because it will work on everything
            var canvasGroup = message.MessagePanel.GetComponent<CanvasGroup>();
            if(canvasGroup != null)
                canvasGroup.DOFade(0f, message.MessagePanel.TransitionTime).SetUpdate(true);



        }

        /// <summary>
        /// Wait the message ShowTime
        /// </summary>
        /// <param name="message"></param>
        private void WaitMessage(Message message)
        {

           

            //wait the showtime
            if (message.MessagePanel.AutoClose)
            {
                message.MessagePanel.GetComponent<RectTransform>().DOAnchorPos(message.MessagePanel.MessageStyle.StartPos, message.MessagePanel.TransitionTime).SetDelay(message.MessagePanel.MessageStyle.ShowTime).OnStart(() => RemoveMessageFromCurrentList(message)).OnComplete(() => CloseMessage(message)).SetId(message.TweenId);

            }
            else
            {
                message.MessagePanel.Close.AddListener(() => message.MessagePanel.GetComponent<RectTransform>().DOAnchorPos(message.MessagePanel.MessageStyle.StartPos, message.MessagePanel.TransitionTime).OnStart(() => RemoveMessageFromCurrentList(message)).OnComplete(() => CloseMessage(message)).SetId(message.TweenId));
            }


        }

        private List<Camera> affectedCamera = new List<Camera>();
        private void BlurCamera(bool enable, float speed = 0f)
        {
            if (MessagecameraTween != null)
            {
                MessagecameraTween.Kill(false);
                MessagecameraTween = null;
            }



           


            //check for blur on camera
            for (int i = 0; i < CameraBlur.Count; i++)
            {

                if (CameraBlur[i])
                {
                    MobileBlur blurEffect = CameraBlur[i].GetComponent<MobileBlur>();
                    if (blurEffect)
                    {

                        if(enable)
                        {
                            if(!blurEffect.enabled)
                            {
                                affectedCamera.Add(CameraBlur[i]);
                                blurEffect.enabled = enable;
                            }

                        } else
                        {
                            if(affectedCamera.IndexOf(CameraBlur[i],0) != -1)
                            {
                                blurEffect.enabled = false;
                            }
                        }

                      

                       

                    }
                }

            }

            if (CameraBlurRendererData != null)
            {
                var blurFeature = CameraBlurRendererData.rendererFeatures.OfType<MobileBlurUrp>().FirstOrDefault();
                if (blurFeature != null)
                {

                    blurFeature.SetActive(enable);
                    CameraBlurRendererData.SetDirty();


                }
            }


            //
            if (enable)
            {
                MessageCamera.gameObject.SetActive(true);
            }
            else
            {
                affectedCamera.Clear();
                MessagecameraTween = DOVirtual.DelayedCall(speed, CheckCamera);
            }


        }


        private void CheckCamera()
        {

           // return;

            if (LockCameraOn) return;

            if (MessagecameraTween != null && MessagecameraTween.IsPlaying()) return;

            if (currentMessages.Count == 0 && disappearMessages.Count == 0)
                MessageCamera.gameObject.SetActive(false);
            else
                MessageCamera.gameObject.SetActive(true);
        }

        private void CloseButtonClicked()
        {

        }

        /// <summary>
        /// When a button is clicked in a Message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="buttonText"></param>
        public void MessageButtonClicked(Message message, string buttonText)
        {

            message.ButtonClicked.Invoke(message, buttonText);

            if (message.AutoCloseOnButtonClick)
            {
                CloseMessage(message, buttonText);
            }





        }

        public void CloseMessage(string id)
        {
            //check if message exist
            Message closeMessage = currentMessages.Where(p => p.MessageId == id).SingleOrDefault();
            if (closeMessage != null)
                CloseMessage(closeMessage);
        }



        /// <summary>
        /// Close Message
        /// </summary>
        /// <param name="message"></param>
        public void CloseMessage(Message message = null, string buttonText = null)
        {

            if (message == null) return;

            //remove from the current list (if it s still in it)
            RemoveMessageFromCurrentList(message);

            //if no messagepanel, impossible already removed
            if (message.MessagePanel == null) return;



            //tell it's finish when the transiiton is finish
            DOVirtual.DelayedCall(message.MessagePanel.TransitionTime, () => DisappearFinish(message));

            //remove all listener on button
            message.MessagePanel.GetComponent<CanvasGroup>().interactable = false;

            if (message.AllButtonsObject != null)
                message.AllButtonsObject.ForEach(p => p.onClick.RemoveAllListeners());

            switch (message.MessagePanel.MessageStyle.Type)
            {

                case MessageType.Popup:

                    //removing listener (not sure useful in unity)
                    if (message.MessagePanel.CloseButton && message.ShowClose)
                    {
                        message.MessagePanel.CloseButton.onClick.RemoveAllListeners();
                    }


                    //remove blur if no more message
                    if (currentMessages.Where(p => p.MessagePanel != null && p.MessagePanel.MessageStyle != null && (p.MessagePanel.MessageStyle.Type == MessageType.Popup || p.BlurCamera)).ToList().Count == 0)
                        BlurCamera(false, message.MessagePanel.TransitionTime);


                    //invoke close
                    OnMessageClose.Invoke(message, buttonText);

                    IsPaused = false;
                    ResumeTicker();





                    //ok removing the animation make it work
                    if (message.MessagePanel.GetComponent<Animator>() != null)
                        Destroy(message.MessagePanel.GetComponent<Animator>());

                    //destroy with a fade
                    message.MessagePanel.GetComponent<CanvasGroup>().DOFade(0f, message.MessagePanel.TransitionTime).OnComplete(() => DestroyMessage(message)).SetUpdate(true);

                    //fade 3d object too
                    Shaders.ShaderUtils.FadeObject(message.MessagePanel, 0f, message.MessagePanel.TransitionTime);

                    MessageLayer useLayer = AllLayers.Where(p => p.Name == message.LayerName).Single();

                    //remove back
                    DOTween.Kill(useLayer.Back, true);
                    useLayer.Back.DOFade(0f, message.MessagePanel.TransitionTime).OnComplete(() => useLayer.Back.gameObject.SetActive(false)).SetUpdate(true);

                    if (message.IconGameObject != null)
                    {
                        GameObject.Destroy(message.IconGameObject);
                    }

                    break;



                case MessageType.Ticker:

                    //ticker removed
                    OnTickerClose.Invoke(message.MessagePanel, message);

                    //already removed from array because of animation

                    
                    Destroy(message.MessagePanel.gameObject);
                     Destroy(message.MessagePanel);
                   

                    break;

                case MessageType.Tooltip:

                    Message duplicate = message;

                    message.MessagePanel.GetComponent<RectTransform>().DOKill();
                    message.MessagePanel.GetComponent<CanvasGroup>().DOKill();

                    message.MessagePanel.GetComponent<RectTransform>().DOAnchorPos(message.MessagePanel.MessageStyle.StartPos, message.MessagePanel.TransitionTime).OnComplete(() => DestroyMessage(duplicate));
                    message.MessagePanel.GetComponent<CanvasGroup>().DOFade(0f, message.MessagePanel.TransitionTime).SetUpdate(true);

                    //Destroy(message.MessagePanel.gameObject);
                    //Destroy(message.MessagePanel);

                    //not perfect but hey.... 
                    OnTooltipClose.Invoke(message,"");


                    break;

            }


            CheckCamera();






        }

        private void DestroyMessage(Message message)
        {

           // Debug.Log("DESTROYING MESSAGE " + message.StyleName);

            if (!message.DontDestroyContentOnDismiss)
            {


                Destroy(message.MessagePanel);
                Destroy(message.MessagePanel.gameObject);
            } else
            {

                if (message.MessageContent)
                {
                    message.MessageContent.transform.SetParent(null, false);


                    //NEW CODE TO FIX RESIZING////////////////////////////////
                    MessagePanel messagePanelForStyle = message.MessagePanel;
                    //set back correct size
                    if (messagePanelForStyle.InnerBack != null && messagePanelForStyle.InnerBack.GetComponent<LayoutElement>() != null && messagePanelForStyle.InnerBack.GetComponent<LayoutElement>().enabled)
                    {

                        message.MessageContent.GetComponent<RectTransform>().sizeDelta = new Vector2(message.MessageContent.GetComponent<LayoutElement>().preferredWidth, message.MessageContent.GetComponent<LayoutElement>().preferredHeight);
                    }
                    //////////////////////////////////////////////////////////
                }
                // DOVirtual.DelayedCall(1f, () => Destroy(message.MessagePanel.gameObject));
                Destroy(message.MessagePanel.gameObject);
                
            }


            OnMessageDestroyed.Invoke(message,null);


        }


        /// <summary>
        /// Close all the current open messages (Not useful for now)
        /// </summary>
        public void CloseCurrentMessages()
        {
            while (currentMessages.Count > 0)
            {
                CloseMessage(currentMessages[0]);
            }

        }

        private void DisappearFinish(Message message)
        {

            //tell it s CLOSE
            message.MessagePanel.OnClose.Invoke();

            //already removed
            if (disappearMessages.IndexOf(message, 0) == -1) return;

            disappearMessages.Remove(message);

            //add to history/////////////////////
            MessageHistory newHistory = new MessageHistory();
            newHistory.Time = DateTime.Now;
            newHistory.Type = message.MessagePanel.MessageStyle.Type;
            newHistory.StyleName = message.StyleName;
            historyMessages.Add(newHistory);
            /////////////////////////////////////////

            CheckCamera();
        }


        private void OnDestroy()
        {
            OnMessageOpen.RemoveAllListeners();
            OnMessageClose.RemoveAllListeners();
            OnMessageDestroyed.RemoveAllListeners();
            OnTickerOpen.RemoveAllListeners();
            OnTickerClose.RemoveAllListeners();
            OnTooltipOpen.RemoveAllListeners();
            OnTooltipClose.RemoveAllListeners();

            BlurCamera(false, 0f);

            Instance = null;

            Debug.Log("MESSAGE MANAGER DESTROYED");
        }

        /// <summary>
        /// Update the MessageManager
        /// </summary>
        void Update()
        {

            //check for message that are MANDATORY (settings, stuff like that)
            Message immediateMessage = waitingMessages.Where(p => p.ImmediateDispatch).FirstOrDefault();
            if(immediateMessage != null)
                ShowMessage(immediateMessage);

            List<Message> allTooltip = waitingMessages.Where(p => Styles.Where(q => q.MessageStyle.StyleName == p.StyleName).Single().MessageStyle.Type == MessageType.Tooltip).ToList();
            //always show all tooltip
            allTooltip.ForEach(p => ShowMessage(p));

            //Debug.Log("IsPaused in MessageManager = " + IsPaused);

            //if paused we just ignore
            if (IsPaused) return;



            //check if there is a forcedispatch

            //we should check first for any popup as they pass first
            List<Message> nextPopup = waitingMessages.Where(p => Styles.Where(q => q.MessageStyle.StyleName == p.StyleName).Single().MessageStyle.Type == MessageType.Popup).ToList();

            if (nextPopup.Where(p => p.PassInFront).Count() > 1) {
                bool debug = true;
            }

            Message forcedMessage = nextPopup.Where(p => p.PassInFront).FirstOrDefault();

            //if we got popup, we should it now
            if (forcedMessage != null || (nextPopup != null && nextPopup.Count > 0 && IsAutoDispatching))
            {

                ShowMessage(forcedMessage != null ? forcedMessage : nextPopup[0]);
            }
            else
            {


                //loop each panel (style) with ticker
                List<MessagePanel> allTickerStyle = Styles.Where(p => p.MessageStyle.Type == MessageType.Ticker).ToList();
                foreach (MessagePanel tickerPanel in allTickerStyle)
                {

                    //get all available ticker for this style
                    List<Message> allTickerForStyle = waitingMessages.Where(p => p.StyleName == tickerPanel.MessageStyle.StyleName).ToList();

                    //if there is a ticker available for this style
                    if (allTickerForStyle.Count > 0)
                    {

                        //get latest history ticker for this style
                        //MessageHistory lastTicker = historyMessages.Where(p => p.Type == MessageType.Ticker && p.StyleName == tickerPanel.MessageStyle.StyleName).LastOrDefault();

                        //check if there is already a message with same type too
                        DateTime lastAppearTime = DateTime.MinValue;
                        Message lastMessage = currentMessages.Where(p => p.StyleName == tickerPanel.MessageStyle.StyleName).LastOrDefault();
                        
                        if(lastMessage == null)
                            lastMessage = disappearMessages.Where(p => p.StyleName == tickerPanel.MessageStyle.StyleName).LastOrDefault();



                        if (lastMessage == null)
                        {
                            var lastHistoryMessage = historyMessages.Where(p => p.StyleName == tickerPanel.MessageStyle.StyleName).LastOrDefault();
                            if(lastHistoryMessage != null)
                                lastAppearTime = lastHistoryMessage.Time;
                        } else
                        {
                            lastAppearTime = lastMessage.AppearTime;
                        }


                        //if waiting message, we show it if there is enough time pass between last
                        if (lastAppearTime == DateTime.MinValue || ((DateTime.Now - lastAppearTime).TotalSeconds > tickerPanel.MessageStyle.TimeBetweenMessage))
                        {
                            
                            

                            ShowMessage(allTickerForStyle[0]);

                        }

                    }
                }






            }


        }
    }

    [Serializable]
    public class MessageLayer
    {
        [HideInInspector]
        public GameObject Layer;

        [HideInInspector]
        public CanvasGroup Canvas;
        [HideInInspector]
        public Image Back;

        public float BackAlpha = 0.5f;

        public int Order;
        public string Name;

        public bool OverwriteCanvas;
        public string OverwriteCanvasSortingLayer;
    }
}
#endif