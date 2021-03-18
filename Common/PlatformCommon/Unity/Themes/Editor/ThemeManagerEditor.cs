#if !BACKOFFICE
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.Themes
{
    [CustomEditor(typeof(ThemeManager))]
    public class ThemeManagerEditor : Editor
    {
        ThemeManager themeManager;
        string filePath = "Assets/";
        string fileName = "TestMyEnum";

        int _choiceIndex = 0;

         

        // private List<ThemeInfo> tempThemes;

        private void OnEnable()
        {
            themeManager = (ThemeManager)target;
        }

        public override void OnInspectorGUI()
        {
           // base.OnInspectorGUI();
            serializedObject.Update();


            EditorGUI.BeginChangeCheck();

            List<string> allThemesString = new List<string>();
            themeManager.AllThemes.ForEach(p => allThemesString.Add(p.Name));
            int currentThemeIndex = themeManager.AllThemes.IndexOf(themeManager.CurrentTheme, 0);
            if (currentThemeIndex == -1)
                currentThemeIndex = 0;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Theme: ");
            currentThemeIndex = EditorGUILayout.Popup(themeManager.CurrentThemeIndex, allThemesString.ToArray());
            GUILayout.EndHorizontal();

            //always one selected
            if (currentThemeIndex == -1)
                currentThemeIndex = 0;

            if(themeManager.AllThemes.Count > 0)
                themeManager.CurrentTheme = themeManager.AllThemes[currentThemeIndex];

            themeManager.CurrentThemeIndex = currentThemeIndex;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //all themetype first?
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AllColors"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ThemeType"));
            //themeManager.ThemeType = EditorGUILayout("Image Path", myTheme.ImagePath);
            
            

           // if (tempThemes == null)
           //  tempThemes = new List<ThemeInfo>();
           //tempThemes.Clear();

            if (GUILayout.Button("Add New Theme"))
            {
                themeManager.AllThemes.Add(new ThemeInfo());
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            for (int i = 0; i < themeManager.AllThemes.Count; i++)
            {
                ThemeInfo myTheme = themeManager.AllThemes[i];
                ThemeInfo tmpTheme = myTheme;

                EditorGUILayout.LabelField("Theme: " + myTheme.Name);

                // myTheme.ThemeType = EditorGUILayout.Popup(_choiceIndex, themeManager.ThemeType.ToArray());
                tmpTheme.Name = EditorGUILayout.TextField("Name", myTheme.Name);


                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Theme Type: ");
                tmpTheme.ThemeType = EditorGUILayout.Popup(myTheme.ThemeType, themeManager.ThemeType.ToArray());
                GUILayout.EndHorizontal();

                tmpTheme.ImagePath = EditorGUILayout.TextField("Image Path", myTheme.ImagePath);


                if (tmpTheme.AllColors == null)
                    tmpTheme.AllColors = new List<ThemeColorInfo>();


                while (tmpTheme.AllColors.Count < themeManager.AllColors.Count)
                    tmpTheme.AllColors.Add(new ThemeColorInfo());

                //loop all colors
                for (int j = 0; j < themeManager.AllColors.Count; j++)
                {
                   

                    tmpTheme.AllColors[j].Name = themeManager.AllColors[j];
                    tmpTheme.AllColors[j].Color = EditorGUILayout.ColorField(themeManager.AllColors[j], myTheme.AllColors[j].Color);
                }


               // tmpTheme.LightColor = EditorGUILayout.ColorField("Light Color", myTheme.LightColor);
               // tmpTheme.DarkColor = EditorGUILayout.ColorField("Dark Color", myTheme.DarkColor);

                //EditorGUILayout.PropertyField(serializedObject.FindProperty("PuzzlePrefabs"));

                themeManager.AllThemes[i] = tmpTheme;

               // tempThemes.Add(tmpTheme);

                if (GUILayout.Button("Delete"))
                {
                    themeManager.AllThemes.RemoveAt(i);
                    i--;
                }



                    EditorGUILayout.Space();
                EditorGUILayout.Space();
            }



            //filePath = EditorGUILayout.TextField("Path", filePath);
            // fileName = EditorGUILayout.TextField("Name", fileName);

            // _choiceIndex = EditorGUILayout.Popup(_choiceIndex, themeManager.ThemeType.ToArray());

            //no need for save
            /*  EditorGUILayout.Space();
            if (GUILayout.Button("Save"))
             {

                 //EdiorMethods.WriteToEnum(filePath, fileName, myScrip.days);
             }*/

            bool updateTheme = false;
            if (EditorGUI.EndChangeCheck())
            {
                updateTheme = true;
               // Debug.Log("SOMETHING CHANGED IN THEME! UPDATE ALL THEME ELEMENT");
            }


            serializedObject.ApplyModifiedProperties();

            if(updateTheme)
            {
                themeManager.OnThemeUpdate.Invoke();
            }


        }
    }
}
#endif