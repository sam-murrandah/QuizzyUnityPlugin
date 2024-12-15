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
    public class QuizGraphView : GraphView
    {
        private NodeSearchWindow _searchWindow;

        #region Constructor
        /// <summary>
        /// Initializes the QuizGraphView, setting up zoom levels, manipulators, nodes, and UI elements.
        /// </summary>
        public QuizGraphView(EditorWindow editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("QuizGraphStyles"));

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            AddGridBackground();
            AddManipulators();
            AddElement(CreateStartNode());
            AddSearchWindow(editorWindow);
            AddMinimap();
        }
        #endregion

        #region Setup Methods

        /// <summary>
        /// Adds standard manipulators to the GraphView.
        /// </summary>
        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }

        /// <summary>
        /// Adds a grid background to the GraphView.
        /// </summary>
        private void AddGridBackground()
        {
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        /// <summary>
        /// Adds the search window for creating nodes.
        /// </summary>
        private void AddSearchWindow(EditorWindow editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Initialize(this, editorWindow);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        /// <summary>
        /// Adds a minimap to the GraphView.
        /// </summary>
        private void AddMinimap()
        {
            var miniMap = new MiniMap { anchored = true };
            miniMap.SetPosition(new Rect(10, 30, 200, 140));
            Add(miniMap);
        }

        #endregion

        #region Node Creation

        /// <summary>
        /// Creates a new node of the specified type at the given position.
        /// </summary>
        public void CreateNode(Type type, Vector2 position)
        {
            Node node = null;

            if (type == typeof(MultipleChoiceNode))
                node = new MultipleChoiceNode(position);
            else if (type == typeof(TrueFalseNode))
                node = new TrueFalseNode(position);
            else if (type == typeof(StartNode))
                node = new StartNode();

            if (node != null)
            {
                node.SetPosition(new Rect(position, new Vector2(200, 150)));
                AddElement(node);
            }
        }

        /// <summary>
        /// Creates the initial start node.
        /// </summary>
        private Node CreateStartNode()
        {
            var startNode = new StartNode();
            startNode.SetPosition(new Rect(100, 200, 150, 100));
            return startNode;
        }

        /// <summary>
        /// Ensures compatibility between ports for connections.
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port || startPort.node == port.node || startPort.direction == port.direction)
                    return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        /// <summary>
        /// Creates a Multiple Choice node at the given position.
        /// </summary>
        public void CreateMultipleChoiceNode(Vector2 position)
        {
            var node = new MultipleChoiceNode(position);
            AddElement(node);
        }

        /// <summary>
        /// Creates a True/False node at the given position.
        /// </summary>
        public void CreateTrueFalseNode(Vector2 position)
        {
            var node = new TrueFalseNode(position);
            AddElement(node);
        }

        /// <summary>
        /// Checks if a start node already exists in the graph.
        /// </summary>
        public bool HasStartNode()
        {
            foreach (var element in graphElements)
            {
                if (element is StartNode)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a start node if none exists.
        /// </summary>
        public void CreateStartNode(Vector2 position)
        {
            if (HasStartNode()) return;

            var startNode = new StartNode();
            startNode.SetPosition(new Rect(position, new Vector2(150, 100)));
            AddElement(startNode);
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Saves the graph structure to a file.
        /// </summary>
        public void SaveGraph(string filePath)
        {
            var quizData = new QuizData();

            graphElements.ForEach(element =>
            {
                if (element is MultipleChoiceNode mcNode)
                    quizData.multipleChoiceNodes.Add((MultipleChoiceNodeData)mcNode.GetData());
                else if (element is TrueFalseNode tfNode)
                    quizData.trueFalseNodes.Add((TrueFalseNodeData)tfNode.GetData());
                else if (element is StartNode startNode)
                    quizData.startNodeData = startNode.GetData();
            });

            foreach (var edge in edges)
            {
                var outputNode = edge.output.node as Node;
                var inputNode = edge.input.node as Node;
                var outputGUID = GetGUIDFromNode(outputNode);
                var inputGUID = GetGUIDFromNode(inputNode);

                if (string.IsNullOrEmpty(outputGUID) || string.IsNullOrEmpty(inputGUID))
                {
                    Debug.LogError("Skipping edge due to null or empty GUID. Check your graph for invalid connections.");
                    continue;
                }

                quizData.edges.Add(new EdgeData
                {
                    outputNodeGUID = outputGUID,
                    outputPortName = edge.output.portName,
                    inputNodeGUID = inputGUID,
                    inputPortName = edge.input.portName
                });
            }

            var jsonData = JsonUtility.ToJson(quizData, true);
            System.IO.File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Helper function to extract the GUID from a node.
        /// </summary>
        private string GetGUIDFromNode(Node node)
        {
            if (node is BaseNode baseNode) return baseNode.GUID;
            if (node is BaseQuestionNode baseQuestionNode) return baseQuestionNode.GUID;
            if (node is StartNode start) return start.GUID;
            return string.Empty;
        }

        /// <summary>
        /// Loads a graph structure from a file.
        /// </summary>
        public void LoadGraph(string filePath)
        {
            ClearGraph();

            var jsonData = System.IO.File.ReadAllText(filePath);
            var quizData = JsonUtility.FromJson<QuizData>(jsonData);

            var nodesDict = new Dictionary<string, Node>();

            foreach (var mcData in quizData.multipleChoiceNodes)
            {
                var node = new MultipleChoiceNode();
                node.LoadData(mcData);
                AddElement(node);
                nodesDict[mcData.GUID] = node;
                //LogNodePorts(node);
            }

            foreach (var tfData in quizData.trueFalseNodes)
            {
                var node = new TrueFalseNode();
                node.LoadData(tfData);
                AddElement(node);
                nodesDict[tfData.GUID] = node;
                //LogNodePorts(node);
            }

            if (quizData.startNodeData != null)
            {
                var startNode = new StartNode(quizData.startNodeData);
                AddElement(startNode);
                nodesDict[quizData.startNodeData.GUID] = startNode;
                //LogNodePorts(startNode);
            }

            foreach (var edgeData in quizData.edges)
            {
                //Debug.Log($"Attempting to create edge from {edgeData.outputNodeGUID} ({edgeData.outputPortName}) to {edgeData.inputNodeGUID} ({edgeData.inputPortName})");

                if (!nodesDict.TryGetValue(edgeData.outputNodeGUID, out var outputNode))
                {
                    Debug.LogError($"Output node with GUID {edgeData.outputNodeGUID} not found.");
                    continue;
                }
                if (!nodesDict.TryGetValue(edgeData.inputNodeGUID, out var inputNode))
                {
                    Debug.LogError($"Input node with GUID {edgeData.inputNodeGUID} not found.");
                    continue;
                }

                var outputPort = outputNode.outputContainer.Q<Port>(edgeData.outputPortName);
                if (outputPort == null)
                {
                    Debug.LogError($"Output port '{edgeData.outputPortName}' not found on node {outputNode.title} (GUID: {edgeData.outputNodeGUID}).");
                }

                var inputPort = inputNode.inputContainer.Q<Port>(edgeData.inputPortName);
                if (inputPort == null)
                {
                    Debug.LogError($"Input port '{edgeData.inputPortName}' not found on node {inputNode.title} (GUID: {edgeData.inputNodeGUID}).");
                }

                if (outputPort != null && inputPort != null)
                {
                    var edge = new Edge
                    {
                        output = outputPort,
                        input = inputPort
                    };
                    edge.input.Connect(edge);
                    edge.output.Connect(edge);
                    AddElement(edge);
                }
            }
        }

        /// <summary>
        /// Logs the ports of a node for debugging purposes.
        /// </summary>
        private void LogNodePorts(Node node)
        {
            Debug.Log($"Logging ports for node: {node.title}, GUID: {(node is BaseNode bn ? bn.GUID : (node is StartNode sn ? sn.GUID : "No GUID"))}");

            foreach (var child in node.inputContainer.Children())
            {
                if (child is Port p)
                {
                    Debug.Log($"{node.title} Input Port: {p.name}");
                }
            }

            foreach (var child in node.outputContainer.Children())
            {
                if (child is Port p)
                {
                    Debug.Log($"{node.title} Output Port: {p.name}");
                }
            }
        }

        /// <summary>
        /// Clears the graph by removing all nodes and edges.
        /// </summary>
        private void ClearGraph()
        {
            graphElements.ForEach(RemoveElement);
        }

        #endregion
    }
}
