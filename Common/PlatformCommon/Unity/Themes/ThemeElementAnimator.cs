#if !BACKOFFICE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MetaLoop.Common.PlatformCommon.Unity.Themes
{

    [System.Serializable]
    public class AnimationInfo
    {
        public string AnimationName;
        public float Delay;
    }

    [ExecuteInEditMode]
    public class ThemeElementAnimator : MonoBehaviour
    {

        public List<AnimationInfo> AnimationInfo;

        public Animator ThemeAnimator;
        public MeshRenderer Image1;
        public MeshRenderer Image2;

        public UnityEvent OnAnimationFinish;

        public AnimationInfo CurrentAnimationInfo;

        void Awake()
        {

            UpdateThemeListener();

           // UpdateImage(true);

            // Debug.Log("AWAKE THEME ELEMENT " + this.name);

        }

        public void SetNextAnimationInfo()
        {
            int currentIndex = 0;
            if (CurrentAnimationInfo != null)
            {
                currentIndex = AnimationInfo.IndexOf(CurrentAnimationInfo, 0) + 1;
                if (currentIndex >= AnimationInfo.Count)
                    currentIndex = 0;
            }
            CurrentAnimationInfo = AnimationInfo[currentIndex];

        }

        private void UpdateThemeListener()
        {
            ThemeManager.Instance.OnThemeUpdate.RemoveListener(ThemeUpdate);
            ThemeManager.Instance.OnThemeUpdate.AddListener(ThemeUpdate);
        }

        void Update()
        {
          
            //not sure about this (its werid.. why i need this?)
            if (!Application.isPlaying)
            {
                UpdateThemeListener();
            }

        }

        public void ThemeUpdate()
        {
           
            //we only animate if we are playing and transition is more than 0f (0f mean no transition)
            UpdateImage(Application.isPlaying && ThemeManager.Instance.ThemeTransitionSpeed != 0f);

        }

        public void UpdateImage(bool animated)
        {

            StartCoroutine(SetImage(animated));

        }

        public IEnumerator SetImage(bool animated = false)
        {
            // Debug.Log("LOADING THEME");

           ResourceRequest resourceRequest = Resources.LoadAsync<Texture2D>(ThemeManager.Instance.CurrentTheme.ImagePath);

            while (!resourceRequest.isDone)
            {
                yield return 0;
            }

            if(animated)
            {
                //in case
                if (CurrentAnimationInfo == null)
                    CurrentAnimationInfo = AnimationInfo[0];

                Image2.sharedMaterial.SetTexture("_BaseMap", resourceRequest.asset as Texture2D);

                ThemeAnimator.speed = 1f;
                ThemeAnimator.Play(CurrentAnimationInfo.AnimationName);
            } else
            {
                //both image same in case
                Image1.sharedMaterial.SetTexture("_BaseMap", resourceRequest.asset as Texture2D);
                Image2.sharedMaterial.SetTexture("_BaseMap", Image1.sharedMaterial.GetTexture("_BaseMap"));

                AnimationFinish();
            }
          

        }


        public void AnimationFinish()
        {
            //when animation is finish, we set on the image 1 the correct image and we set back the time to 0
            Image1.sharedMaterial.SetTexture("_BaseMap",Image2.sharedMaterial.GetTexture("_BaseMap"));

            //we reset to tstart
            ThemeAnimator.speed = 0f;
            ThemeAnimator.Play(AnimationInfo[0].AnimationName, 0,0f);

            //transition is finish.. if someone listen (ThemeManager shoudl listen to the background)
            OnAnimationFinish.Invoke();
        }


    }
}
#endif
