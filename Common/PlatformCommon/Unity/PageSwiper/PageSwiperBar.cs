#if !BACKOFFICE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.PageSwiper
{
    public class PageSwiperBar : MonoBehaviour
    {

        public List<PageSwiperBarPage> AllPageBar;


        public void SetMaxPage(int maxPage)
        {
            for (int i = 0; i < AllPageBar.Count; i++)
            {
                AllPageBar[i].gameObject.SetActive((i+1) <= maxPage);
            }
        }

        public void SetCurrentPage(int currentPage)
        {

            for(int i = 0; i < AllPageBar.Count;i++)
            {
                AllPageBar[i].SetPage((i+1) == currentPage);
            }

        }
     
    }
}
#endif
