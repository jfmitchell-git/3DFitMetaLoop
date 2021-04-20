using DG.Tweening;
using MetaLoop.Common.PlatformCommon.Unity.Sounds;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace MetaLoop.Common.PlatformCommon.Unity.Buttons
{
    public class GenericButton : MonoBehaviour
    {

        [HideInInspector]
        public Button Button;
        [HideInInspector]
        public Animator Animator;

        

        public string SoundClickName;
        public string AnimationOnClick = "Pressed";
        public float AnimationOnClickDelay = 0.1f;
        public UnityEvent OnClick;

        public TextMeshProUGUI ButtonText;

        private void Awake()
        {
            
            Button = this.GetComponent<Button>();
            Animator = GetComponent<Animator>();

            if (Button == null)
                this.gameObject.AddComponent<Button>();

            if (Button != null)
                Button.onClick.AddListener(ButtonClicked);

        }

        private void ButtonClicked()
        {


            Button.enabled = false;


            if (SoundClickName != null && SoundManager.Instance != null)
                SoundManager.Instance.PlaySoundByName(SoundClickName);

            if (Animator != null)
            {
                Animator.Play("Pressed");

               
                DOVirtual.DelayedCall(AnimationOnClickDelay, ()=> { OnClick.Invoke(); Button.enabled = true; });

            } else
            {
                OnClick.Invoke();
                DOVirtual.DelayedCall(AnimationOnClickDelay, () => { Button.enabled = true; });
            }

        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}