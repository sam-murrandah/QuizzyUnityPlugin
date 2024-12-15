using System;
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
            // Load stored preferences before building UI
            UserPreferences.LoadPreferences();

            // Main container layout
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            ConstructToolbar();
            ConstructGraphView();
            ConstructSettingsView();
            ShowGraphView();
        }

        private void OnDisable()
        {
            if (_graphView != null) rootVisualElement.Remove(_graphView);
            if (_settingsView != null) rootVisualElement.Remove(_settingsView);
        }

        /// <summary>
        /// Constructs the top toolbar with view switching and save/load actions.
        /// </summary>
        private void ConstructToolbar()
        {
            var toolbar = new Toolbar();

            _toolbarMenu = new ToolbarMenu { text = "Graph View" };
            _toolbarMenu.menu.AppendAction("Graph View", _ => ShowGraphView(), DropdownMenuAction.Status.Normal);
            _toolbarMenu.menu.AppendAction("Settings", _ => ShowSettingsView(), DropdownMenuAction.Status.Normal);
            toolbar.Add(_toolbarMenu);

            var saveButton = new Button(SaveData) { text = "Save" };
            toolbar.Add(saveButton);

            var loadButton = new Button(LoadData) { text = "Load" };
            toolbar.Add(loadButton);

            rootVisualElement.Add(toolbar);
        }

        /// <summary>
        /// Creates and configures the main graph view element.
        /// </summary>
        private void ConstructGraphView()
        {
            _graphView = new QuizGraphView(this)
            {
                name = "Quiz Graph",
                style =
                {
                    flexGrow = 1,
                    display = DisplayStyle.None
                }
            };
            rootVisualElement.Add(_graphView);
        }

        /// <summary>
        /// Creates and configures the settings view panel.
        /// </summary>
        private void ConstructSettingsView()
        {
            _settingsView = new VisualElement { name = "Settings View" };
            _settingsView.style.flexGrow = 1;
            _settingsView.style.display = DisplayStyle.None;

            var settingsContainer = new VisualElement
            {
                style =
                {
                    paddingTop = 10,
                    paddingLeft = 10,
                    paddingRight = 10
                }
            };

            var titleLabel = new Label("Settings")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 18
                }
            };
            settingsContainer.Add(titleLabel);

            // Start Node settings
            AddNodeSettingsSection(
                container: settingsContainer,
                sectionTitle: "Start Node",
                nodeColor: UserPreferences.StartNodeColor,
                onNodeColorChanged: newColor =>
                {
                    UserPreferences.StartNodeColor = newColor;
                    UserPreferences.SavePreferences();
                    ApplyStyles();
                }
            );

            // Multiple Choice Node settings
            AddNodeSettingsSection(
                container: settingsContainer,
                sectionTitle: "Multiple Choice Node",
                nodeColor: UserPreferences.MultipleChoiceColor,
                onNodeColorChanged: newColor =>
                {
                    UserPreferences.MultipleChoiceColor = newColor;
                    UserPreferences.SavePreferences();
                    ApplyStyles();
                }
            );

            // True/False Node settings
            AddNodeSettingsSection(
                container: settingsContainer,
                sectionTitle: "True/False Node",
                nodeColor: UserPreferences.TrueFalseColor,
                onNodeColorChanged: newColor =>
                {
                    UserPreferences.TrueFalseColor = newColor;
                    UserPreferences.SavePreferences();
                    ApplyStyles();
                }
            );

            _settingsView.Add(settingsContainer);
            rootVisualElement.Add(_settingsView);
        }

        /// <summary>
        /// Adds a settings section for a given node type (e.g., Start Node, Multiple Choice Node).
        /// </summary>
        private void AddNodeSettingsSection(
            VisualElement container,
            string sectionTitle,
            Color nodeColor,
            Action<Color> onNodeColorChanged
        )
        {
            var headerLabel = new Label(sectionTitle)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginTop = 10,
                    marginBottom = 5
                }
            };
            container.Add(headerLabel);

            // Node Color field
            var nodeColorField = new ColorField("Node Color") { value = nodeColor };
            nodeColorField.RegisterValueChangedCallback(evt => onNodeColorChanged(evt.newValue));
            container.Add(nodeColorField);

            // Optional spacer between sections
            var spacer = new VisualElement
            {
                style =
                {
                    height = 3,
                    marginTop = 10,
                    marginBottom = 10,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f)
                }
            };
            container.Add(spacer);
        }

        /// <summary>
        /// Switches to the Graph View.
        /// </summary>
        private void ShowGraphView()
        {
            _toolbarMenu.text = "Graph View";
            _graphView.style.display = DisplayStyle.Flex;
            _settingsView.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Switches to the Settings View.
        /// </summary>
        private void ShowSettingsView()
        {
            _toolbarMenu.text = "Settings";
            _graphView.style.display = DisplayStyle.None;
            _settingsView.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Saves the current graph data to a JSON file.
        /// </summary>
        private void SaveData()
        {
            var path = EditorUtility.SaveFilePanel("Save Quiz Graph", "", "QuizGraph.json", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _graphView.SaveGraph(path);
            }
        }

        /// <summary>
        /// Loads graph data from a JSON file.
        /// </summary>
        private void LoadData()
        {
            var path = EditorUtility.OpenFilePanel("Load Quiz Graph", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _graphView.LoadGraph(path);
            }
        }

        /// <summary>
        /// Applies updated color preferences to all nodes in the graph view.
        /// </summary>
        private void ApplyStyles()
        {
            if (_graphView == null) return;

            foreach (var element in _graphView.graphElements)
            {
                switch (element)
                {
                    case BaseQuestionNode bqn:
                        bqn.ApplyColorPreferences();
                        break;
                    case StartNode sNode:
                        sNode.ApplyColorPreferences();
                        break;
                }
            }
        }
    }
}
