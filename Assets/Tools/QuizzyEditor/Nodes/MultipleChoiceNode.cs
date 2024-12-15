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

namespace QuizGraphEditor
{
    public class MultipleChoiceNode : BaseQuestionNode
    {
        public string[] Answers { get; private set; }
        public int CorrectAnswerIndex { get; private set; }

        public MultipleChoiceNode() : base() { }
        public MultipleChoiceNode(Vector2 position) : base(position) { }

        /// <summary>
        /// Updates the answer fields for the Multiple Choice node. Creates fields for up to four answers and a field to set the correct answer index.
        /// </summary>
        protected override void UpdateAnswerFields()
        {
            answerFieldsContainer.Clear();

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

            RefreshExpandedState();
        }

        /// <summary>
        /// Adds the main fields (e.g., Question, Category, Difficulty) to the node UI.
        /// </summary>
        protected override void AddFields()
        {
            var container = new VisualElement();
            container.AddToClassList("node-main-container"); // A class for the main content

            questionField = new TextField("Question:");
            questionField.multiline = true;
            questionField.AddToClassList("node-field");
            container.Add(questionField);

            categoryField = new TextField("Category:");
            categoryField.AddToClassList("node-field");
            container.Add(categoryField);

            difficultyField = new EnumField("Difficulty", Difficulty.Medium);
            difficultyField.AddToClassList("node-field");
            container.Add(difficultyField);

            timerField = new FloatField("Time Limit (seconds):");
            timerField.AddToClassList("node-field");
            container.Add(timerField);

            pointsField = new IntegerField("Point Value:");
            pointsField.AddToClassList("node-field");
            container.Add(pointsField);

            explanationField = new TextField("Explanation:");
            explanationField.multiline = true;
            explanationField.AddToClassList("node-field");
            container.Add(explanationField);

            // Optional: add a separator
            var separator = new VisualElement();
            separator.AddToClassList("node-separator");
            container.Add(separator);

            extensionContainer.Add(container);

            // Initialize answer fields container in a separate section
            answerFieldsContainer = new VisualElement();
            answerFieldsContainer.AddToClassList("node-answer-container");
            extensionContainer.Add(answerFieldsContainer);
            
            categoryField.RegisterValueChangedCallback(evt =>
            {
                Category = evt.newValue;
                Debug.Log($"MultipleChoiceNode: Category updated to {Category}");
            });

            difficultyField.RegisterValueChangedCallback(evt =>
            {
                DifficultyLevel = (Difficulty)evt.newValue;
                Debug.Log($"MultipleChoiceNode: DifficultyLevel updated to {DifficultyLevel}");
            });

            timerField.RegisterValueChangedCallback(evt =>
            {
                TimeLimit = evt.newValue;
                Debug.Log($"MultipleChoiceNode: TimeLimit updated to {TimeLimit}");
            });

            pointsField.RegisterValueChangedCallback(evt =>
            {
                PointValue = evt.newValue;
                Debug.Log($"MultipleChoiceNode: PointValue updated to {PointValue}");
            });

            questionField.RegisterValueChangedCallback(evt =>
            {
                QuestionText = evt.newValue;
                Debug.Log($"MultipleChoiceNode: QuestionText updated to {QuestionText}");
            });

            explanationField.RegisterValueChangedCallback(evt =>
            {
                Explanation = evt.newValue;
                Debug.Log($"MultipleChoiceNode: Explanation updated to {Explanation}");
            });

            UpdateAnswerFields();
            UpdateOutputPorts();

            RefreshExpandedState();
        }

        /// <summary>
        /// Updates the output ports for the Multiple Choice node. Creates one output port per answer.
        /// </summary>
        protected override void UpdateOutputPorts()
        {
            outputContainer.Clear();

            // One output port per answer
            for (int i = 0; i < Answers.Length; i++)
            {
                var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                output.portName = $"Answer {i + 1}";
                output.name = output.portName;
                outputContainer.Add(output);
            }

            RefreshPorts();
        }

        /// <summary>
        /// Applies the colour preferences for Multiple Choice nodes based on user settings.
        /// </summary>
        public override void ApplyColorPreferences()
        {
            style.backgroundColor = UserPreferences.MultipleChoiceColor;
        }

        /// <summary>
        /// Retrieves the data from the Multiple Choice node in a serializable format.
        /// </summary>
        public override BaseQuestionNodeData GetData()
        {
            return new MultipleChoiceNodeData
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
                Position = GetPosition().position
            };
        }

        /// <summary>
        /// Loads data into the Multiple Choice node, setting fields and ports based on serialized data.
        /// </summary>
        public override void LoadData(BaseQuestionNodeData baseData)
        {
            if (baseData is MultipleChoiceNodeData data)
            {
                GUID = data.GUID;
                QuestionText = data.QuestionText;
                Answers = data.Answers;
                CorrectAnswerIndex = data.CorrectAnswerIndex;
                Category = data.Category;
                DifficultyLevel = data.DifficultyLevel;
                TimeLimit = data.TimeLimit;
                PointValue = data.PointValue;
                Explanation = data.Explanation;

                SetPosition(new Rect(data.Position, new Vector2(200, 150)));
                AddToClassList("multiple-choice-node");

                // Rebuild fields and ports
                UpdateAnswerFields();
                UpdateOutputPorts();
                ApplyColorPreferences();

                // Now update UI fields from data
                RefreshUIFromData();
            }
        }

    }
}
