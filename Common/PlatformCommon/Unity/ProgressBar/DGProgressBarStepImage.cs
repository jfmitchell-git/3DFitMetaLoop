#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MetaLoop.Common.PlatformCommon.Unity.ProgressBar
{
    [System.Serializable]
    public class DGProgressBarStepImage
    {
        public bool ColorizeImage = true;
        public bool ResizeImage = true;
        public bool IsFighting = false;
        public Gradient Color;
        public Image Image;
        public RectTransform RectTransform;
        public RectTransform RectTransformMask;
        public float Alpha = 1f;

    }
}
#endif