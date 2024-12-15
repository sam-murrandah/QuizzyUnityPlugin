/*
Made by Samuel Murrandah
Student Number: 1031741
Student Email: 1031741@student.sae.edu.au
Class Code: GPG315
Assignment: 2 

AI Declaration:
Generative AI was used for editing and organisation such as reordering functions as well as some comments.
All code and logic was created and written by me
*/

using UnityEditor;
using UnityEngine;

namespace QuizGraphEditor
{
    public static class UserPreferences
    {
        #region Constants and Defaults
        private const string PrefKeyStartNodeColor = "QuizGraphEditor_StartNodeColor";
        private const string PrefKeyMultipleChoiceColor = "QuizGraphEditor_MultipleChoiceColor";
        private const string PrefKeyTrueFalseColor = "QuizGraphEditor_TrueFalseColor";

        // Default colours
        private static readonly Color DefaultStartColor = new Color(0.9f, 1f, 0.9f);
        private static readonly Color DefaultMCColor = new Color(0.94f, 0.97f, 1f);
        private static readonly Color DefaultTFColor = new Color(1f, 1f, 0.8f);
        #endregion

        #region Properties
        public static Color StartNodeColor { get; set; } = DefaultStartColor;
        public static Color MultipleChoiceColor { get; set; } = DefaultMCColor;
        public static Color TrueFalseColor { get; set; } = DefaultTFColor;
        #endregion

        #region Methods
        /// <summary>
        /// Loads user preferences for node colours from the editor settings.
        /// </summary>
        public static void LoadPreferences()
        {
            StartNodeColor = LoadColor(PrefKeyStartNodeColor, DefaultStartColor);
            MultipleChoiceColor = LoadColor(PrefKeyMultipleChoiceColor, DefaultMCColor);
            TrueFalseColor = LoadColor(PrefKeyTrueFalseColor, DefaultTFColor);
        }

        /// <summary>
        /// Saves user preferences for node colours to the editor settings.
        /// </summary>
        public static void SavePreferences()
        {
            SaveColor(PrefKeyStartNodeColor, StartNodeColor);
            SaveColor(PrefKeyMultipleChoiceColor, MultipleChoiceColor);
            SaveColor(PrefKeyTrueFalseColor, TrueFalseColor);
        }

        /// <summary>
        /// Loads a colour from the editor preferences or uses a default value if not set.
        /// </summary>
        /// <param name="key">The key for the preference.</param>
        /// <param name="defaultColor">The default colour to use if the preference is not found.</param>
        /// <returns>The loaded or default colour.</returns>
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

        /// <summary>
        /// Saves a colour to the editor preferences.
        /// </summary>
        /// <param name="key">The key for the preference.</param>
        /// <param name="color">The colour to save.</param>
        private static void SaveColor(string key, Color color)
        {
            string colorString = ColorUtility.ToHtmlStringRGBA(color);
            EditorPrefs.SetString(key, $"#{colorString}");
        }
        #endregion
    }
}