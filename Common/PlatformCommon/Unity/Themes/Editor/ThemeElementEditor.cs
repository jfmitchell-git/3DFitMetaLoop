#if !BACKOFFICE

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.Themes
{



    [CustomEditor(typeof(ThemeElement))]
    public class ThemeElementEditor : Editor
    {

        ThemeElement themeElement;
        ThemeManager themeManager;

        private void OnEnable()
        {
            themeElement = (ThemeElement)target;
            themeManager = ThemeManager.Instance;
        }
        public override void OnInspectorGUI()
        {


            //llte fix in case
            themeManager = ThemeManager.Instance;

            // base.OnInspectorGUI();
            serializedObject.Update();

            //all color by name(string)
            List<string> allColorString = new List<string>();
            themeManager.AllColors.ForEach(p => allColorString.Add(p));

            int currentColorIndex1 = themeElement.CurrentThemeColorInfoIndex1;
            int currentColorIndex2 = themeElement.CurrentThemeColorInfoIndex2;

            float currentColorBrightness1 = themeElement.ColorBrightness1;
            float currentColorBrightness2 = themeElement.ColorBrightness2;

            if (currentColorIndex1 == -1)
                currentColorIndex1 = 0;
            if (currentColorIndex2 == -1)
                currentColorIndex2 = 0;

            string color1 = "Color: ";
            string color2 = "Color2: ";

            if (themeElement.Gradient != null)
            {
                color1 = "Vertex1: ";
                color2 = "Vertex2: ";
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(color1);
            currentColorIndex1 = EditorGUILayout.Popup(currentColorIndex1, allColorString.ToArray());
            GUILayout.EndHorizontal();

            currentColorBrightness1 = EditorGUILayout.Slider(currentColorBrightness1, -1, 1);

            if (themeElement.Gradient != null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(color2);
                currentColorIndex2 = EditorGUILayout.Popup(currentColorIndex2, allColorString.ToArray());
                GUILayout.EndHorizontal();

                currentColorBrightness2 = EditorGUILayout.Slider(currentColorBrightness2, -1, 1);
            }


            //EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentThemeColorInfo"));


            //always one selected
            if (currentColorIndex1 == -1)
                currentColorIndex1 = 0;
            if (currentColorIndex2 == -1)
                currentColorIndex2 = 0;

            bool refresh = false;
            if (EditorGUI.EndChangeCheck() || themeElement.CurrentThemeColorInfoIndex1 == -1)
            {
                refresh = true;
            }

            //this way work in prefabs!!
            serializedObject.FindProperty("CurrentThemeColorInfoIndex1").intValue = currentColorIndex1;
            serializedObject.FindProperty("CurrentThemeColorInfoIndex2").intValue = currentColorIndex2;

            serializedObject.FindProperty("ColorBrightness1").floatValue = currentColorBrightness1;
            serializedObject.FindProperty("ColorBrightness2").floatValue = currentColorBrightness2;


            serializedObject.ApplyModifiedProperties();

            if (refresh)
            {
                themeElement.UpdateColor();
            }
           

        }



    }
}
#endif