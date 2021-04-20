#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace MetaLoop.Common.PlatformCommon.Unity.Optimization
{

    public enum ScreenRatio
    {
        Ratio_9_20,
        Ratio_9_18,
        Ratio_9_16,
        Ratio_10_16,
        Ratio_3_4,

        Ratio_4_3,
        Ratio_16_10,
        Ratio_16_9,
        Ratio_18_9,
        Ratio_20_9
    }


    public enum MobileType
    {
        Undefined = 0,
        Phone = 1,
        Tablet = 2

    }


    [ExecuteInEditMode]
    public class PerformanceManager : MonoBehaviour
    {

        public static PerformanceManager Instance;

        [HideInInspector]
        public float ScreenHeightInInch = -1f;
        [HideInInspector]
        public ScreenRatio ScreenRatio;
        [HideInInspector]
        public float ScreenRatioPercent;
        [HideInInspector]
        public MobileType MobileType;
        [HideInInspector]
        public UnityEvent ScreenSizeChanged;

        private Vector2 currentScreenSize;

        public Fps FpsCounter;

        public static bool LowRam = false;

        [HideInInspector]
        public PerformanceQualityInfo QualityInfo;

        public UnityEvent OnQualityChanged;

        List<string> LowGraphicCard;

        public static Vector2 nativeResolution;



        //doing my OWN fixedDPI code lol
        private int fixedDPIHigh = 300;
        private int fixedDPIMedium = 250;
        private int fixedDPILow = 144;

        [HideInInspector]
        public float CurrentScreenRatioPercent = 1f;

        public bool ShowFps;

        void Start()
        {
           
            MobileType = GetDeviceType();
        }

        void Awake()
        {

            MobileType = GetDeviceType();



            if (Instance == null)
            {

                Instance = this;

                


                if (!Application.isPlaying) return;

               

                if (SystemInfo.systemMemorySize < 3500)
                {
                    LowRam = true;
                }


                if (nativeResolution == Vector2.zero)
                    nativeResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

                LowGraphicCard = new List<string>();
                LowGraphicCard.Add("Mali-T628".ToLower());
                LowGraphicCard.Add("Mali-T830".ToLower());

                QualityInfo = new PerformanceQualityInfo();
                QualityInfo.QualityLevel = 2;
                //2 = high
                //1 = medium
                //0 = low

                //fps counter always invisible if in production
                FpsCounter.enabled = ShowFps;

#if !UNITY_EDITOR
                FpsCounter.enabled = false;
#endif

                 QualitySettings.vSyncCount = 0;  // VSync must be disabled
                Application.targetFrameRate = 60;
                //QualityInfo.QualityLevel = QualitySettings.GetQualityLevel();


                QualitySettings.asyncUploadTimeSlice = 8;
                QualitySettings.asyncUploadBufferSize = 16;
                QualitySettings.asyncUploadPersistentBuffer = true;

                //we just evaluate first time moron i am
                //if (GameData.Current.MetaDataState.SettingState.Graphic == -1)
                //EvaluateQuality();

                //here for now
                //EvaluateQuality();

            }
            else
            {
                Destroy(this);

            }




        }

      

        public void EvaluateQuality()
        {

            //let be sure when we evaluate we are at 2
            QualityInfo.QualityLevel = 2;

            int graphicMemory = SystemInfo.graphicsMemorySize;
            int systemMemory = SystemInfo.systemMemorySize;
            Vector2 screenSize = new Vector2(nativeResolution.x, nativeResolution.y);

            //Debug.Log("GraphicMemory = " + graphicMemory);
            // Debug.Log("SystemMemory = " + systemMemory);
            // Debug.Log("ScreenSize = " + screenSize);

            //if we have under 4gig ram, we need to reduce quality a bit
            if (systemMemory < 3700 && Application.platform == RuntimePlatform.Android)
            {
                QualityInfo.QualityLevel--;
            }

            //under 2.5 gig of ram, mean probably 2.. mean shitty device, mean LOW
            if (systemMemory < 2500 /*&& Application.platform == RuntimePlatform.IPhonePlayer*/)
            {
                QualityInfo.QualityLevel--;
            }

            //nothing will ever hit that i think
            if (systemMemory < 1500)
            {
                //old iphoen should hit that and get quality 1
                QualityInfo.QualityLevel--;
            }




            //screen size too on android is very evidant (that will be only on android)
            if (Application.platform == RuntimePlatform.Android)
            {
                //if the screen is weak
                if (screenSize.x * screenSize.y < 1050000)
                {
                    QualityInfo.QualityLevel--;
                }


                //if the graphic lvl is 3.1 (not vulkan)
                if (SystemInfo.graphicsDeviceVersion.ToLower().Contains("opengl es 3.1") || SystemInfo.graphicsDeviceVersion.ToLower().Contains("opengl es 3.0"))
                {
                    QualityInfo.QualityLevel--;
                }


            }


            if (QualityInfo.QualityLevel < 0)
                QualityInfo.QualityLevel = 0;


            //ovewrite to test
            //QualityInfo.QualityLevel = 0;

            ChangeQuality(QualityInfo.QualityLevel);


        }

        public void SetFpsOnQuality()
        {

            switch (QualityInfo.QualityLevel)
            {
                case 0:
                    ChangeFps(30);
                    break;

                case 1:
                    ChangeFps(40);
                    break;

                case 2:
                    ChangeFps(50);
                    break;
            }



        }

        public void ChangeFps(int fps)
        {
            QualitySettings.vSyncCount = 0;  // VSync must be disabled
            Application.targetFrameRate = fps;
            // Utils.DelayedCallWaitFrame(1f, () => Application.targetFrameRate = fps, this);
        }




        public void ChangeQuality(int quality)
        {

            //HACK
            //quality = 0;

            int graphicMemory = SystemInfo.graphicsMemorySize;
            int systemMemory = SystemInfo.systemMemorySize;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            //save
            QualityInfo.QualityLevel = quality;
            QualitySettings.SetQualityLevel(quality);

            //some change here
            bool containsLowGraphicCard = false;
            foreach (string lowGraphicCardName in LowGraphicCard)
            {
                if (SystemInfo.graphicsDeviceName.ToLower().IndexOf(lowGraphicCardName, 0) != -1)
                {
                    containsLowGraphicCard = true;
                    break;
                }
            }


          

         

            //if we can't detect DPI
            if(Screen.dpi == 0f)
            {

                if (QualityInfo.QualityLevel == 1 || QualityInfo.QualityLevel == 0)
                {

                    if (Screen.dpi <= 325)
                    {
                        Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * 0.75f), Mathf.RoundToInt(nativeResolution.y * 0.75f), true, Screen.currentResolution.refreshRate);
                    }
                    else
                    {
                        Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * 0.50f), Mathf.RoundToInt(nativeResolution.y * 0.50f), true, Screen.currentResolution.refreshRate);
                    }

                }
                else
                {
                    Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * 0.75f), Mathf.RoundToInt(nativeResolution.y * 0.75f), true, Screen.currentResolution.refreshRate);
                }


            } else
            {

                switch(QualityInfo.QualityLevel)
                {
                    case 0:

                        CurrentScreenRatioPercent = fixedDPILow / Screen.dpi;

                        break;

                    case 1:

                        CurrentScreenRatioPercent = fixedDPIMedium / Screen.dpi;

                        break;

                    case 2:

                        CurrentScreenRatioPercent = fixedDPIHigh / Screen.dpi;

                        break;
                }

                //min max value
                if (CurrentScreenRatioPercent > 1f) CurrentScreenRatioPercent = 1f;
                if (CurrentScreenRatioPercent < .5f) CurrentScreenRatioPercent = .5f;

                Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * CurrentScreenRatioPercent), Mathf.RoundToInt(nativeResolution.y * CurrentScreenRatioPercent), true, Screen.currentResolution.refreshRate);
                Debug.Log("screen resized to " + Mathf.RoundToInt(nativeResolution.y * CurrentScreenRatioPercent) + " " + CurrentScreenRatioPercent);


            }

           

            /*

            if (QualityInfo.QualityLevel == 0 || (Screen.dpi >= 300 && Application.platform == RuntimePlatform.Android && systemMemory < 3500 && QualityInfo.QualityLevel != 2) || (Application.platform == RuntimePlatform.Android && containsLowGraphicCard && QualityInfo.QualityLevel != 2))
            {

                if (Screen.dpi <= 325)
                {
                    Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * 0.75f), Mathf.RoundToInt(nativeResolution.y * 0.75f), true, Screen.currentResolution.refreshRate);
                }
                else
                {
                    Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * 0.50f), Mathf.RoundToInt(nativeResolution.y * 0.50f), true, Screen.currentResolution.refreshRate);
                }

            }
            else
            {

                if(QualityInfo.QualityLevel == 1)
                {

                    if (Screen.dpi <= 325)
                    {
                        Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * 0.75f), Mathf.RoundToInt(nativeResolution.y * 0.75f), true, Screen.currentResolution.refreshRate);
                    }
                    else
                    {
                        Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * 0.50f), Mathf.RoundToInt(nativeResolution.y * 0.50f), true, Screen.currentResolution.refreshRate);
                    }

                } else
                {
                    Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * 0.75f), Mathf.RoundToInt(nativeResolution.y * 0.75f), true, Screen.currentResolution.refreshRate);
                }

               
               // Screen.SetResolution(Mathf.RoundToInt(nativeResolution.x * 0.75f), Mathf.RoundToInt(nativeResolution.y * 0.75f), true, Screen.currentResolution.refreshRate);

            }
            */



            //invoke so all can change for new settings
            OnQualityChanged.Invoke();

        }




        void Update()
        {

            if (currentScreenSize.x != Screen.width || currentScreenSize.y != Screen.height)
            {
                RefreshScreenSetting();

            }

          
            if(!Application.isPlaying)
            {
                Instance = this;
            }


        }





        private static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        public static MobileType GetDeviceType()
        {
#if UNITY_IOS
    bool deviceIsIpad = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
            if (deviceIsIpad)
            {
                return ENUM_Device_Type.Tablet;
            }
            bool deviceIsIphone = UnityEngine.iOS.Device.generation.ToString().Contains("iPhone");
            if (deviceIsIphone)
            {
                return ENUM_Device_Type.Phone;
            }
#elif UNITY_ANDROID

            if (Screen.width == 0f || Screen.height == 0f) return MobileType.Phone;

            float aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
            bool isTablet = (DeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f);

            Debug.Log("AspectRatio = " + aspectRatio + " Diagonal =" + DeviceDiagonalSizeInInches() + " isTablet=" + isTablet);

            if (isTablet)
            {
                return MobileType.Tablet;
            }
            else
            {
                return MobileType.Phone;
            }
#endif
        }
    


    private void RefreshScreenSetting()
        {

            currentScreenSize = new Vector2(Screen.width, Screen.height);
            ScreenHeightInInch = Screen.height / Screen.dpi;


            float aspectRatio = ((float)Screen.width) / ((float)Screen.height);


            ScreenRatio closestRatio = ScreenRatio.Ratio_9_20;
            float closestDiff = 100f;


            string[] allScreenRatio = System.Enum.GetNames(typeof(ScreenRatio));
            for (int i = 0; i < allScreenRatio.Length; i++)
            {
                string[] getRatio = allScreenRatio[i].Split('_');

                float ratioX = float.Parse(getRatio[1]);
                float ratioY = float.Parse(getRatio[2]);

                float currentDiff = aspectRatio - (ratioX/ratioY);


                if (currentDiff < -0.01f) continue;

                if(currentDiff < closestDiff)
                {
                    closestDiff = currentDiff;
                    closestRatio = (ScreenRatio)Enum.Parse(typeof(ScreenRatio), allScreenRatio[i]);
                    ScreenRatioPercent = (ratioX / ratioY);
                }
            }


            ScreenRatio = closestRatio;

            Debug.Log("ScreenRatio = " + ScreenRatio);

            ScreenSizeChanged.Invoke();

        }


    }
}
#endif