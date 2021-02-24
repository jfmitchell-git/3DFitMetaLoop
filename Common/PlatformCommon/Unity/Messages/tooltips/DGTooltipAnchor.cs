#if !BACKOFFICE

using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{
    public class DGTooltipAnchor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        public MessageTooltip TooltipInfo;

   
        public float AppearDelay = 0.25f;

        [HideInInspector]
        public Tween AppearTween;

        private bool isMouseOver;


        public UnityEvent OnTooltipAppear;

        public DGTooltipAnchor()
        {
            if (TooltipInfo == null) TooltipInfo = new MessageTooltip();
        }

        void Awake()
        {

            OnTooltipAppear = new UnityEvent();

            //Needed when compononent is created by code.
            if (TooltipInfo == null) TooltipInfo = new MessageTooltip();

            if (TooltipInfo.MessageId == "" || TooltipInfo.MessageId == null)
                TooltipInfo.MessageId = Utils.Utils.RandomString(6);

            if (TooltipInfo.Anchor == null)
                TooltipInfo.Anchor = this.gameObject;
        }



        public void AppearTooltip()
        {

            KillTween();

            OnTooltipAppear.Invoke();

            MessageManager.Instance.AddMessage(TooltipInfo);
            //TooltipInfo.Place(MessageManager.Instance.MessageCamera);
        }

        private void KillTween()
        {

            //kill tween and tooltip
            if (AppearTween != null)
            {
                AppearTween.Kill();
                AppearTween = null;
            }

        }

        void OnMouseOver()
        {

            return;

            Button anchorButton = this.GetComponent<Button>();
            if(anchorButton && !anchorButton.enabled)
            {
                return;
            }

            if (isMouseOver)
                return;

            KillTween();
            AppearTween = DOVirtual.DelayedCall(AppearDelay, AppearTooltip);

            isMouseOver = true;
        }

        void OnMouseExit()
        {

            KillTween();
            if (!isMouseOver) return;

            //closing the message.....
            MessageManager.Instance.CloseMessage(TooltipInfo);

            isMouseOver = false;

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseOver();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseExit();
        }

        void OnMouseDown()
        {
           // Debug.Log("nice");
        }

        void OnDisable()
        {
            OnMouseExit();
        }

        void OnDestroy()
        {
            KillTween();

            if(MessageManager.Instance != null)
                MessageManager.Instance.CloseMessage(TooltipInfo);
        }

    }
}
#endif