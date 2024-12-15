using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizGraphEditor
{
    public class RuntimeQuizUnitTest : MonoBehaviour
    {
        [Header("Quiz Setup")]
        [Tooltip("Path to the quiz JSON file relative to Application.dataPath")]
        public string quizJsonFilePath = "QuizGraph.json";

        [Header("Analytics")]
        public StudentPerformanceAnalytics analytics;
        [Tooltip("Provide a student ID for test purposes")]
        public string testStudentID = "TestStudent001";

        private QuizData quizData;
        private Dictionary<string, BaseQuestionNodeData> questionNodes;
        private Dictionary<string, List<EdgeData>> adjacencyList;
        private string startNodeGUID;
        private int currentScore = 0;
        private float startTime;

        private void Start()
        {
            LoadQuizData();
            if (quizData == null)
            {
                Debug.LogError("Quiz data not loaded. Cannot run test.");
                return;
            }

            BuildRuntimeStructures();
            RunTest();
        }

        private void LoadQuizData()
        {
            string fullPath = System.IO.Path.Combine(Application.dataPath, quizJsonFilePath);
            if (!System.IO.File.Exists(fullPath))
            {
                Debug.LogError("Quiz JSON file not found at: " + fullPath);
                return;
            }

            string json = System.IO.File.ReadAllText(fullPath);
            quizData = JsonUtility.FromJson<QuizData>(json);

            if (quizData != null)
                Debug.Log("Quiz Data Loaded Successfully.");
        }

        private void BuildRuntimeStructures()
        {
            questionNodes = new Dictionary<string, BaseQuestionNodeData>();
            adjacencyList = new Dictionary<string, List<EdgeData>>();

            if (quizData.startNodeData == null)
            {
                Debug.LogError("No StartNode found in the quiz data.");
                return;
            }
            startNodeGUID = quizData.startNodeData.GUID;

            // Add question nodes
            foreach (var mc in quizData.multipleChoiceNodes)
                questionNodes[mc.GUID] = mc;

            foreach (var tf in quizData.trueFalseNodes)
                questionNodes[tf.GUID] = tf;

            // Build adjacency list
            foreach (var edge in quizData.edges)
            {
                if (!adjacencyList.ContainsKey(edge.outputNodeGUID))
                    adjacencyList[edge.outputNodeGUID] = new List<EdgeData>();
                adjacencyList[edge.outputNodeGUID].Add(edge);
            }

            Debug.Log("Runtime structures built. Questions: " + questionNodes.Count + ", Edges: " + quizData.edges.Count);
        }

        private void RunTest()
        {
            StartQuiz();

            // Move from StartNode to the first question node
            string currentNodeGUID = MoveFromStartNodeToFirstQuestion(startNodeGUID);

            while (!string.IsNullOrEmpty(currentNodeGUID))
            {
                var nodeData = questionNodes[currentNodeGUID];
                PresentQuestion(nodeData);

                // Simulate always picking the first answer (index 0)
                int chosenAnswerIndex = 0;
                bool correct = CheckAnswer(nodeData, chosenAnswerIndex);
                currentScore += correct ? nodeData.PointValue : 0;
                Debug.Log(correct ? "Correct Answer! Score: " + currentScore : "Incorrect Answer! Score: " + currentScore);

                currentNodeGUID = MoveToNextNode(currentNodeGUID, nodeData, chosenAnswerIndex);
            }

            Debug.Log("Quiz completed. Final Score: " + currentScore);
            FinishQuiz();
        }

        private void StartQuiz()
        {
            currentScore = 0;
            startTime = Time.time;
            Debug.Log("Quiz started at runtime test.");
        }

        private string MoveFromStartNodeToFirstQuestion(string startGUID)
        {
            if (!adjacencyList.TryGetValue(startGUID, out var edges) || edges.Count == 0)
            {
                Debug.LogWarning("StartNode has no outgoing edges. Ending quiz.");
                return string.Empty;
            }

            // Assuming the first edge leads to a question node
            string nextGUID = edges[0].inputNodeGUID;
            if (!questionNodes.ContainsKey(nextGUID))
            {
                Debug.LogWarning("The node after StartNode is not a question node. Ending quiz.");
                return string.Empty;
            }

            return nextGUID;
        }

        private void PresentQuestion(BaseQuestionNodeData nodeData)
        {
            Debug.Log("Question: " + nodeData.QuestionText);
            if (nodeData is MultipleChoiceNodeData mc)
            {
                for (int i = 0; i < mc.Answers.Length; i++)
                    Debug.Log($"Answer {i + 1}: {mc.Answers[i]}");
            }
            else if (nodeData is TrueFalseNodeData)
            {
                Debug.Log("1: True");
                Debug.Log("2: False");
            }
        }

        private bool CheckAnswer(BaseQuestionNodeData nodeData, int chosenIndex)
        {
            if (nodeData is MultipleChoiceNodeData mc)
                return chosenIndex == mc.CorrectAnswerIndex;
            if (nodeData is TrueFalseNodeData tf)
                return chosenIndex == tf.CorrectAnswerIndex;
            return false;
        }

        private string MoveToNextNode(string currentGUID, BaseQuestionNodeData nodeData, int chosenIndex)
        {
            if (!adjacencyList.TryGetValue(currentGUID, out var edges))
            {
                Debug.Log("No further edges, ending quiz.");
                return string.Empty;
            }

            string expectedPortName;
            if (nodeData is MultipleChoiceNodeData)
                expectedPortName = $"Answer {chosenIndex + 1}";
            else if (nodeData is TrueFalseNodeData)
                expectedPortName = chosenIndex == 0 ? "True" : "False";
            else
                return edges.Count > 0 ? edges[0].inputNodeGUID : string.Empty;

            foreach (var edge in edges)
            {
                if (edge.outputPortName == expectedPortName)
                    return edge.inputNodeGUID;
            }

            Debug.LogWarning("No matching edge for chosen answer. Ending quiz.");
            return string.Empty;
        }

        private void FinishQuiz()
        {
            float endTime = Time.time;
            float totalTime = endTime - startTime;

            Debug.Log("Unit test: Recording student performance...");
            if (analytics == null)
            {
                Debug.LogWarning("No analytics object assigned. Cannot record attempts.");
                return;
            }

            analytics.RecordAttempt(testStudentID, currentScore, totalTime);
            // Optionally export analytics:
            // analytics.SaveAnalytics(Application.dataPath + "/StudentAnalytics.json");

            Debug.Log("Unit test completed successfully.");
        }
    }
}
