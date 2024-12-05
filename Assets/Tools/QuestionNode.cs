using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace QuizGraphEditor
{
    public enum QuestionType { MultipleChoice, TrueFalse }
    public enum Difficulty { Easy, Medium, Hard }

    public class QuestionNode : Node
    {
        public string QuestionText { get; private set; }
        public string[] Answers { get; private set; }
        public int CorrectAnswerIndex { get; private set; }
        public string Category { get; private set; }
        public Difficulty DifficultyLevel { get; private set; }
        public float TimeLimit { get; private set; }
        public int PointValue { get; private set; }
        public string Explanation { get; private set; }
        public QuestionType questionType;
        public string GUID { get; private set; }

        private VisualElement answerFieldsContainer;

        public QuestionNode()
        {
            InitializeNode();
        }

        public QuestionNode(QuestionNodeData data)
        {
            InitializeNode();

            // Load data
            GUID = data.GUID;
            QuestionText = data.QuestionText;
            Answers = data.Answers;
            CorrectAnswerIndex = data.CorrectAnswerIndex;
            Category = data.Category;
            DifficultyLevel = data.DifficultyLevel;
            TimeLimit = data.TimeLimit;
            PointValue = data.PointValue;
            Explanation = data.Explanation;
            questionType = data.questionType;

            SetPosition(new Rect(data.Position, Vector2.zero));

            // Update UI elements with loaded data
            LoadFields();
            UpdateAnswerFields();
            UpdateOutputPorts();
        }

        private void InitializeNode()
        {
            title = "Question";
            GUID = Guid.NewGuid().ToString();
            capabilities = Capabilities.Movable | Capabilities.Selectable | Capabilities.Deletable | Capabilities.Resizable;

            CreatePorts();
            AddFields();
            UpdateOutputPorts();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void CreatePorts()
        {
            // Input port
            var input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            input.portName = "Input";
            input.name = input.portName; // For serialization
            inputContainer.Add(input);

            // Output ports
            UpdateOutputPorts();
        }

        private void UpdateOutputPorts()
        {
            outputContainer.Clear();

            if (questionType == QuestionType.MultipleChoice)
            {
                if (Answers != null)
                {
                    for (int i = 0; i < Answers.Length; i++)
                    {
                        var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                        output.portName = $"Answer {i + 1}";
                        output.name = output.portName; // For serialization
                        outputContainer.Add(output);
                    }
                }
            }
            else if (questionType == QuestionType.TrueFalse)
            {
                var truePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                truePort.portName = "True";
                truePort.name = truePort.portName;
                outputContainer.Add(truePort);

                var falsePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                falsePort.portName = "False";
                falsePort.name = falsePort.portName;
                outputContainer.Add(falsePort);
            }

            RefreshPorts();
        }

        private void AddFields()
        {
            var container = new VisualElement();

            // Question type dropdown
            var typeField = new EnumField("Question Type", questionType);
            typeField.Init(QuestionType.MultipleChoice);
            typeField.RegisterValueChangedCallback(evt =>
            {
                questionType = (QuestionType)evt.newValue;
                UpdateAnswerFields();
                UpdateOutputPorts();
            });
            container.Add(typeField);

            // Category field
            var categoryField = new TextField("Category:");
            categoryField.RegisterValueChangedCallback(evt => Category = evt.newValue);
            container.Add(categoryField);

            // Difficulty dropdown
            var difficultyField = new EnumField("Difficulty", DifficultyLevel);
            difficultyField.Init(Difficulty.Medium);
            difficultyField.RegisterValueChangedCallback(evt => DifficultyLevel = (Difficulty)evt.newValue);
            container.Add(difficultyField);

            // Time limit field
            var timerField = new FloatField("Time Limit (seconds):");
            timerField.RegisterValueChangedCallback(evt => TimeLimit = evt.newValue);
            container.Add(timerField);

            // Point value field
            var pointsField = new IntegerField("Point Value:");
            pointsField.RegisterValueChangedCallback(evt => PointValue = evt.newValue);
            container.Add(pointsField);

            // Question text
            var questionField = new TextField("Question:");
            questionField.multiline = true;
            questionField.RegisterValueChangedCallback(evt => QuestionText = evt.newValue);
            container.Add(questionField);

            // Explanation field
            var explanationField = new TextField("Explanation:");
            explanationField.multiline = true;
            explanationField.RegisterValueChangedCallback(evt => Explanation = evt.newValue);
            container.Add(explanationField);

            extensionContainer.Add(container);

            // Initialize answer fields container
            answerFieldsContainer = new VisualElement();
            extensionContainer.Add(answerFieldsContainer);

            UpdateAnswerFields();

            RefreshExpandedState();
        }

        private void LoadFields()
        {
            // Load saved data into UI elements
            var container = extensionContainer[0] as VisualElement;

            int index = 0;

            // Question type field
            var typeField = container.ElementAt(index++) as EnumField;
            typeField.SetValueWithoutNotify(questionType);

            // Category field
            var categoryField = container.ElementAt(index++) as TextField;
            categoryField.SetValueWithoutNotify(Category);

            // Difficulty field
            var difficultyField = container.ElementAt(index++) as EnumField;
            difficultyField.SetValueWithoutNotify(DifficultyLevel);

            // Time limit field
            var timerField = container.ElementAt(index++) as FloatField;
            timerField.SetValueWithoutNotify(TimeLimit);

            // Point value field
            var pointsField = container.ElementAt(index++) as IntegerField;
            pointsField.SetValueWithoutNotify(PointValue);

            // Question text field
            var questionField = container.ElementAt(index++) as TextField;
            questionField.SetValueWithoutNotify(QuestionText);

            // Explanation field
            var explanationField = container.ElementAt(index++) as TextField;
            explanationField.SetValueWithoutNotify(Explanation);
        }

        private void UpdateAnswerFields()
        {
            // Clear existing answer fields
            answerFieldsContainer.Clear();

            if (questionType == QuestionType.MultipleChoice)
            {
                // Create multiple-choice answer fields
                if (Answers == null || Answers.Length != 4)
                {
                    Answers = new string[4];
                }
                for (int i = 0; i < Answers.Length; i++)
                {
                    var answerField = new TextField($"Answer {i + 1}:");
                    int index = i;
                    answerField.RegisterValueChangedCallback(evt => Answers[index] = evt.newValue);
                    if (!string.IsNullOrEmpty(Answers[index]))
                    {
                        answerField.SetValueWithoutNotify(Answers[index]);
                    }
                    answerFieldsContainer.Add(answerField);
                }

                // Correct answer index
                var correctIndexField = new IntegerField("Correct Answer Index:");
                correctIndexField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue >= 0 && evt.newValue < Answers.Length)
                        CorrectAnswerIndex = evt.newValue;
                    else
                        Debug.LogWarning("Correct Answer Index is out of range!");
                });
                correctIndexField.SetValueWithoutNotify(CorrectAnswerIndex);
                answerFieldsContainer.Add(correctIndexField);
            }
            else if (questionType == QuestionType.TrueFalse)
            {
                // For true/false questions
                Answers = new[] { "True", "False" };

                // Correct answer toggle
                var correctAnswerToggle = new Toggle("Correct Answer is True");
                correctAnswerToggle.RegisterValueChangedCallback(evt => CorrectAnswerIndex = evt.newValue ? 0 : 1);
                correctAnswerToggle.SetValueWithoutNotify(CorrectAnswerIndex == 0);
                answerFieldsContainer.Add(correctAnswerToggle);
            }

            RefreshExpandedState();
        }

        public QuestionNodeData GetData()
        {
            return new QuestionNodeData
            {
                GUID = GUID,
                QuestionText = QuestionText,
                Answers = Answers,
                CorrectAnswerIndex = CorrectAnswerIndex,
                Category = Category,
                DifficultyLevel = DifficultyLevel,
                TimeLimit = TimeLimit,
                PointValue = PointValue,
                Explanation = Explanation,
                questionType = questionType,
                Position = GetPosition().position
            };
        }
    }
}
