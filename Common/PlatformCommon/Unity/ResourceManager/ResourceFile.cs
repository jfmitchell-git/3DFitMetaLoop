#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity
{
    class ResourceFile : MonoBehaviour
    {

        private static Dictionary<SystemLanguage, string> values;

        private static void InitCulturesDict()
        {
            if (values == null)
            {
                values = new Dictionary<SystemLanguage, string>();
                values.Add(SystemLanguage.English, "en-US");
                values.Add(SystemLanguage.French, "fr-FR");
                values.Add(SystemLanguage.Spanish, "es-ES");
                values.Add(SystemLanguage.Russian, "ru-RU");
                values.Add(SystemLanguage.Portuguese, "pt-BR");
                values.Add(SystemLanguage.German, "de-DE");
                values.Add(SystemLanguage.Italian, "it-IT");
                values.Add(SystemLanguage.ChineseSimplified, "zh-CHS");
                values.Add(SystemLanguage.Chinese, "zh-CHT");
            }
        }


        public static XmlDocument GetResourceFile(string filename)
        {
            TextAsset xmlData = new TextAsset();
            xmlData = (TextAsset)Resources.Load(filename, typeof(TextAsset));
            if (xmlData != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData.text);
                return xmlDoc;
            }
            else
            {
                return null;
            }

        }

        public static string GetPathFromLanguage(SystemLanguage language, string resourcePath = "Lang/{0}")
        {
            return string.Format(resourcePath, GetCultureCode(language));
        }

        public static string GetPathFromLanguage(string cultureCode, string resourcePath = "Lang/{0}")
        {
            return string.Format(resourcePath, cultureCode);
        }

        public static string GetCultureCode(SystemLanguage language, bool shortFormat = true)
        {
            InitCulturesDict();

            if (values.ContainsKey(language))
            {

                return shortFormat ? values[language].Substring(0, 2) : values[language];
            }
            else
            {
                return "XX";
            }
        }

        public static SystemLanguage GetSystemLanguageFromShortCultureCode(string culture)
        {
            InitCulturesDict();

            foreach (KeyValuePair<SystemLanguage, string> entry in values)
            {
                if (entry.Value.Substring(0, 2).ToLower() == culture.ToLower()) {
                    return entry.Key;
                }
            }

            return SystemLanguage.English;
        }

    }
}
#endif