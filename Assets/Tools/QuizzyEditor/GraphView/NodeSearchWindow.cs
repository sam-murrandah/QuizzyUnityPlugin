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

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuizGraphEditor
{
    public enum CreationNodeType
    {
        MultipleChoiceQuestion,
        TrueFalseQuestion,
        StartNode
    }

    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private QuizGraphView _graphView;
        private EditorWindow _editorWindow;

        /// <summary>
        /// Initializes the NodeSearchWindow with the associated graph view and editor window.
        /// </summary>
        public void Initialize(QuizGraphView graphView, EditorWindow editorWindow)
        {
            _graphView = graphView;
            _editorWindow = editorWindow;
        }

        /// <summary>
        /// Creates the search tree for the node creation menu.
        /// </summary>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Quiz Nodes"), 1),

                new SearchTreeEntry(new GUIContent("Multiple Choice Question Node"))
                {
                    level = 2, userData = CreationNodeType.MultipleChoiceQuestion
                },
                new SearchTreeEntry(new GUIContent("True/False Question Node"))
                {
                    level = 2, userData = CreationNodeType.TrueFalseQuestion
                }
            };

            // Only add StartNode if one doesn't exist already
            if (!_graphView.HasStartNode())
            {
                tree.Add(new SearchTreeEntry(new GUIContent("Start Node"))
                {
                    level = 2,
                    userData = CreationNodeType.StartNode
                });
            }

            return tree;
        }

        /// <summary>
        /// Handles the selection of a node creation entry from the search window.
        /// </summary>
        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            Vector2 mousePosition = _editorWindow.rootVisualElement.ChangeCoordinatesTo(
                _editorWindow.rootVisualElement.parent,
                context.screenMousePosition - _editorWindow.position.position);
            Vector2 graphMousePosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);

            var creationType = (CreationNodeType)entry.userData;

            switch (creationType)
            {
                case CreationNodeType.MultipleChoiceQuestion:
                    _graphView.CreateMultipleChoiceNode(graphMousePosition);
                    break;
                case CreationNodeType.TrueFalseQuestion:
                    _graphView.CreateTrueFalseNode(graphMousePosition);
                    break;
                case CreationNodeType.StartNode:
                    _graphView.CreateStartNode(graphMousePosition);
                    break;
            }

            return true;
        }
    }
}
