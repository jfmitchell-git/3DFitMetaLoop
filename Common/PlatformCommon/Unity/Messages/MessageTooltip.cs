#if !BACKOFFICE

using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


namespace MetaLoop.Common.PlatformCommon.Unity.Messages
{
    [Serializable]
    public class MessageTooltip : Message
    {

        public DGTooltipPosition ToolTipPosition;
        public string ToolTipId;

        private DGTooltipPosition usePosition;

        [HideInInspector]
        public GameObject Anchor;

        private GameObject styleUsed;


        private Camera keepCamera;

        void Awake()
        {
            Debug.Log("arg");
        }


        void Update()
        {
            if(keepCamera != null)
             Place(keepCamera);
        }

        public void Kill()
        {
            keepCamera = null;
            MessagePanel.OnUpdate.RemoveListener(Update);
            MessagePanel.OnClose.RemoveListener(Kill);
        }

        /// <summary>
        /// Place the tooltip at correct place in screen
        /// </summary>
        /// <param name="camera"></param>
        public void Place(Camera camera)
        {


            //new checkup IN CASE OF BUG
            if(((DGTooltip)MessagePanel) == null || ((DGTooltip)MessagePanel).BottomMiddle == null)
            {
                MessageManager.Instance.CloseMessage(this);
                return;
            }
            

            //udpate position
            if (ToolTipPosition == DGTooltipPosition.FollowMouse && keepCamera == null)
            {
                MessagePanel.OnUpdate.AddListener(Update);
                MessagePanel.OnClose.AddListener(Kill);
            }

            keepCamera = camera;

            


            //if no anchor, we return
            if (Anchor == null) return;


            //not sure about performance
            if (ToolTipPosition != DGTooltipPosition.FollowMouse)
                Canvas.ForceUpdateCanvases();

            usePosition = ToolTipPosition;

            if(ToolTipPosition == DGTooltipPosition.FollowMouse)
            {
                usePosition = DGTooltipPosition.RightMiddle;
            }

            //set correctly style
            SetStyle();


            //in case
            camera.enabled = true;
            camera.gameObject.SetActive(true);

            //tooltip rectTransform
            RectTransform tooltipRect = this.MessagePanel.GetComponent<RectTransform>();

            //check if rectTransform
            RectTransform anchorRectTransform = Anchor.GetComponent<RectTransform>();
            Vector3[] anchorCorner = new Vector3[4];

            //canvas
            Canvas canvas = GetCanvasFromObject(this.MessagePanel.gameObject.transform.parent.gameObject);
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            if(ToolTipPosition == DGTooltipPosition.FollowMouse)
            {
                int mousePadding = 20;
                anchorCorner[0] = new Vector3(Input.mousePosition.x - mousePadding, Input.mousePosition.y - mousePadding);
                anchorCorner[1] = new Vector3(Input.mousePosition.x - mousePadding, Input.mousePosition.y + mousePadding);
                anchorCorner[2] = new Vector3(Input.mousePosition.x + mousePadding, Input.mousePosition.y + mousePadding);
                anchorCorner[3] = new Vector3(Input.mousePosition.x + mousePadding, Input.mousePosition.y - mousePadding);

            } else if (anchorRectTransform)
            {

                //corner of the anchor element//////////////////////////
                anchorRectTransform.GetWorldCorners(anchorCorner);
                /////////////////////////////////////////////////////////////

            } else //3d object
            {

                //3d object
                // We're not using a trigger from a Canvas, so that means it's a regular world space game object.
                Vector3 center = Vector3.zero;
                Vector3 extents = Vector3.zero;

                //check if colliders
                Collider coll = Anchor.GetComponent<Collider>();
                if (coll == null)
                {
                    coll = Anchor.GetComponentInChildren<Collider>();
                }

                if(coll != null)
                {
                    center = coll.bounds.center;
                    extents = coll.bounds.extents;

                } else
                {
                    Renderer rend = Anchor.GetComponent<Renderer>();
                    center = rend.bounds.center;
                    extents = rend.bounds.extents;
                }


                Vector3 frontBottomLeftCorner = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
                Vector3 frontTopLeftCorner = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
                Vector3 frontTopRightCorner = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
                Vector3 frontBottomRightCorner = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);


                //not sure about this ????
                anchorCorner[0] = frontBottomLeftCorner;
                anchorCorner[1] = frontTopLeftCorner;
                anchorCorner[2] = frontTopRightCorner;
                anchorCorner[3] = frontBottomRightCorner;

                /*
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    anchorCorner[0] = frontBottomLeftCorner;
                    anchorCorner[1] = frontTopLeftCorner;
                    anchorCorner[2] = frontTopRightCorner;
                    anchorCorner[3] = frontBottomRightCorner;
                }
                else
                {
                    anchorCorner[0] = camera.WorldToScreenPoint(frontBottomLeftCorner);
                    anchorCorner[1] = camera.WorldToScreenPoint(frontTopLeftCorner);
                    anchorCorner[2] = camera.WorldToScreenPoint(frontTopRightCorner);
                    anchorCorner[3] = camera.WorldToScreenPoint(frontBottomRightCorner);
                }*/


            }

            float canvasScale = canvas.scaleFactor;

            //test
            //canvasScale = 1;

            //not sure bout this
            Vector3 anchorCorner1 = camera.WorldToScreenPoint(anchorCorner[0]) / canvasScale;
            Vector3 anchorCorner2 = camera.WorldToScreenPoint(anchorCorner[1]) / canvasScale;
            Vector3 anchorCorner3 = camera.WorldToScreenPoint(anchorCorner[2]) / canvasScale;
            Vector3 anchorCorner4 = camera.WorldToScreenPoint(anchorCorner[3]) / canvasScale;

            if (ToolTipPosition == DGTooltipPosition.FollowMouse)
            {
                anchorCorner1 = anchorCorner[0] / canvasScale;
                anchorCorner2 = anchorCorner[1] / canvasScale;
                anchorCorner3 = anchorCorner[2] / canvasScale;
                anchorCorner4 = anchorCorner[3] / canvasScale;
            }

            //size of anchor
            Vector2 sizeOfAnchor = anchorCorner3 - anchorCorner1;
            Rect anchorPos = new Rect(anchorCorner1.x, anchorCorner1.y, sizeOfAnchor.x, sizeOfAnchor.y);

            //place tooltips
            //size of the tooltip/////////////////////////////////////
            Vector3[] tooltipCorner = new Vector3[4];
            RectTransform tooltipStyleRect = styleUsed.GetComponent<RectTransform>();
            tooltipStyleRect.GetWorldCorners(tooltipCorner);
           // Vector3 tooltipCorner1 = camera.WorldToScreenPoint(tooltipCorner[0]) / canvasScale;
           // Vector3 tooltipCorner3 = camera.WorldToScreenPoint(tooltipCorner[2]) / canvasScale;
           // Vector2 sizeOfTooltip = tooltipCorner3 - tooltipCorner1;
            /////////////////////////////////////////////////////////

            

            //check anchoredPosition of tooltips to get correct value
            Rect tooltipRectPos = tooltipRect.rect;
            Vector2 tooltipSize = new Vector2(tooltipRectPos.xMax - tooltipRectPos.xMin, tooltipRectPos.yMax - tooltipRectPos.yMin);

            //fix tooltipRectPos
            //tooltipRectPos.x = tooltipRectPos.x;
           // tooltipRectPos.y = tooltipRectPos.y;
            tooltipRectPos.width = tooltipSize.x;
            tooltipRectPos.height = tooltipSize.y;

            //auto add TIP offset


            //////////////////////////////////////////////////////////


            SetPosition(anchorPos, tooltipRectPos, tooltipStyleRect,canvasRect);

            //FIX FOR STUPID HINGES
            var fuckyou = Screen.width;
            var parentRect = this.MessagePanel.transform.parent.GetComponent<RectTransform>();
            var addToQuickFix = parentRect.rect.width * canvasScale;
            float finalVaChier = (addToQuickFix - fuckyou);
         



            //set tooltip to the end position to checkout the corners ?
            tooltipRect.anchoredPosition = MessagePanel.MessageStyle.EndPos;

            //check overflow
            Vector3[] tooltipCheckOverflowCorners = new Vector3[4];
            tooltipStyleRect.GetWorldCorners(tooltipCheckOverflowCorners);

            for(int i = 0 ; i < tooltipCheckOverflowCorners.Length ; i++)
            {
                tooltipCheckOverflowCorners[i] = RectTransformUtility.WorldToScreenPoint(camera, tooltipCheckOverflowCorners[i]);
            }

            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

            bool bottomLeftCorner = !screenRect.Contains(tooltipCheckOverflowCorners[0]);
            bool topLeftCorner = !screenRect.Contains(tooltipCheckOverflowCorners[1]);
            bool topRightCorner = !screenRect.Contains(tooltipCheckOverflowCorners[2]);
            bool bottomRightCorner = !screenRect.Contains(tooltipCheckOverflowCorners[3]);

            DGTooltipPosition newTooltipPosition = usePosition;

            //logical overflow here

            if(bottomLeftCorner || topLeftCorner || topRightCorner || bottomRightCorner)
            {

                Vector2 tooltipDiff = new Vector2(0, 0);

                //switch on position
                switch (ToolTipPosition)
                {

                    case DGTooltipPosition.LeftMiddle:

                        //engine will try to set outside
                        if (topLeftCorner && bottomLeftCorner)
                        {
                            newTooltipPosition = DGTooltipPosition.RightMiddle;
                        }
                        else
                        {

                            //only one coner is out so we will adjust the tooltip correctly!!! (engine is too hot)

                            GameObject reajustTooltipPart;
                            LayoutElement reajustTooltipElement;

                            if (topLeftCorner)
                            {
                                tooltipDiff.y = (screenRect.height - tooltipCheckOverflowCorners[1].x) / canvasScale;

                                //fix in style
                                reajustTooltipPart = styleUsed.transform.GetChild(0).gameObject;


                            }
                            else
                            {



                                tooltipDiff.y = -(screenRect.height - tooltipCheckOverflowCorners[0].y) / canvasScale;

                                //fix in style
                                reajustTooltipPart = styleUsed.transform.GetChild(2).gameObject;

                            }



                            reajustTooltipElement = reajustTooltipPart.GetComponent<LayoutElement>();

                            //remove dif to the width
                            float newHeight = reajustTooltipPart.GetComponent<RectTransform>().rect.height - Math.Abs(tooltipDiff.y);

                            //check minimum size
                            if (newHeight < ((DGTooltip)MessagePanel).MinimumSideSize)
                            {
                                newHeight = ((DGTooltip)MessagePanel).MinimumSideSize;
                            }

                            reajustTooltipElement.flexibleHeight = 0;
                            reajustTooltipElement.preferredHeight = newHeight;




                        }


                        break;

                    case DGTooltipPosition.BottomMiddle:

                        //engine will try to set outside
                       if(bottomLeftCorner && bottomRightCorner)
                        {
                            //need to change position
                            if(!topLeftCorner && !topRightCorner)
                            {

                                newTooltipPosition = DGTooltipPosition.TopMiddle;
                                //tooltipPos = SetPosition()
                            } else if(topLeftCorner)
                            {
                                newTooltipPosition = DGTooltipPosition.RightMiddle;
                            } else
                            {
                                newTooltipPosition = DGTooltipPosition.LeftMiddle;
                            }


                        } else
                        {

                            //only one coner is out so we will adjust the tooltip correctly!!! (engine is too hot)

                            GameObject reajustTooltipPart;
                            LayoutElement reajustTooltipElement;

                            if(bottomLeftCorner)
                            {
                                tooltipDiff.x = tooltipCheckOverflowCorners[0].x/canvasScale;

                                tooltipDiff.x += ((DGTooltip)MessagePanel).MaximumSideOutOfScreen;

                                //fix in style(it get the top part
                                reajustTooltipPart = styleUsed.transform.GetChild(0).gameObject;
                                

                            } else
                            {



                                tooltipDiff.x = -(screenRect.width - tooltipCheckOverflowCorners[2].x)/canvasScale;

                                tooltipDiff.x -= ((DGTooltip)MessagePanel).MaximumSideOutOfScreen;

                                //fix in style
                                reajustTooltipPart = styleUsed.transform.GetChild(2).gameObject;

                            }

                            

                            reajustTooltipElement = reajustTooltipPart.GetComponent<LayoutElement>();

                            //remove dif to the width
                            float newWidth = reajustTooltipPart.GetComponent<RectTransform>().rect.width - Math.Abs(tooltipDiff.x);

                            //check minimum size
                            if (newWidth < ((DGTooltip)MessagePanel).MinimumSideSize)
                            {
                                newWidth = ((DGTooltip)MessagePanel).MinimumSideSize;
                            }

                            reajustTooltipElement.flexibleWidth = 0;
                            reajustTooltipElement.preferredWidth = newWidth;
                            



                        }

                        break;



                }


                //fix with new style and position
                if (newTooltipPosition != ToolTipPosition)
                {
                    SetStyle();
                    SetPosition(anchorPos, tooltipRectPos, tooltipStyleRect,canvasRect);
                }

                //probabaly an else here for this
                MessagePanel.MessageStyle.EndPos -= tooltipDiff;
                MessagePanel.MessageStyle.StartPos -= tooltipDiff;



            }

           if(Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.Portrait)
            {

                MessagePanel.MessageStyle.EndPos.x += finalVaChier / 2;
                MessagePanel.MessageStyle.StartPos.x += finalVaChier / 2;

            } else
            {
                MessagePanel.MessageStyle.EndPos.x -= finalVaChier / 2;
                MessagePanel.MessageStyle.StartPos.x -= finalVaChier / 2;
            }
            


            //set start position
            tooltipRect.anchoredPosition = MessagePanel.MessageStyle.StartPos;

            //do the motion, and oncomplete we wait
            tooltipRect.DOAnchorPos(MessagePanel.MessageStyle.EndPos, MessagePanel.TransitionTime);
            this.MessagePanel.gameObject.SetActive(true);


            //overflow checkup



        }



        private void SetStyle()
        {

           

            //set all tip invisible
            ((DGTooltip)MessagePanel).BottomMiddle.SetActive(false);
            ((DGTooltip)MessagePanel).TopMiddle.SetActive(false);
            ((DGTooltip)MessagePanel).LeftMiddle.SetActive(false);
            ((DGTooltip)MessagePanel).RightMiddle.SetActive(false);

            switch(usePosition)
            {

                case DGTooltipPosition.BottomMiddle:
                    styleUsed = ((DGTooltip)MessagePanel).BottomMiddle;
                    
                    break;

                case DGTooltipPosition.TopMiddle:
                    styleUsed = ((DGTooltip)MessagePanel).TopMiddle;
                    
                    break;

                case DGTooltipPosition.LeftMiddle:
                    styleUsed = ((DGTooltip)MessagePanel).LeftMiddle;
                    
                    break;

                case DGTooltipPosition.RightMiddle:
                    styleUsed = ((DGTooltip)MessagePanel).RightMiddle;
                   
                    break;

            }

            styleUsed.SetActive(true);

        }


        private void SetPosition(Rect anchorPos,Rect tooltipRectPos,RectTransform tooltipStyleRect,RectTransform canvasRect)
        {

            MessagePanel.MessageStyle.StartPos = new Vector2();
            MessagePanel.MessageStyle.EndPos = new Vector2();

            Vector2 offset = new Vector2(0, 0);

           

            switch (usePosition)
            {
                case DGTooltipPosition.LeftMiddle:

                    MessagePanel.MessageStyle.EndPos.x = anchorPos.x + tooltipRectPos.x - tooltipStyleRect.offsetMax.x;
                    MessagePanel.MessageStyle.EndPos.y = anchorPos.y + anchorPos.height / 2 - tooltipRectPos.y - tooltipRectPos.height / 2;

                    offset.x = -((DGTooltip)MessagePanel).AnimationOffset;

                    break;

                case DGTooltipPosition.RightMiddle:

                    //offset of tip = tooltipStyleRect.offsetMin.x

                    MessagePanel.MessageStyle.EndPos.x = anchorPos.x + anchorPos.width - tooltipRectPos.x - tooltipStyleRect.offsetMin.x;
                    MessagePanel.MessageStyle.EndPos.y = anchorPos.y + anchorPos.height / 2 - tooltipRectPos.y - tooltipRectPos.height / 2;

                    offset.x = ((DGTooltip)MessagePanel).AnimationOffset;

                    break;

                case DGTooltipPosition.TopMiddle:

                    MessagePanel.MessageStyle.EndPos.x = anchorPos.x + anchorPos.width / 2 + (tooltipRectPos.x + tooltipRectPos.width / 2);
                    MessagePanel.MessageStyle.EndPos.y = anchorPos.y + anchorPos.height + (tooltipRectPos.y + tooltipRectPos.height) - tooltipStyleRect.offsetMin.y; //- (tooltipStyleRect.offsetMax.y + tooltipStyleRect.offsetMin.y);

                    offset.y = ((DGTooltip)MessagePanel).AnimationOffset;

                    break;

                case DGTooltipPosition.BottomMiddle:

                    MessagePanel.MessageStyle.EndPos.x = anchorPos.x + anchorPos.width / 2 + (tooltipRectPos.x + tooltipRectPos.width / 2);
                    MessagePanel.MessageStyle.EndPos.y = anchorPos.y - (tooltipRectPos.y + tooltipRectPos.height) - tooltipStyleRect.offsetMax.y;

                    offset.y = -((DGTooltip)MessagePanel).AnimationOffset;

                    break;
            }


            //always remove half the screen because canvas point is at center of screen and we need bottom-left
            MessagePanel.MessageStyle.EndPos.x -= (canvasRect.sizeDelta.x / 2);
            MessagePanel.MessageStyle.EndPos.y -= (canvasRect.sizeDelta.y / 2);


            //set start position
            MessagePanel.MessageStyle.StartPos = MessagePanel.MessageStyle.EndPos + offset;
            



        }


        private Canvas GetCanvasFromObject(GameObject gameObject)
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();

            while(canvas == null && gameObject != null)
            {
                gameObject = gameObject.transform.parent.gameObject;
                if(gameObject)
                    canvas = gameObject.GetComponent<Canvas>();
                
            }

            return canvas;

        }

    
      



    }
}
#endif