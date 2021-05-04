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
    public class ResizerOnScreenRatio : MonoBehaviour
    {

       
        
        public CanvasScaler CanvasScaler;
        public Canvas Canvas;

        public List<CanvasResizerInfo> CanvasResizerInfo;
        public Vector2 ReferenceSize;

        private bool isDetected;

        public Transform Transform;
        public bool UseXZForScale = false;

        void Awake()
        {

            DetectElement();
            UpdateListener();


        }

        private void OnEnable()
        {
            DetectElement();
            UpdateListener();
        }


        private void ResizeScreen()
        {

            if (!this.enabled) return;
            
            if(Canvas != null)
            {
                CanvasScaler.referenceResolution = new Vector2(ReferenceSize.x, ReferenceSize.y);
            } else if(Transform != null)
            {

            }



            CanvasResizerInfo useInfo = CanvasResizerInfo.Where(p => p.ScreenRatio == PerformanceManager.Instance.ScreenRatio).SingleOrDefault();
            if (useInfo != null)
            {
                //Debug.Log("FOUND INFO");
            }
            else if (CanvasResizerInfo.Count > 0)
            {



                //Debug.Log("DIDNT FOUND INFO");

                string[] allScreenRatio = System.Enum.GetNames(typeof(ScreenRatio));

                //we check if we got an available ratio up 
                for (int i = allScreenRatio.ToList().IndexOf(PerformanceManager.Instance.ScreenRatio.ToString(), 0); i < allScreenRatio.Length; i++)
                {
                    useInfo = CanvasResizerInfo.Where(p => p.ScreenRatio.ToString() == allScreenRatio[i].ToString()).SingleOrDefault();

                    if (useInfo != null)
                    {
                        if (useInfo.EverythingBelow)
                        {
                            //TADA
                            Debug.Log("COOOOOL");


                        }
                        else
                        {
                            useInfo = null;
                        }

                        break;
                    }
                }
            }

            if(Canvas != null)
            {
                if (useInfo != null)
                {
                    CanvasScaler.referenceResolution = new Vector2(ReferenceSize.x, ReferenceSize.y / useInfo.Resize);
                    Canvas.ForceUpdateCanvases();
                }
            } else if(Transform != null)
            {
                if(UseXZForScale)
                {
                    Transform.localScale = new Vector3(ReferenceSize.x / useInfo.Resize, Transform.localScale.y, ReferenceSize.y / useInfo.Resize);
                } else
                {
                    Transform.localScale = new Vector3(ReferenceSize.x / useInfo.Resize, ReferenceSize.y / useInfo.Resize, Transform.localScale.z);
                }
                

            }

        }




        void UpdateListener()
        {

            if (PerformanceManager.Instance == null) return;

            PerformanceManager.Instance.ScreenSizeChanged.RemoveListener(ResizeScreen);
            PerformanceManager.Instance.ScreenSizeChanged.AddListener(ResizeScreen);

        }


        private void DetectElement()
        {

            if ((CanvasScaler == null && Canvas == null && Transform == null) || !isDetected)
            {
                CanvasScaler = this.GetComponent<CanvasScaler>();
                Canvas = this.GetComponent<Canvas>();

                //if not canvas, we resize transform
                if(Canvas == null)
                {
                    Transform = this.GetComponent<Transform>();
                }

                
                isDetected = CanvasScaler != null || Canvas != null || Transform != null;

            }
        }



        private void OnDestroy()
        {
            if (PerformanceManager.Instance == null) return;
            PerformanceManager.Instance.ScreenSizeChanged.RemoveListener(ResizeScreen);

        }

        // Update is called once per frame
        void Update()
        {
          
            DetectElement();

            if (!Application.isPlaying)
                UpdateListener();


        }

    }
}
