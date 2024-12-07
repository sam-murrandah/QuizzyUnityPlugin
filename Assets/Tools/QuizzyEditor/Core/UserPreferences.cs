using UnityEditor;
using UnityEngine;

namespace QuizGraphEditor
{
    public static class UserPreferences
    {
        // Existing colour preferences
        public static Color MultipleChoiceColor { get; set; } = new Color(0.941f, 0.972f, 1f); // Example
        public static Color TrueFalseColor { get; set; } = new Color(1f, 1f, 0.8f); // Example
        public static Color StartNodeColor { get; set; } = new Color(0.902f, 1f, 0.902f); // Example

        // New text colour preferences
        public static Color MultipleChoiceTextColor
        {
            get => GetColor("QuizGraphEditor_MultipleChoiceTextColor", Color.black);
            set => SetColor("QuizGraphEditor_MultipleChoiceTextColor", value);
        }

        public static Color TrueFalseTextColor
        {
            get => GetColor("QuizGraphEditor_TrueFalseTextColor", Color.black);
            set => SetColor("QuizGraphEditor_TrueFalseTextColor", value);
        }

        public static Color StartNodeTextColor
        {
            get => GetColor("QuizGraphEditor_StartNodeTextColor", Color.black);
            set => SetColor("QuizGraphEditor_StartNodeTextColor", value);
        }

        private static Color GetColor(string key, Color defaultColor)
        {
            if (EditorPrefs.HasKey(key))
            {
                string colorString = EditorPrefs.GetString(key);
                if (ColorUtility.TryParseHtmlString(colorString, out var color))
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
