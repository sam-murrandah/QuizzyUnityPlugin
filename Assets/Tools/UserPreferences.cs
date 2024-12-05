using UnityEngine;
using UnityEditor;

namespace QuizGraphEditor
{
    public static class UserPreferences
    {
        private const string QuestionNodeColorKey = "QuizGraphEditor_QuestionNodeColor";
        private const string StartNodeColorKey = "QuizGraphEditor_StartNodeColor";
        private const string PortColorKey = "QuizGraphEditor_PortColor";

        public static Color QuestionNodeColor
        {
            get => GetColor(QuestionNodeColorKey, new Color(0.941f, 0.972f, 1f)); // Default: #f0f8ff
            set => SetColor(QuestionNodeColorKey, value);
        }

        public static Color StartNodeColor
        {
            get => GetColor(StartNodeColorKey, new Color(0.902f, 1f, 0.902f)); // Default: #e6ffe6
            set => SetColor(StartNodeColorKey, value);
        }

        public static Color PortColor
        {
            get => GetColor(PortColorKey, new Color(0.678f, 0.847f, 0.902f)); // Default: #add8e6
            set => SetColor(PortColorKey, value);
        }

        private static Color GetColor(string key, Color defaultColor)
        {
            if (EditorPrefs.HasKey(key))
            {
                string colorString = EditorPrefs.GetString(key);
                if (ColorUtility.TryParseHtmlString(colorString, out Color color))
                {
                    return color;
                }
            }
            return defaultColor;
        }

        private static void SetColor(string key, Color color)
        {
            string colorString = ColorUtility.ToHtmlStringRGBA(color);
            EditorPrefs.SetString(key, $"#{colorString}");
        }
    }
}
