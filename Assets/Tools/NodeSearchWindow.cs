using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace QuizGraphEditor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private QuizGraphView _graphView;
        private EditorWindow _window;

        public void Initialize(QuizGraphView graphView, EditorWindow window)
        {
            _graphView = graphView;
            _window = window;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Quiz Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Question Node"))
                {
                    level = 2,
                    userData = typeof(QuestionNode)
                },
                new SearchTreeEntry(new GUIContent("Start Node"))
                {
                    level = 2,
                    userData = typeof(StartNode)
                }
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var mousePosition = _window.rootVisualElement.ChangeCoordinatesTo(
                _window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);

            _graphView.CreateNode((Type)entry.userData, graphMousePosition);

            return true;
        }
    }
}
