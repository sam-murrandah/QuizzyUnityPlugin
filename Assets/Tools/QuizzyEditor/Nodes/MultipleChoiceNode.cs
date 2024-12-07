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

        public override void ApplyColorPreferences()
        {
            style.backgroundColor = UserPreferences.MultipleChoiceColor;
            titleContainer.style.color = UserPreferences.MultipleChoiceTextColor;
        }


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
                UpdateAnswerFields();
                UpdateOutputPorts();
                ApplyColorPreferences();
            }
        }
    }
}
