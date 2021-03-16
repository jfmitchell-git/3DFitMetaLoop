#if !BACKOFFICE
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

        public UnityEvent OnThemeUpdate;

        public void Awake()
        {
           
            Instance = this;
        }

        public void SetTheme(string themeName)
        {
            Debug.Log("SET THEME = " + themeName);

            CurrentTheme = AllThemes.Where(p => p.Name == themeName).Single();
            CurrentThemeIndex = AllThemes.IndexOf(CurrentTheme, 0);

            Debug.Log("current Theme = " + CurrentTheme.Name);
            OnThemeUpdate.Invoke();
        }

        public void Update()
        {
            if(!Application.isPlaying)
            {
                if (Instance == null)
                    Instance = this;
            }
        }

    }
}
#endif
