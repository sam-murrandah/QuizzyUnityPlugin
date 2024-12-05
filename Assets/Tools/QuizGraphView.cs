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
        public QuizGraphView(EditorWindow editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("QuizGraphStyles"));

            // Setup zoom levels
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Add grid background
            AddGridBackground();

            // Add manipulators
            AddManipulators();

            // Add the start node
            AddElement(CreateStartNode());

            // Add search window
            AddSearchWindow(editorWindow);

            // Set up the minimap
            AddMinimap();
        }
        #endregion

        #region Setup Methods
        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }

        private void AddGridBackground()
        {
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        private void AddSearchWindow(EditorWindow editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Initialize(this, editorWindow);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        private void AddMinimap()
        {
            var miniMap = new MiniMap { anchored = true };
            miniMap.SetPosition(new Rect(10, 30, 200, 140));
            Add(miniMap);
        }
        #endregion

        #region Node Creation
        public void CreateNode(Type type, Vector2 position)
        {
            Node node = null;

            if (type == typeof(QuestionNode))
                node = new QuestionNode();
            else if (type == typeof(StartNode))
                node = new StartNode();

            if (node != null)
            {
                node.SetPosition(new Rect(position, new Vector2(200, 150)));
                AddElement(node);
            }
        }

        private Node CreateStartNode()
        {
            var startNode = new StartNode();
            startNode.SetPosition(new Rect(100, 200, 150, 100));
            return startNode;
        }

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
        #endregion

        #region Serialization
        public void SaveGraph(string filePath)
        {
            var quizData = new QuizData();

            // Save nodes
            graphElements.ForEach(element =>
            {
                if (element is QuestionNode questionNode)
                    quizData.questionNodes.Add(questionNode.GetData());
                else if (element is StartNode startNode)
                    quizData.startNodeData = startNode.GetData();
            });

            // Save edges
            foreach (var edge in edges)
            {
                var outputNode = edge.output.node as BaseNode;
                var inputNode = edge.input.node as BaseNode;

                quizData.edges.Add(new EdgeData
                {
                    outputNodeGUID = outputNode.GUID,
                    outputPortName = edge.output.portName,
                    inputNodeGUID = inputNode.GUID,
                    inputPortName = edge.input.portName
                });
            }

            // Serialize to JSON
            var jsonData = JsonUtility.ToJson(quizData, true);
            System.IO.File.WriteAllText(filePath, jsonData);
        }

        public void LoadGraph(string filePath)
        {
            ClearGraph();

            var jsonData = System.IO.File.ReadAllText(filePath);
            var quizData = JsonUtility.FromJson<QuizData>(jsonData);

            // Create nodes
            var nodesDict = new Dictionary<string, Node>();
            foreach (var nodeData in quizData.questionNodes)
            {
                var node = new QuestionNode(nodeData);
                AddElement(node);
                nodesDict[nodeData.GUID] = node;
            }

            if (quizData.startNodeData != null)
            {
                var startNode = new StartNode(quizData.startNodeData);
                AddElement(startNode);
                nodesDict[quizData.startNodeData.GUID] = startNode;
            }

            // Create edges
            foreach (var edgeData in quizData.edges)
            {
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
                var inputPort = inputNode.inputContainer.Q<Port>(edgeData.inputPortName);

                if (outputPort == null)
                {
                    Debug.LogError($"Output port '{edgeData.outputPortName}' not found in node {outputNode.title} (GUID: {edgeData.outputNodeGUID}).");
                    continue;
                }

                if (inputPort == null)
                {
                    Debug.LogError($"Input port '{edgeData.inputPortName}' not found in node {inputNode.title} (GUID: {edgeData.inputNodeGUID}).");
                    continue;
                }

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


        private void ClearGraph()
        {
            // Remove all nodes and edges
            graphElements.ForEach(RemoveElement);
        }
        #endregion
    }
}
