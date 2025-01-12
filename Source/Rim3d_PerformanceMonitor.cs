using UnityEngine;

namespace Rim3D
{
    public static class PerformanceMonitor
    {
        private static GUIStyle style;
        private static readonly Color backgroundColor = new Color(0, 0, 0, 0.5f);
        private static readonly Color textColor = Color.white;
        private static string cachedText = "";
        private static float updateTimer;
        private static readonly float updateInterval = 0.5f;

        public static void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                float fps = 1.0f / Time.deltaTime;
                cachedText = $"FPS: {Mathf.Round(fps)}";
            }
        }

        public static void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
                style.normal.textColor = textColor;
            }

            Rect rect = new Rect(100, 10, 120, 30);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, backgroundColor, 0, 0);
            GUI.Label(rect, cachedText, style);
        }
    }
}
