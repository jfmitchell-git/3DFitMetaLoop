using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MetaLoop.Common.PlatformCommon.Unity.Optimization
{


    [System.Serializable]
    public class CanvasResizerInfo
    {
        public ScreenRatio ScreenRatio;
        public float Resize;
        public bool EverythingBelow;
        public bool EverythingOver;
    }

    [ExecuteInEditMode]
    public class CanvasResizer : MonoBehaviour
    {

        public Vector2 ReferenceSize;

        [HideInInspector]
        public CanvasScaler CanvasScaler;
        [HideInInspector]
        public Canvas Canvas;

        public List<CanvasResizerInfo> CanvasResizerInfo;

        void Awake()
        {


            UpdateListener();
        }


        private void ResizeScreen()
        {

          
            CanvasScaler.referenceResolution = new Vector2(ReferenceSize.x, ReferenceSize.y);

            //PerformanceManager.Instance.ScreenRatio


            CanvasResizerInfo useInfo = CanvasResizerInfo.Where(p => p.ScreenRatio == PerformanceManager.Instance.ScreenRatio).SingleOrDefault();

            if(useInfo != null)
            {
                Debug.Log("FOUND INFO");


            } else if(CanvasResizerInfo.Count > 0)
            {



                Debug.Log("DIDNT FOUND INFO");

                string[] allScreenRatio = System.Enum.GetNames(typeof(ScreenRatio));

                //we check if we got an available ratio up 
                for (int i = allScreenRatio.ToList().IndexOf(PerformanceManager.Instance.ScreenRatio.ToString(),0) ; i < allScreenRatio.Length; i++)
                {
                    useInfo = CanvasResizerInfo.Where(p => p.ScreenRatio.ToString() == allScreenRatio[i].ToString()).SingleOrDefault();
                    
                    if(useInfo != null)
                    {
                        if(useInfo.EverythingBelow)
                        {
                            //TADA
                            Debug.Log("COOOOOL");


                        } else
                        {
                            useInfo = null;
                        }

                        break;
                    }

                }



            }



           if(useInfo != null)
           {
                CanvasScaler.referenceResolution = new Vector2(ReferenceSize.x, ReferenceSize.y/ useInfo.Resize);

                Canvas.ForceUpdateCanvases();
            }




        }

        void UpdateListener()
        {

            CanvasScaler = this.GetComponent<CanvasScaler>();
            Canvas = this.GetComponent<Canvas>();

            if (PerformanceManager.Instance == null) return;

            PerformanceManager.Instance.ScreenSizeChanged.RemoveListener(ResizeScreen);
            PerformanceManager.Instance.ScreenSizeChanged.AddListener(ResizeScreen);
        }


        
       

        private void OnDestroy()
        {
            if (PerformanceManager.Instance == null) return;
                PerformanceManager.Instance.ScreenSizeChanged.RemoveListener(ResizeScreen);

        }

       

        // Update is called once per frame
        void Update()
        {

            if(!Application.isPlaying)
                UpdateListener();


        }
    }
}
