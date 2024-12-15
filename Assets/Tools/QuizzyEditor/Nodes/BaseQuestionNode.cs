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
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuizGraphEditor
{
    public enum Difficulty { Easy, Medium, Hard }

    public abstract class BaseQuestionNode : BaseNode
    {
        public string QuestionText { get; protected set; }
        public string Category { get; protected set; }
        public Difficulty DifficultyLevel { get; protected set; }
        public float TimeLimit { get; protected set; }
        public int PointValue { get; protected set; }
        public string Explanation { get; protected set; }

        protected VisualElement answerFieldsContainer;

        public BaseQuestionNode()
        {
            InitializeNode();
        }

        protected BaseQuestionNode(Vector2 position)
        {
            InitializeNode();
            SetPosition(new Rect(position, new Vector2(200, 150)));
        }

        protected virtual void InitializeNode()
        {
            title = "Question";
            GUID = Guid.NewGuid().ToString();
            //Debug.Log("Node " + this.title + " GUID: " + GUID);
            capabilities = Capabilities.Movable | Capabilities.Selectable | Capabilities.Deletable | Capabilities.Resizable;

            CreatePorts();
            AddFields();

            RefreshExpandedState();
            RefreshPorts();
            ApplyColorPreferences();
        }

        protected virtual void CreatePorts()
        {
            // Input port
            var input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            input.portName = "Input";
            input.name = input.portName; // For serialization
            inputContainer.Add(input);
        }

        protected TextField categoryField;
        protected EnumField difficultyField;
        protected FloatField timerField;
        protected IntegerField pointsField;
        protected TextField questionField;
        protected TextField explanationField;

        protected virtual void AddFields()
        {
            var container = new VisualElement();

            // Category field
            categoryField = new TextField("Category:");
            categoryField.RegisterValueChangedCallback(evt => Category = evt.newValue);
            container.Add(categoryField);

            // Difficulty dropdown
            difficultyField = new EnumField("Difficulty", Difficulty.Medium);
            difficultyField.RegisterValueChangedCallback(evt => DifficultyLevel = (Difficulty)evt.newValue);
            container.Add(difficultyField);

            // Time limit field
            timerField = new FloatField("Time Limit (seconds):");
            timerField.RegisterValueChangedCallback(evt => TimeLimit = evt.newValue);
            container.Add(timerField);

            // Point value field
            pointsField = new IntegerField("Point Value:");
            pointsField.RegisterValueChangedCallback(evt => PointValue = evt.newValue);
            container.Add(pointsField);

            // Question text
            questionField = new TextField("Question:");
            questionField.multiline = true;
            questionField.RegisterValueChangedCallback(evt => QuestionText = evt.newValue);
            container.Add(questionField);

            // Explanation field
            explanationField = new TextField("Explanation:");
            explanationField.multiline = true;
            explanationField.RegisterValueChangedCallback(evt => Explanation = evt.newValue);
            container.Add(explanationField);

            extensionContainer.Add(container);

            // Initialize answer fields container
            answerFieldsContainer = new VisualElement();
            extensionContainer.Add(answerFieldsContainer);

            UpdateAnswerFields();
            UpdateOutputPorts();

            RefreshExpandedState();
        }

        /// <summary>
        /// Called after LoadData() to update UI fields with data values.
        /// </summary>
        protected void RefreshUIFromData()
        {
            if (categoryField != null)
                categoryField.SetValueWithoutNotify(Category);

            if (difficultyField != null)
                difficultyField.SetValueWithoutNotify(DifficultyLevel);

            if (timerField != null)
                timerField.SetValueWithoutNotify(TimeLimit);

            if (pointsField != null)
                pointsField.SetValueWithoutNotify(PointValue);

            if (questionField != null)
                questionField.SetValueWithoutNotify(QuestionText);

            if (explanationField != null)
                explanationField.SetValueWithoutNotify(Explanation);

            // Also refresh answer fields if needed
            UpdateAnswerFields();
            UpdateOutputPorts();
        }


        protected abstract void UpdateAnswerFields();
        protected abstract void UpdateOutputPorts();

        public abstract void ApplyColorPreferences();

        public abstract BaseQuestionNodeData GetData();
        public abstract void LoadData(BaseQuestionNodeData data);
    }
}
