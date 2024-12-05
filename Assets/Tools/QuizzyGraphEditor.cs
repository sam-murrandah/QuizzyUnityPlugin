using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuizGraphEditor
{
    public class QuizzyGraphEditor : EditorWindow
    {
        private QuizGraphView _graphView;
        private VisualElement _settingsView;
        private ToolbarMenu _toolbarMenu;

        [MenuItem("Tools/Quizzy Graph Editor")]
        public static void OpenGraphEditor()
        {
            var window = GetWindow<QuizzyGraphEditor>();
            window.titleContent = new GUIContent("Quizzy Graph");
        }

        private void OnEnable()
        {
            ConstructToolbar();
            ConstructGraphView();
            ConstructSettingsView();
            ShowGraphView();
        }

        private void OnDisable()
        {
            if (_graphView != null)
                rootVisualElement.Remove(_graphView);
            if (_settingsView != null)
                rootVisualElement.Remove(_settingsView);
        }

        private void ConstructToolbar()
        {
            var toolbar = new Toolbar();

            // Toolbar Menu for Tabs
            _toolbarMenu = new ToolbarMenu { text = "Graph View" };
            _toolbarMenu.menu.AppendAction("Graph View", action => ShowGraphView(), DropdownMenuAction.Status.Normal);
            _toolbarMenu.menu.AppendAction("Settings", action => ShowSettingsView(), DropdownMenuAction.Status.Normal);
            toolbar.Add(_toolbarMenu);

            // Save Button
            var saveButton = new Button(SaveData) { text = "Save" };
            toolbar.Add(saveButton);

            // Load Button
            var loadButton = new Button(LoadData) { text = "Load" };
            toolbar.Add(loadButton);

            rootVisualElement.Add(toolbar);
        }

        private void ConstructGraphView()
        {
            _graphView = new QuizGraphView(this) { name = "Quiz Graph" };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void ConstructSettingsView()
        {
            _settingsView = new VisualElement { name = "Settings View" };
            _settingsView.style.flexGrow = 1;
            _settingsView.style.display = DisplayStyle.None; // Start hidden

            // Add settings UI elements here (if any)
            var settingsContainer = new VisualElement();
            settingsContainer.style.paddingTop = 10;
            settingsContainer.style.paddingLeft = 10;
            settingsContainer.style.paddingRight = 10;

            var titleLabel = new Label("Settings")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 18
                }
            };
            settingsContainer.Add(titleLabel);

            _settingsView.Add(settingsContainer);
            rootVisualElement.Add(_settingsView);
        }

        private void ShowGraphView()
        {
            _toolbarMenu.text = "Graph View";
            _graphView.style.display = DisplayStyle.Flex;
            _settingsView.style.display = DisplayStyle.None;
        }

        private void ShowSettingsView()
        {
            _toolbarMenu.text = "Settings";
            _graphView.style.display = DisplayStyle.None;
            _settingsView.style.display = DisplayStyle.Flex;
        }

        private void SaveData()
        {
            var path = EditorUtility.SaveFilePanel("Save Quiz Graph", "", "QuizGraph.json", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _graphView.SaveGraph(path);
            }
        }

        private void LoadData()
        {
            var path = EditorUtility.OpenFilePanel("Load Quiz Graph", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _graphView.LoadGraph(path);
            }
        }
    }
}
