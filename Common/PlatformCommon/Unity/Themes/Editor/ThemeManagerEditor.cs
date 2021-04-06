#if !BACKOFFICE
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

namespace MetaLoop.Common.PlatformCommon.Unity.Themes
{
    [CustomEditor(typeof(ThemeManager))]
    public class ThemeManagerEditor : Editor
    {
        ThemeManager themeManager;
        string filePath = "Assets/";
        string fileName = "TestMyEnum";

        int _choiceIndex = 0;

        bool saveTheme;

        // private List<ThemeInfo> tempThemes;

        private void OnEnable()
        {
            themeManager = (ThemeManager)target;

            EditorApplication.update += OnEditorUpdate;
        }

        protected virtual void OnDisable()
        {
           
               EditorApplication.update -= OnEditorUpdate;
           
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
            bool deleteTheme = false;
            int deleteThemeIndex = 0;

            bool addNewTheme = false;
            if (GUILayout.Button("Add New Theme"))
            {
                addNewTheme = true;
               
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

               

               // tempThemes.Add(tmpTheme);

                if (GUILayout.Button("Delete"))
                {
                    deleteTheme = true;
                    deleteThemeIndex = i;
                   
                }



                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Theme Color"))
                {
                    themeManager.CopyTheme = myTheme;
                }
                if (GUILayout.Button("Paste Theme Color"))
                {


                   // myTheme.AllColors = themeManager.CopyTheme.AllColors;


                    //themeManager.AllThemes[i] = new ThemeInfo();
                    //themeManager.AllThemes[i].AllColors = new List<ThemeColorInfo>();

                    tmpTheme.AllColors = new List<ThemeColorInfo>();

                    //copy color
                    foreach (var myColor in themeManager.CopyTheme.AllColors)
                    {
                        ThemeColorInfo addNewColor = new ThemeColorInfo();
                        addNewColor.Color = myColor.Color;
                        addNewColor.Name = myColor.Name;

                        tmpTheme.AllColors.Add(addNewColor);
                        

                    }
                }
                GUILayout.EndHorizontal();

                themeManager.AllThemes[i] = tmpTheme;

                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }



            //filePath = EditorGUILayout.TextField("Path", filePath);
            // fileName = EditorGUILayout.TextField("Name", fileName);

            // _choiceIndex = EditorGUILayout.Popup(_choiceIndex, themeManager.ThemeType.ToArray());

            //no need for save
            EditorGUILayout.Space();
            if (GUILayout.Button("Save"))
             {
                PrefabUtility.ApplyPrefabInstance(themeManager.gameObject, InteractionMode.UserAction);
            }

            bool updateTheme = false;
            if (EditorGUI.EndChangeCheck())
            {
                updateTheme = true;
               // Debug.Log("SOMETHING CHANGED IN THEME! UPDATE ALL THEME ELEMENT");
            }

            if(addNewTheme)
            {
                themeManager.AllThemes.Add(new ThemeInfo());
                PrefabUtility.ApplyPrefabInstance(themeManager.gameObject, InteractionMode.UserAction);
            }

            if(deleteTheme)
            {
                themeManager.AllThemes.RemoveAt(deleteThemeIndex);
                PrefabUtility.ApplyPrefabInstance(themeManager.gameObject, InteractionMode.UserAction);
            }


            serializedObject.ApplyModifiedProperties();

            if(updateTheme)
            {
                themeManager.OnThemeUpdate.Invoke();


                //if(!Application.isPlaying)
                //  PrefabUtility.ApplyPrefabInstance(themeManager.gameObject, InteractionMode.UserAction);

                editorTimer = 0;
                saveTheme = true;
               
              

            }


        }


 
             




        int editorTimer;

        protected virtual void OnEditorUpdate()
        {
            editorTimer++;

            if (editorTimer > 60 && saveTheme)
            {
              
                Save();
            }

            // In here you can check the current realtime, see if a certain
            // amount of time has elapsed, and perform some task.
        }

        public void Save()
        {
            Debug.Log("SAVE THEME");
            PrefabUtility.ApplyPrefabInstance(themeManager.gameObject, InteractionMode.UserAction);
            saveTheme = false;
        }
    }
}
#endif