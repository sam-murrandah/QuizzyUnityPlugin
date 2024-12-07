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

        public override void ApplyColorPreferences()
        {
            style.backgroundColor = UserPreferences.TrueFalseColor;
            //style.color = UserPreferences.TrueFalseTextColor;
        }


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
                UpdateAnswerFields();
                UpdateOutputPorts();
                ApplyColorPreferences();
            }
        }
    }
}
