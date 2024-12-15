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

using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace QuizGraphEditor
{
    public class StartNode : Node
    {
        public string GUID { get; private set; }

        public StartNode()
        {
            InitializeNode();
        }

        public StartNode(StartNodeData data)
        {
            InitializeNode();

            // Load data
            GUID = data.GUID;
            SetPosition(new Rect(data.Position, Vector2.zero));
            ApplyColorPreferences();
        }

        /// <summary>
        /// Initializes the Start Node with a single output port and default settings.
        /// </summary>
        private void InitializeNode()
        {
            title = "Start";
            GUID = Guid.NewGuid().ToString();
            //Debug.Log("Node " + this.title + " GUID: " + GUID);
            capabilities = Capabilities.Movable | Capabilities.Selectable;

            // Create a single output port
            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort.portName = "Start Quiz";
            outputPort.name = outputPort.portName; // For serialization
            outputContainer.Add(outputPort);
            AddToClassList("start-node");

            RefreshExpandedState();
            RefreshPorts();
            ApplyColorPreferences();
        }

        /// <summary>
        /// Applies the color preferences to the Start Node based on user settings.
        /// </summary>
        public void ApplyColorPreferences()
        {
            style.backgroundColor = UserPreferences.StartNodeColor;
        }

        /// <summary>
        /// Serializes the Start Node's data into a StartNodeData object.
        /// </summary>
        public StartNodeData GetData()
        {
            return new StartNodeData
            {
                GUID = GUID,
                Position = GetPosition().position
            };
        }
    }
}
