#if !BACKOFFICE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MetaLoop.Common.PlatformCommon.Unity.PageSwiper
{
    public class PageSwiperBarPage : MonoBehaviour
    {
        public GameObject PageOn;
        public GameObject PageOff;


       

        public void SetPage(bool on)
        {

   
            if (on && PageOn.activeSelf) return;
            if (!on && PageOff.activeSelf) return;

            PageOn.SetActive(on);
            PageOff.SetActive(!on);

        }

    }
}
#endif
