#if !BACKOFFICE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

namespace MetaLoop.Common.PlatformCommon.Unity.PageSwiper
{

    [ExecuteInEditMode]
    public class PageSwiperManager : MonoBehaviour, IDragHandler, IEndDragHandler
    {

        private Vector3 panelLocation;
        public float PercentThreshold = 0.4f;
        public bool HidePageBarIfOne = true;
        public List<PageSwiperPage> AllPages;

        public RectTransform RectTransform;

        //for testing purpocse
        public Canvas UseCanvas;

        public PageSwiperBar PageSwiperBar;

        private float pageWidth;

        [HideInInspector]
        public int CurrentPage = 1;

        [HideInInspector]
        public int CurrentMaxPage = 1;

        // Start is called before the first frame update
        void Awake()
        {
            RectTransform = this.GetComponent<RectTransform>();
            panelLocation = RectTransform.anchoredPosition;

            pageWidth = Screen.width / UseCanvas.scaleFactor;

            ResizePage();

        }

        public void SetCurrentPage(int currentPage,float speed = 0.25f)
        {
            CurrentPage = currentPage;
            PageSwiperBar.SetCurrentPage(CurrentPage);

            Vector3 newLocation = panelLocation;

            newLocation.x = -((CurrentPage - 1) * pageWidth);

            if(speed == 0f)
            {
                RectTransform.anchoredPosition = newLocation;
            } else
            {
                RectTransform.DOAnchorPos(newLocation, 0.25f);
            }

            panelLocation = newLocation;
        }

        public void SetMaxPage(int maxPage)
        {
            CurrentMaxPage = maxPage;
            PageSwiperBar.SetMaxPage(maxPage);

            PageSwiperBar.gameObject.SetActive(!(HidePageBarIfOne && maxPage == 1) || maxPage>1);
           
        }

        // Update is called once per frame
        void Update()
        {

            if(!Application.isPlaying)
            {
                pageWidth = Screen.width / UseCanvas.scaleFactor;
                ResizePage();
               

            }

        }

        public void ResizePage()
        {
            float startPos = 0f;
           
            foreach (var pages in AllPages)
            {

                pages.RectTransform.sizeDelta = new Vector2(pageWidth, pages.RectTransform.sizeDelta.y);
                pages.RectTransform.anchoredPosition = new Vector2(startPos, pages.RectTransform.anchoredPosition.y);
                startPos += pageWidth;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            float difference = (eventData.pressPosition.x - eventData.position.x)/UseCanvas.scaleFactor;


            //inertia
            Vector3 endValue = panelLocation - new Vector3(difference, 0, 0);

            float maxInertia = pageWidth/2;
            float maxContainerSize = (CurrentMaxPage-1) * pageWidth;
            //maximum inertia (left drag)
            if(endValue.x > 0f)
            {
                float diff = maxInertia - endValue.x;
                float inertiaPercent = maxInertia / diff;
              

                if(inertiaPercent >=2f || inertiaPercent < 0f)
                {
                    endValue.x = maxInertia/4f;
                } else
                {
                    float inertia = endValue.x / (maxInertia / diff);
                    endValue.x = inertia;
                }
            }

            if(Math.Abs(endValue.x) > maxContainerSize)
            {
                float rest = Math.Abs(endValue.x) - maxContainerSize;

                //too big
                float diff = maxInertia - rest;
                float inertiaPercent = maxInertia / diff;

                if (inertiaPercent >= 2f || inertiaPercent < 0f)
                {
                    endValue.x = -(maxContainerSize) - (maxInertia / 4f);
                }
                else
                {
                    float inertia = -(maxContainerSize) - (rest / (maxInertia / diff));
                    endValue.x = inertia;
                }
            }

            RectTransform.DOAnchorPos(endValue, 0.1f);
        }

        public void OnEndDrag(PointerEventData eventData)
        {

            float percentage = ((eventData.pressPosition.x - eventData.position.x) / UseCanvas.scaleFactor) / pageWidth;
            Vector3 newLocation = panelLocation;
            if (Mathf.Abs(percentage) >= PercentThreshold)
            {
                if (percentage > 0)
                {
                    newLocation += new Vector3(-pageWidth, 0, 0);
                } else
                {
                    newLocation += new Vector3(pageWidth, 0, 0);
                }
            }

            //fix if under page 1
            if (newLocation.x > 0f)
                newLocation.x = 0f;

            CurrentPage = Convert.ToInt32(Math.Abs(newLocation.x)/pageWidth) + 1;
           
            if(CurrentPage>CurrentMaxPage)
            {
                CurrentPage = CurrentMaxPage;
                //newLocation.x = -((CurrentMaxPage-1) * pageWidth);
            }

            SetCurrentPage(CurrentPage);
            /*
            RectTransform.DOAnchorPos(newLocation, 0.25f);
            panelLocation = newLocation;
            PageSwiperBar.SetCurrentPage(CurrentPage);*/

        }

    }
}
#endif