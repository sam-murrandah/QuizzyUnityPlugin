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
    public class TrueFalseNode : BaseQuestionNode
    {
        public string[] Answers { get; private set; } = { "True", "False" };
        public int CorrectAnswerIndex { get; private set; }

        public TrueFalseNode() : base() { }
        public TrueFalseNode(Vector2 position) : base(position) { }

        /// <summary>
        /// Updates the answer fields for True/False nodes. Creates a toggle to select the correct answer.
        /// </summary>
        protected override void UpdateAnswerFields()
        {
            answerFieldsContainer.Clear();

            // For true/false, just a toggle
            var correctAnswerToggle = new Toggle("Correct Answer is True");
            correctAnswerToggle.RegisterValueChangedCallback(evt => CorrectAnswerIndex = evt.newValue ? 0 : 1);
            correctAnswerToggle.SetValueWithoutNotify(CorrectAnswerIndex == 0);
            answerFieldsContainer.Add(correctAnswerToggle);

            RefreshExpandedState();
        }

        /// <summary>
        /// Updates the output ports for True/False nodes. Creates ports for "True" and "False" answers.
        /// </summary>
        protected override void UpdateOutputPorts()
        {
            outputContainer.Clear();

            // Two ports: True and False
            var truePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            truePort.portName = "True";
            truePort.name = "True";
            outputContainer.Add(truePort);

            var falsePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            falsePort.portName = "False";
            falsePort.name = "False";
            outputContainer.Add(falsePort);

            RefreshPorts();
        }

        /// <summary>
        /// Applies the colour preferences for True/False nodes based on user settings.
        /// </summary>
        public override void ApplyColorPreferences()
        {
            style.backgroundColor = UserPreferences.TrueFalseColor;
        }

        /// <summary>
        /// Adds the main fields (e.g., Question, Category, Difficulty) to the node UI.
        /// </summary>
        protected override void AddFields()
        {
            var container = new VisualElement();
            // Question text
            questionField = new TextField("Question:")
            {
                multiline = true
            };
            questionField.RegisterValueChangedCallback(evt =>
            {
                QuestionText = evt.newValue;
                Debug.Log($"TrueFalseNode: QuestionText updated to {QuestionText}");
            });
            container.Add(questionField);
            // Category field
            categoryField = new TextField("Category:");
            categoryField.RegisterValueChangedCallback(evt =>
            {
                Category = evt.newValue;
                Debug.Log($"TrueFalseNode: Category updated to {Category}");
            });
            container.Add(categoryField);

            // Difficulty dropdown
            difficultyField = new EnumField("Difficulty", Difficulty.Medium);
            difficultyField.RegisterValueChangedCallback(evt =>
            {
                DifficultyLevel = (Difficulty)evt.newValue;
                Debug.Log($"TrueFalseNode: DifficultyLevel updated to {DifficultyLevel}");
            });
            container.Add(difficultyField);

            // Time limit field
            timerField = new FloatField("Time Limit (seconds):");
            timerField.RegisterValueChangedCallback(evt =>
            {
                TimeLimit = evt.newValue;
                Debug.Log($"TrueFalseNode: TimeLimit updated to {TimeLimit}");
            });
            container.Add(timerField);

            // Point value field
            pointsField = new IntegerField("Point Value:");
            pointsField.RegisterValueChangedCallback(evt =>
            {
                PointValue = evt.newValue;
                Debug.Log($"TrueFalseNode: PointValue updated to {PointValue}");
            });
            container.Add(pointsField);

            

            // Explanation field
            explanationField = new TextField("Explanation:")
            {
                multiline = true
            };
            explanationField.RegisterValueChangedCallback(evt =>
            {
                Explanation = evt.newValue;
                Debug.Log($"TrueFalseNode: Explanation updated to {Explanation}");
            });
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
        /// Retrieves the data from the True/False node in a serializable format.
        /// </summary>
        public override BaseQuestionNodeData GetData()
        {
            return new TrueFalseNodeData
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
        /// Loads data into the True/False node, setting fields and ports based on serialized data.
        /// </summary>
        public override void LoadData(BaseQuestionNodeData baseData)
        {
            if (baseData is TrueFalseNodeData data)
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
                AddToClassList("truefalse-node");

                UpdateAnswerFields();
                UpdateOutputPorts();
                ApplyColorPreferences();

                RefreshUIFromData();
            }
        }

    }
}
