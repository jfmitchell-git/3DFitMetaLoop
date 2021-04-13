#if !BACKOFFICE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;
using System;

namespace MetaLoop.Common.PlatformCommon.Unity.ProgressBar
{

    public enum DGProgressFillType
    {
        Left = 0,
        Middle = 1,
        Right = 2
    }

    [ExecuteInEditMode]
    public class DGProgressBar : MonoBehaviour
    {
        private bool isCreated = false;

       

        [HideInInspector]
        public CanvasGroup ProgressCanvasGroup;

        public DGProgressBarStep ProgressBarImagePrefabs;
        private DGProgressBarStep currentProgressBarImagePrefabs;
        public GameObject ProgressBarContainer;


        public bool UpdateProgressBar = true;
        public bool Digital;
        public bool DigitalOnlyInStep;
        public bool DigitalRect2D;
        public RectTransform DigitalRect2DMask;
        public int NumberOfDigitalStep = 10;

        public bool PixelPerfect = true;

        public bool FilledImageType;

        public TextMeshProUGUI PercentText;

        public float PaddingWidth = 2;
        public float PaddingHeight = 2;
        public float Spacing = 2f;

        //should do this
        public float MinValue = 0;
        public float MaxValue = 100;

        public bool Inverse;
        public DGProgressFillType FillType;

        [Range(0.0f, 100.0f)]
        public float CurrentValue = 100f;



        [HideInInspector]
        public List<DGProgressBarStep> AllProgressBarImage;

        [HideInInspector]
        public Color OverwriteColor = Color.clear;

        
        public Gradient OverwriteGradient = null;
        public bool OverwriteGradientBool = false;

        public Image ProgressContour;

        public List<Image> ImageToColorWithProgress;

        private Vector2 sizeOfEachStep;
        private int numberOfStep = 1;

        public float RoundNumberBy = 1f;
        public float TextMaxValue = 100f;
        public bool ReverseNumber = false;

        [Header("Write OnOff or {0}%")]
        public string TextString = "{0}%";

        [HideInInspector]
        public RectTransform ProgressBarRectTransform;

        [HideInInspector]
        public Rect progressBarSize;

        [HideInInspector]
        public float ProgressBarCurrentWidth;


        private Color newColor;
        private Gradient useGradient;

        public Slider AffiliatedSlider;
        

        public void OnValueChangeFromSlider()
        {
            SetValue(AffiliatedSlider.value/ AffiliatedSlider.maxValue* 100f,0f);

            //Debug.Log(AffiliatedSlider.value);
        }

        public Color CurrentColor(string elementName)
        {

           Image image =  AllProgressBarImage[0].Images.ToList().Where(p => p.Image.name == elementName).SingleOrDefault().Image;

            if (image)
                return image.color;
            else
                return Color.white;
        }

        void Awake()
        {
           
          if(this.GetComponent<CanvasGroup>() == null)
            {
                ProgressCanvasGroup = this.gameObject.AddComponent<CanvasGroup>();
            } else
            {
                ProgressCanvasGroup = this.GetComponent<CanvasGroup>();
            }

            if (AffiliatedSlider != null)
                CurrentValue = AffiliatedSlider.value;

            CreateProgressBar();

        }

        // Use this for initialization
        void Start()
        {

            //we construct it (maybe should wait like always ?)
            //CreateProgressBar();

            
           
        }

        public void SetNewGradient(Gradient gradient = null)
        {
            OverwriteGradient = gradient;


            OverwriteGradientBool = gradient == null ? false : true;

            //update it
            SetValue(CurrentValue);
        }


        public void SetNewColor(Color color)
        {
            OverwriteColor = color;

            if (ProgressContour)
                ProgressContour.color = OverwriteColor;

            SetValue(CurrentValue,0.1f,true);
        }

        public void SetValue(float value,float speed = 0.2f,bool force = false)
        {

            //i suck
            //TextMaxValue = MaxValue;

            var startValue = CurrentValue;

            //why isnt that there ?
            if (CurrentValue == value && !force) return;

            CurrentValue = value;

            if (!isCreated) return;

            float useValue = CurrentValue;
            if (Inverse) useValue = 100 - CurrentValue;
   
            if(PercentText)
            {
                float numToUse = Mathf.Floor(value * RoundNumberBy) / RoundNumberBy;

                //set on max
                numToUse = (numToUse / 100) * TextMaxValue;

                if (ReverseNumber)
                    numToUse = TextMaxValue - numToUse;

                /*
                if (value < TextMaxValue && numToUse == Mathf.Floor(numToUse) && RoundNumberBy != 1)
                {
                    PercentText.text = String.Format("{0:0.0}", numToUse);
                    
                } else
                {
                    PercentText.text = (numToUse).ToString();
                }*/

                if (speed == 0f)
                {
                    if(TextString == "OnOff")
                    {
                        PercentText.text = numToUse == 0f ? ResourceManager.GetValue("Off") : ResourceManager.GetValue("On");
                    } else
                    {
                        PercentText.text = TextString.Replace("{0}", numToUse.ToString("F" + (RoundNumberBy.ToString().Length - 1).ToString())).ToString();
                    }
                   
                } else
                {
                    if (TextString == "OnOff")
                    {
                        PercentText.text = numToUse == 0f ? ResourceManager.GetValue("Off") : ResourceManager.GetValue("On");
                    }
                    else
                    {
                        Utils.Utils.AnimateNumberInText(PercentText, startValue, numToUse, speed, Convert.ToInt32(RoundNumberBy.ToString().Length - 1), TextString);
                    }
                }

            }

            if (AllProgressBarImage == null) return;

            float maxProgressBarSize = numberOfStep * sizeOfEachStep.x;
            float currentSize = ((useValue / 100* MaxValue) / 100) * maxProgressBarSize;

            //current size is BAD!


            Vector2 startPos = new Vector2(PaddingWidth, -PaddingHeight);

            if (FillType == DGProgressFillType.Middle)
            {
                currentSize -= maxProgressBarSize / 2;
                startPos.x += maxProgressBarSize / 2; 

                if(currentSize < 0)
                {

                } else
                {

                }
            }

            if(FillType == DGProgressFillType.Right)
            {

                currentSize -= maxProgressBarSize;
                startPos.x += maxProgressBarSize;

            }

            //Numb of step
            float weAreAtStep = Mathf.Floor((useValue / 100*MaxValue) / 100 * numberOfStep);
            
            for (int i = 0; i < AllProgressBarImage.Count; i++)
            {

                DGProgressBarStep progressBarStep = AllProgressBarImage[i];
               
                if(progressBarStep.StepImageRectTransform == null)
                {
                    progressBarStep.StepImageRectTransform = progressBarStep.GetComponent<RectTransform>();
                }

                float currentStepSize = currentSize - sizeOfEachStep.x * i;
                    if (currentStepSize > sizeOfEachStep.x) currentStepSize = sizeOfEachStep.x;

                //if (!FilledImageType)
                //{
                    if (currentStepSize < 0 && !Digital)
                    {
                        currentStepSize = Math.Abs(currentSize);
                        if (progressBarStep.StepImageRectTransform.localScale.x > 0)
                            progressBarStep.StepImageRectTransform.localScale = new Vector3(-(progressBarStep.StepImageRectTransform.localScale.x), progressBarStep.StepImageRectTransform.localScale.y, progressBarStep.StepImageRectTransform.localScale.z);
                    }
                    else if (currentStepSize < 0)
                    {
                        currentStepSize = 0;
                    }
                    else
                    {
                        if (progressBarStep.StepImageRectTransform.localScale.x < 0)
                            progressBarStep.StepImageRectTransform.localScale = new Vector3(-(progressBarStep.StepImageRectTransform.localScale.x), progressBarStep.StepImageRectTransform.localScale.y, progressBarStep.StepImageRectTransform.localScale.z);
                    }
               // }

                if (DigitalOnlyInStep)
                    {
                        currentStepSize = currentStepSize < sizeOfEachStep.x && currentStepSize != 0 ? sizeOfEachStep.x : currentStepSize;
                    }

                    float startSize = maxProgressBarSize - sizeOfEachStep.x * i;



                //progressBarStep.StepImageRectTransform.sizeDelta = new Vector2(sizeOfEachStep.x, sizeOfEachStep.y);

                //position correctly
                progressBarStep.StepImageRectTransform.localPosition = new Vector3(startPos.x + (sizeOfEachStep.x * i) + (Spacing * i), startPos.y, 0);


                

                //set correct color
                foreach (DGProgressBarStepImage stepColor in progressBarStep.Images)
                {
                
                   
                    if (stepColor.ColorizeImage)
                    {
                        newColor = new Color();

                        useGradient = stepColor.Color;

                        if(OverwriteGradientBool)
                        {
                            useGradient = OverwriteGradient;
                        }

                        if (DigitalOnlyInStep)
                        {
                            
                            newColor = useGradient.Evaluate(weAreAtStep/numberOfStep);
                        }
                        else {
                            newColor = useGradient.Evaluate(useValue / MaxValue);
                        }

                        
                        if (OverwriteColor != Color.clear)
                            newColor = OverwriteColor;

                        newColor.a = stepColor.Alpha;
                        stepColor.Image.color = newColor;

                        ImageToColorWithProgress.ForEach(p => p.color = new Color(newColor.r,newColor.g,newColor.b,p.color.a));

                    }

                    if (Application.isPlaying)
                    {

                        if (FilledImageType)
                        {
                           // stepColor.Image.DOKill();
                           if(speed == 0f)
                            {
                                stepColor.Image.fillAmount = stepColor.ResizeImage ? (useValue / 100) / (FillType == DGProgressFillType.Middle ? 2 : 1) : sizeOfEachStep.x;
                            } else
                            {
                                stepColor.Image.DOFillAmount(stepColor.ResizeImage ? (useValue / 100) / (FillType == DGProgressFillType.Middle ? 2 : 1) : sizeOfEachStep.x, speed).SetEase(Ease.Linear).SetDelay(stepColor.IsFighting ? 0.75f : 0f).SetUpdate(true);
                            }
                            
                        }
                        else
                        {


                            if (DigitalRect2D)
                            {

                                if (stepColor.RectTransformMask != null)
                                {
                                    //set correct size
                                    stepColor.RectTransformMask.sizeDelta = new Vector2(stepColor.ResizeImage ? currentStepSize : sizeOfEachStep.x, sizeOfEachStep.y);

                                    stepColor.RectTransformMask.position = DigitalRect2DMask.position;
                                    stepColor.RectTransformMask.position = progressBarStep.StepImageRectTransform.position;

                                }
                                //set correct size
                                stepColor.RectTransform.sizeDelta = new Vector2(DigitalRect2DMask.sizeDelta.x, DigitalRect2DMask.sizeDelta.y);
                                stepColor.RectTransform.position = DigitalRect2DMask.position;






                            }
                            else
                            {
                                //set correct size
                                stepColor.RectTransform.DOSizeDelta(new Vector2(stepColor.ResizeImage ? currentStepSize : sizeOfEachStep.x, sizeOfEachStep.y), speed).SetEase(Ease.Linear).SetUpdate(true).SetDelay(stepColor.IsFighting ? 0.75f : 0f);

                            }
                        }

                    } else
                    {
                        if (FilledImageType)
                        {

                            float fillValue = useValue;
                            if (FillType == DGProgressFillType.Middle)
                            {

                                if (fillValue >= 50)
                                {
                                    //100 = 100
                                    //50 = 0
                                    //100 - 50 * 2
                                    fillValue = (fillValue - 50) * 2;
                                }
                                else
                                {

                                    //50 = 0
                                    //25 = 50
                                    //0 = 100
                                    fillValue = 100 - (fillValue * 2);
                                }

                            }

                            stepColor.Image.fillAmount = stepColor.ResizeImage ? fillValue/100 : sizeOfEachStep.x;
                        }
                        else
                        {

                            if(DigitalRect2D)
                            {

                                if (stepColor.RectTransformMask != null)
                                {
                                    //set correct size
                                    stepColor.RectTransformMask.sizeDelta = new Vector2(stepColor.ResizeImage ? currentStepSize : sizeOfEachStep.x, sizeOfEachStep.y);

                                    stepColor.RectTransformMask.position = DigitalRect2DMask.position;
                                    stepColor.RectTransformMask.position = progressBarStep.StepImageRectTransform.position;

                                }
                                    //set correct size
                                    stepColor.RectTransform.sizeDelta = new Vector2(DigitalRect2DMask.sizeDelta.x, DigitalRect2DMask.sizeDelta.y);
                                    stepColor.RectTransform.position = DigitalRect2DMask.position;

                                   
                                
                              


                            } else
                            {
                                //set correct size
                                stepColor.RectTransform.sizeDelta = new Vector2(stepColor.ResizeImage ? currentStepSize : sizeOfEachStep.x, sizeOfEachStep.y);
                            }
                           
                        }
                    }



                }

                

            }

           

            ProgressBarCurrentWidth = currentSize;

        }



        public void CreateProgressBar()
        {

            //return if missing important part
            if (ProgressBarImagePrefabs == null) return;
            if (ProgressBarContainer == null) return;

            //num of progressBar (digital)
            numberOfStep = Digital ? NumberOfDigitalStep : 1;



            //calculate the size of each shit
            ProgressBarRectTransform = this.GetComponent<RectTransform>();

                
             progressBarSize = new Rect(ProgressBarRectTransform.rect);

            if (PixelPerfect)
            {

                progressBarSize.width = Mathf.Round(progressBarSize.width);
                progressBarSize.height = Mathf.Round(progressBarSize.height);

                int diffDown = 0;
                int diffTop = 0;

                var checkPixelPerfectWidth = (progressBarSize.width - PaddingWidth * 2 - ((numberOfStep - 1) * Spacing));
                var notRoundedSize = checkPixelPerfectWidth / numberOfStep;

                while (notRoundedSize != Mathf.Round(checkPixelPerfectWidth / numberOfStep))
                {
                    diffDown++;
                    progressBarSize.width--;
                    checkPixelPerfectWidth = (progressBarSize.width - PaddingWidth * 2 - ((numberOfStep - 1) * Spacing));
                    notRoundedSize = checkPixelPerfectWidth / numberOfStep;

                }

                //reset
                progressBarSize = new Rect(ProgressBarRectTransform.rect);
                progressBarSize.width = Mathf.Round(progressBarSize.width);
                progressBarSize.height = Mathf.Round(progressBarSize.height);

                //checkup
                while (notRoundedSize != Mathf.Round(checkPixelPerfectWidth / numberOfStep))
                {
                    diffTop++;
                    progressBarSize.width++;
                    checkPixelPerfectWidth = (progressBarSize.width - PaddingWidth * 2 - ((numberOfStep - 1) * Spacing));
                    notRoundedSize = checkPixelPerfectWidth / numberOfStep;

                }



                if(diffDown < diffTop)
                {

                    progressBarSize = new Rect(ProgressBarRectTransform.rect);
                    progressBarSize.width = Mathf.Round(progressBarSize.width);
                    progressBarSize.height = Mathf.Round(progressBarSize.height);

                    progressBarSize.width -= diffDown;

                }

            } else
            {

            }

                sizeOfEachStep = new Vector2((progressBarSize.width - PaddingWidth * 2 - ((numberOfStep - 1) * Spacing)) / numberOfStep, progressBarSize.height - PaddingHeight * 2);

            //fix size fo r pixel perfect
            if (PixelPerfect)
            {

                sizeOfEachStep = new Vector2(Mathf.Round(sizeOfEachStep.x),Mathf.Round(sizeOfEachStep.y));
            }


            if (AllProgressBarImage != null && AllProgressBarImage.Count > 0 && AllProgressBarImage[0] != null && AllProgressBarImage[0].GetType() == ProgressBarImagePrefabs.GetType() && AllProgressBarImage.Count == numberOfStep && currentProgressBarImagePrefabs == ProgressBarImagePrefabs)
            {

            }
            else
            {

 
                if (UpdateProgressBar || Application.isPlaying)
                {
                   // if(UpdateProgressBar)
                       // Debug.Log("WTF");
  
                    //remove every object in it fi different
                    while (ProgressBarContainer.transform.childCount > 0)
                    {
                        
                        DestroyImmediate(ProgressBarContainer.transform.GetChild(0).gameObject);
                    }
                    AllProgressBarImage = new List<DGProgressBarStep>();


                    //add all iamge
                    for (int i = 0; i < numberOfStep; i++)
                    {

                        DGProgressBarStep progressBar = UnityEngine.Object.Instantiate(ProgressBarImagePrefabs);

                        progressBar.transform.SetParent(ProgressBarContainer.transform, false);
                        
                        progressBar.gameObject.SetActive(true);
                        AllProgressBarImage.Add(progressBar);

                        if (DigitalRect2D)
                        {
                            progressBar.Images.ForEach(x => x.RectTransform.sizeDelta = new Vector2(progressBarSize.width, progressBarSize.height));
                        }
                        else
                        {
                            progressBar.Images.ForEach(x => x.RectTransform.sizeDelta = new Vector2(sizeOfEachStep.x, sizeOfEachStep.y));
                        }
                    }
                }

            }

            isCreated = true;

            //resize everything correctly
            SetValue(CurrentValue,0f,true);

            currentProgressBarImagePrefabs = ProgressBarImagePrefabs;

        }


        //private bool firstUpdate;

        // Update is called once per frame
        void Update()
        {

            if (ProgressBarImagePrefabs != currentProgressBarImagePrefabs)
                CreateProgressBar();

           // firstUpdate = true;
          
            //resize if needed
            if (Application.isPlaying && ((!PixelPerfect && ProgressBarRectTransform.rect.width != progressBarSize.width) || (PixelPerfect && Mathf.Round(ProgressBarRectTransform.rect.width) != progressBarSize.width)))
            {
                CreateProgressBar();
            }

            //all code here are for construction in editor
            if (Application.isPlaying) return;


            //we construct it
            CreateProgressBar();

            





        }
    }
}
#endif