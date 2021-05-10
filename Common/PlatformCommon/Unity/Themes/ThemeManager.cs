#if !BACKOFFICE
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MetaLoop.Common.PlatformCommon.Unity.Themes
{

    
    [System.Serializable]
    public class ThemeColorInfo
    {
        public string Name;
        public Color Color;
    }
    

    [System.Serializable]
    public struct ThemeInfo
    {
        public string Name;
        public int ThemeType;
        public string ImagePath;
        public List<ThemeColorInfo> AllColors;

    }

    [ExecuteInEditMode]
    public class ThemeManager : MonoBehaviour
    {
        public List<string> ThemeType;
        public List<string> AllColors;
        public List<ThemeInfo> AllThemes;

        public ThemeInfo CurrentTheme;
        public int CurrentThemeIndex = -1;

        public static ThemeManager Instance;

        [HideInInspector]
        public ThemeInfo CopyTheme;

        public ThemeElementAnimator Background;

        public UnityEvent OnThemeUpdate;
        public UnityEvent OnThemeFinishUpdate;

        public float ThemeTransitionSpeed = 0.5f;
        public float ThemeTransitionDelay = 0f;

        public void Awake()
        {
           
            Instance = this;

            UpdateThemeListener();

        }

        private void UpdateThemeListener()
        {
            if (Background != null)
            {
                Background.OnAnimationFinish.RemoveListener(BackgroundAnimationFinish);
                Background.OnAnimationFinish.AddListener(BackgroundAnimationFinish);
            }

        }

        private void BackgroundAnimationFinish()
        {
            OnThemeFinishUpdate.Invoke();
        }

        public ThemeInfo GetTheme(string themeName)
        {
            return AllThemes.Where(p => p.Name == themeName).Single();
        }

        public ThemeInfo SetTheme(string themeName,float speed = 0.5f)
        {

#if UNITY_STANDALONE
            themeName = "Green";
#endif
            Debug.Log("SET THEME = " + themeName);

            ThemeTransitionSpeed = speed;

            CurrentTheme = AllThemes.Where(p => p.Name == themeName).Single();
            CurrentThemeIndex = AllThemes.IndexOf(CurrentTheme, 0);

            Debug.Log("current Theme = " + CurrentTheme.Name);

            //this is going to tell everybody to update their theme, including the background we are listening too
            


            //if no background, it's finish after the time we select
            if(Background == null)
            {

                OnThemeUpdate.Invoke();

                DOVirtual.DelayedCall(speed, () => {
                    OnThemeFinishUpdate.Invoke();
                });
              
            } else
            {
                //maybe we should only do that if there is a animation

                if (speed > 0f)
                {
                    Background.SetNextAnimationInfo();
                    ThemeTransitionDelay = Background.CurrentAnimationInfo.Delay;

                }

                OnThemeUpdate.Invoke();
              
            }

            return CurrentTheme;

        }

        public void Update()
        {
            if(!Application.isPlaying)
            {
                if (Instance == null)
                    Instance = this;

                UpdateThemeListener();
            }
        }

    }
}
#endif
