#if !BACKOFFICE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MetaLoop.Common.PlatformCommon.Unity.Utils;
using DG.Tweening;

namespace MetaLoop.Common.PlatformCommon.Unity.Themes
{




    [ExecuteInEditMode]
    public class ThemeElement : MonoBehaviour
    {
        //automaticlaly detect TEXT vs IMAGE
        [HideInInspector]
        public Image Image;
        [HideInInspector]
        public TextMeshProUGUI Text;
        [HideInInspector]
        public UnityEngine.UI.Extensions.Gradient Gradient;
        [HideInInspector]
        public SpriteRenderer SpriteRenderer;

        //public ThemeColorInfo CurrentThemeColorInfo;
        public int CurrentThemeColorInfoIndex1 = -1;
        public int CurrentThemeColorInfoIndex2 = -1;

        public float ColorBrightness1 = 0;
        public float ColorBrightness2 = 0;

        private bool isDetected;

        void Awake()
        {

            UpdateThemeListener();
            DetectElement();
            UpdateColor(true);


           // Debug.Log("AWAKE THEME ELEMENT " + this.name);
            

        }

        private void UpdateThemeListener()
        {
            ThemeManager.Instance.OnThemeUpdate.RemoveListener(ThemeUpdate);
            ThemeManager.Instance.OnThemeUpdate.AddListener(ThemeUpdate);
        }

        // Update is called once per frame
        void Update()
        {
            if(!isDetected)
                DetectElement();


            //not sure about this (its werid.. why i need this?)
            if (!Application.isPlaying)
            {
                UpdateThemeListener();
            }

        }

        public void ThemeUpdate()
        {
            if(ThemeManager.Instance.ThemeTransitionDelay != 0f)
            {
                DOVirtual.DelayedCall(ThemeManager.Instance.ThemeTransitionDelay, () => UpdateColor(false));
            } else
            {
                UpdateColor(false);
            }
           
          
        }

        public void UpdateColor(bool immediate = false)
        {

          //  Debug.Log("UPDATE COLOR THEME ELEMENT " + this.name + " " + CurrentThemeColorInfoIndex1 + " " + ThemeManager.Instance.CurrentTheme.Name);

            if (CurrentThemeColorInfoIndex1 == -1) return;

            ThemeColorInfo CurrentThemeColorInfo = ThemeManager.Instance.CurrentTheme.AllColors[CurrentThemeColorInfoIndex1];
            var useColor1 = ColorUtils.ChangeColorBrightness(CurrentThemeColorInfo.Color, ColorBrightness1);

            if (CurrentThemeColorInfoIndex2 == -1)
                CurrentThemeColorInfoIndex2 = 0;

            ThemeColorInfo CurrentThemeColorInfo2 = ThemeManager.Instance.CurrentTheme.AllColors[CurrentThemeColorInfoIndex2];
            var useColor2 = ColorUtils.ChangeColorBrightness(CurrentThemeColorInfo2.Color, ColorBrightness2);

          //  if (!Application.isPlaying || force)
           // {

                if (CurrentThemeColorInfo == null) return;


                if(Gradient != null)
                {

                    if (Application.isPlaying && !immediate)
                    {
                        DOTween.To(() => Gradient.Vertex1, x => Gradient.Vertex1 = x, useColor1, ThemeManager.Instance.ThemeTransitionSpeed);
                        DOTween.To(() => Gradient.Vertex2, x => Gradient.Vertex2 = x, useColor2, ThemeManager.Instance.ThemeTransitionSpeed);
                    } else
                    {
                        Gradient.Vertex1 = useColor1;
                        Gradient.Vertex2 = useColor2;
                    }

                   

                    if (Image.enabled)
                    {
                        Image.enabled = false;
                        Image.enabled = true;
                    }

                } else
                {
                    if (Image != null)
                    {
                        if (Application.isPlaying && !immediate)
                        {
                            Image.DOColor(useColor1, ThemeManager.Instance.ThemeTransitionSpeed);
                        } else
                        {
                            Image.color = useColor1;
                        }


                        if (Image.enabled)
                        {
                            Image.enabled = false;
                            Image.enabled = true;
                        }
                    }
                }

                if (Text != null)
                {
                    if (Application.isPlaying && !immediate)
                    {
                        Text.DOColor(useColor1, ThemeManager.Instance.ThemeTransitionSpeed);
                    }
                    else
                    {
                        Text.color = useColor1;
                    }

                    if (Text.enabled)
                    {
                        Text.enabled = false;
                        Text.enabled = true;
                    }

                }

                if(SpriteRenderer != null)
                {
                    if (Application.isPlaying && !immediate)
                    {
                        SpriteRenderer.DOColor(useColor1, ThemeManager.Instance.ThemeTransitionSpeed);
                    } else
                    {
                        SpriteRenderer.color = useColor1;
                    }
                       

                    if (SpriteRenderer.enabled)
                    {
                        SpriteRenderer.enabled = false;
                        SpriteRenderer.enabled = true;
                    }
                }



          //  }


        }

        private void DetectElement()
        {

            if (Image == null && Text == null && Gradient == null && SpriteRenderer == null)
            {
                Gradient = this.GetComponent<UnityEngine.UI.Extensions.Gradient>();
                Image = this.GetComponent<Image>();
                Text = this.GetComponent<TextMeshProUGUI>();
                SpriteRenderer = this.GetComponent<SpriteRenderer>();
                isDetected = true;

            }
        }

    }
}
#endif