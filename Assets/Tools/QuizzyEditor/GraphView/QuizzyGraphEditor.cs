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
            rootVisualElement.style.flexDirection = FlexDirection.Column;

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

            _toolbarMenu = new ToolbarMenu { text = "Graph View" };
            _toolbarMenu.menu.AppendAction("Graph View", action => ShowGraphView(), DropdownMenuAction.Status.Normal);
            _toolbarMenu.menu.AppendAction("Settings", action => ShowSettingsView(), DropdownMenuAction.Status.Normal);
            toolbar.Add(_toolbarMenu);

            var saveButton = new Button(SaveData) { text = "Save" };
            toolbar.Add(saveButton);

            var loadButton = new Button(LoadData) { text = "Load" };
            toolbar.Add(loadButton);

            rootVisualElement.Add(toolbar);
        }

        private void ConstructGraphView()
        {
            _graphView = new QuizGraphView(this) { name = "Quiz Graph" };
            _graphView.style.flexGrow = 1;
            _graphView.style.display = DisplayStyle.None; // Hidden initially until ShowGraphView() is called
            rootVisualElement.Add(_graphView);
        }

        private void ConstructSettingsView()
        {
            _settingsView = new VisualElement { name = "Settings View" };
            _settingsView.style.flexGrow = 1;
            _settingsView.style.display = DisplayStyle.None; // Hidden initially until ShowSettingsView() is called

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
            // Add section for Start Node
            AddNodeSettingsSection(
                settingsContainer,
                "Start Node",
                UserPreferences.StartNodeColor,
                UserPreferences.StartNodeTextColor,
                (newNodeColor) => { UserPreferences.StartNodeColor = newNodeColor; ApplyStyles(); }
                //(newTextColor) => { UserPreferences.StartNodeTextColor = newTextColor; ApplyStyles(); }
            );
            // Add section for Multiple Choice Node
            AddNodeSettingsSection(
                settingsContainer,
                "Multiple Choice Node",
                UserPreferences.MultipleChoiceColor,
                UserPreferences.MultipleChoiceTextColor,
                (newNodeColor) => { UserPreferences.MultipleChoiceColor = newNodeColor; ApplyStyles(); }
               // (newTextColor) => { UserPreferences.MultipleChoiceTextColor = newTextColor; ApplyStyles(); }
            );

            // Add section for True/False Node
            AddNodeSettingsSection(
                settingsContainer,
                "True/False Node",
                UserPreferences.TrueFalseColor,
                UserPreferences.TrueFalseTextColor,
                (newNodeColor) => { UserPreferences.TrueFalseColor = newNodeColor; ApplyStyles(); }
                //(newTextColor) => { UserPreferences.TrueFalseTextColor = newTextColor; ApplyStyles(); }
            );
            _settingsView.Add(settingsContainer);
            rootVisualElement.Add(_settingsView);
        }

        private void AddNodeSettingsSection(
            VisualElement container,
            string sectionTitle,
            Color nodeColor,
            Color textColor,
            Action<Color> onNodeColorChanged
            //Action<Color> onTextColorChanged
        )
        {
            // Add a header for the node type
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
            var nodeColorField = new ColorField("Node Color")
            {
                value = nodeColor
            };
            nodeColorField.RegisterValueChangedCallback(evt => onNodeColorChanged(evt.newValue));
            container.Add(nodeColorField);

            // Text Color field
            // TODO: Text color isnt updating, likely an issue to do with ApplyColors(), commenting this out to disable it
            /*
             * var textColorField = new ColorField("Text Color")
            {
                value = textColor
            };
            textColorField.RegisterValueChangedCallback(evt => onTextColorChanged(evt.newValue));
            container.Add(textColorField);
            */
            // Add spacing between sections
            var spacer = new VisualElement
            {
                style =
                {
                    height = 3,
                    marginTop = 10,
                    marginBottom = 10,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f) // Optional separator
                }
            };
            container.Add(spacer);
        }

        private void AddColorField(VisualElement container, string label, Color initialValue, Action<Color> onChange)
        {
            var colorField = new ColorField(label)
            {
                value = initialValue
            };
            colorField.RegisterValueChangedCallback(evt => onChange(evt.newValue));
            container.Add(colorField);
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

        private void ApplyStyles()
        {
            if (_graphView == null) return;

            foreach (var element in _graphView.graphElements)
            {
                if (element is BaseQuestionNode bqn)
                    bqn.ApplyColorPreferences();
                else if (element is StartNode sNode)
                    sNode.ApplyColorPreferences();
            }
        }
    }
}
