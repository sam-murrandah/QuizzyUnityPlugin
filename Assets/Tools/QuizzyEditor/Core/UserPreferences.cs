using UnityEditor;
using UnityEngine;

namespace QuizGraphEditor
{
    public static class UserPreferences
    {
        private const string PrefKeyStartNodeColor = "QuizGraphEditor_StartNodeColor";
        private const string PrefKeyMultipleChoiceColor = "QuizGraphEditor_MultipleChoiceColor";
        private const string PrefKeyTrueFalseColor = "QuizGraphEditor_TrueFalseColor";

        // Default colours
        private static readonly Color DefaultStartColor = new Color(0.9f, 1f, 0.9f);
        private static readonly Color DefaultMCColor = new Color(0.94f, 0.97f, 1f);
        private static readonly Color DefaultTFColor = new Color(1f, 1f, 0.8f);

        public static Color StartNodeColor { get; set; } = DefaultStartColor;
        public static Color MultipleChoiceColor { get; set; } = DefaultMCColor;
        public static Color TrueFalseColor { get; set; } = DefaultTFColor;

        public static void LoadPreferences()
        {
            StartNodeColor = LoadColor(PrefKeyStartNodeColor, DefaultStartColor);
            MultipleChoiceColor = LoadColor(PrefKeyMultipleChoiceColor, DefaultMCColor);
            TrueFalseColor = LoadColor(PrefKeyTrueFalseColor, DefaultTFColor);
        }

        public static void SavePreferences()
        {
            SaveColor(PrefKeyStartNodeColor, StartNodeColor);
            SaveColor(PrefKeyMultipleChoiceColor, MultipleChoiceColor);
            SaveColor(PrefKeyTrueFalseColor, TrueFalseColor);
        }

        private static Color LoadColor(string key, Color defaultColor)
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

        private static void SaveColor(string key, Color color)
        {
            string colorString = ColorUtility.ToHtmlStringRGBA(color);
            EditorPrefs.SetString(key, $"#{colorString}");
        }
    }
}
