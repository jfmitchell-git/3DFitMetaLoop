#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MetaLoop.Common.PlatformCommon.Unity.ProgressBar
{
   
    public class DGProgressBarStep : MonoBehaviour
    {
        public string Name;
        public List<DGProgressBarStepImage> Images;
        public RectTransform StepImageRectTransform;

        private void Awake()
        {
            StepImageRectTransform = this.GetComponent<RectTransform>();
        }

    }
}
#endif