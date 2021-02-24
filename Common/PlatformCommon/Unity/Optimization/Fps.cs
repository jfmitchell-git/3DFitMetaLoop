#if !BACKOFFICE
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.Optimization
{
    public class Fps : MonoBehaviour
    {

        float deltaTime = 0.0f;

        public float CurrentFPS;

        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {

            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(w - 300, h - 100, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 50;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.red;
            float msec = deltaTime * 1000.0f;
            CurrentFPS = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, CurrentFPS);
            GUI.Label(rect, text, style);
        }
    }
}
#endif