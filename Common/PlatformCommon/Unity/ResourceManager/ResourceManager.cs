#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity
{

    public class ReplaceStringItem
    {
        public string Find { get; set; }
        public string Replace { get; set; }

        public ReplaceStringItem(string find, string replace)
        {
            this.Find = find;
            this.Replace = replace;
        }
    }

    public class ResourceManager
    {
        private static Dictionary<string, string> content;
        private static string currentCulture;

        private static List<KeyValuePair<string, string>> WordsOverrides = new List<KeyValuePair<string, string>>();
        public static List<ReplaceStringItem> ReplaceStringItems = new List<ReplaceStringItem>();

        public static string CurrentCulture { get; internal set; }


        public static void LoadCulture(XmlDocument file, string culture)
        {
            currentCulture = culture;

            if (content == null)
            {
                content = new Dictionary<string, string>();
            }
            else
            {
                content.Clear();
            }

            foreach (XmlNode node in file.DocumentElement.ChildNodes)
            {


                /*if(node == null || node.Attributes == null ||  node.Attributes["name"] == null)
                {
                    Debug.Log("BAD XML EROR");
                }*/


                if (!content.ContainsKey(node.Attributes["name"].InnerText))
                {
                    content.Add(node.Attributes["name"].InnerText, node.InnerText);
                }
                else
                {
                    content[node.Attributes["name"].InnerText] = node.InnerText;
                }

            }


            if (content.ContainsKey("Words.Override") && !string.IsNullOrEmpty(content["Words.Override"]))
            {
                List<string> allOverides = content["Words.Override"].Split('|').ToList();
                foreach (var item in allOverides)
                {
                    KeyValuePair<string, string> newItem = new KeyValuePair<string, string>(item.Split(':')[0], item.Split(':')[1]);
                    WordsOverrides.Add(newItem);
                }
            }
        }



        public static string GetValue(string key, params object[] formats)
        {
            return string.Format(GetValue(key), formats);
        }


        public static string GetValue(string key)
        {
            if (content != null)
            {
                if (content.ContainsKey(key))
                {
                    string result = content[key];

                    foreach (var item in WordsOverrides)
                    {
                        result = result.Replace(item.Key, item.Value);
                    }

                    return result;
                }
                else
                {
                    return string.Empty;
                    return string.Format("[!{0}]", key);
                }

            }
            else
            {
                return string.Empty;
            }
        }

        public static void LoadCulture(string userProfileCulture)
        {
            XmlDocument resourcesFile;
            string cultureCode = string.Empty;

            if (string.IsNullOrEmpty(userProfileCulture))
            {
                //culture has not been set in settings yet. Will try to take from system language.
                resourcesFile = ResourceFile.GetResourceFile(ResourceFile.GetPathFromLanguage(Application.systemLanguage));
                CurrentCulture = ResourceFile.GetCultureCode(Application.systemLanguage);
            }
            else
            {
                //culture has been set in settings, load file from culture code.
                resourcesFile = ResourceFile.GetResourceFile(ResourceFile.GetPathFromLanguage(userProfileCulture));
                CurrentCulture = userProfileCulture;
            }

            if (resourcesFile != null)
            {
                LoadCulture(resourcesFile, cultureCode);
            }
            else
            {
                //Could not load settings culture or Application.systemLanguage, revert back to english.
                resourcesFile = ResourceFile.GetResourceFile(ResourceFile.GetPathFromLanguage(SystemLanguage.English));
                LoadCulture(resourcesFile, ResourceFile.GetCultureCode(SystemLanguage.English));
            }
        }

    }
}

#endif