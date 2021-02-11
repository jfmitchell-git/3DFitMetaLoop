#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;

namespace MetaLoop.Common.PlatformCommon.Unity
{
    
    public class ResourceUiText : MonoBehaviour
    {
        public string ResourceKey;
        public bool ToUpperCase;

        void Start()
        {
            UpdateText(ResourceKey);
        }

        public void UpdateText(string resourceKey)
        {
            ResourceKey = resourceKey;

            TextMeshProUGUI textBox = this.gameObject.GetComponent<TextMeshProUGUI>();
            if (textBox != null)
            {
                textBox.text = ToUpperCase ? ResourceManager.GetValue(ResourceKey).ToUpper() : ResourceManager.GetValue(ResourceKey);
            }

        }
    }
}

#endif