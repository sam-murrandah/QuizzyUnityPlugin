using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuizGraphEditor
{
    public class UnitTest_Runtime : MonoBehaviour
    {
        [Header("Quiz Setup")]
        [Tooltip("Path to the quiz JSON file relative to Application.dataPath or use a Resources folder")]
        public string quizJsonFilePath = "QuizGraph.json";

        [Header("Analytics")]
        public StudentPerformanceAnalytics analytics;
        [Tooltip("Provide a student ID for demo purposes")]
        public string testStudentID = "TestStudent001";

        private QuizData quizData;
        private Dictionary<string, BaseQuestionNodeData> questionNodes;
        private Dictionary<string, StartNodeData> startNodeDict;
        private Dictionary<string, List<EdgeData>> adjacencyList;

        private int currentScore = 0;
        private float startTime;

        private void Start()
        {
            // For demo, load JSON file from a known path (e.g. Application.dataPath + "/QuizGraph.json")
            // Adjust this as needed. If in Editor, you can use a Resources folder and use Resources.Load.
            LoadQuizData();

            BuildRuntimeStructures();

            // Start Quiz
            StartQuiz();

            // Simulate going through quiz
            RunQuizSimulation();

            // When quiz completes, record analytics
            FinishQuiz();
        }

        private void LoadQuizData()
        {
            // For simplicity, let's assume the file is in the StreamingAssets folder or Application.dataPath.
            // Replace this with your desired loading strategy:
            string fullPath = System.IO.Path.Combine(Application.dataPath, quizJsonFilePath);
            if (!System.IO.File.Exists(fullPath))
            {
                Debug.LogError("Quiz JSON file not found at: " + fullPath);
                return;
            }

            string json = System.IO.File.ReadAllText(fullPath);
            quizData = JsonUtility.FromJson<QuizData>(json);
            Debug.Log("Quiz Data Loaded: " + (quizData != null));

        }

        private void BuildRuntimeStructures()
        {
            questionNodes = new Dictionary<string, BaseQuestionNodeData>();
            startNodeDict = new Dictionary<string, StartNodeData>();
            adjacencyList = new Dictionary<string, List<EdgeData>>();

            // Add start node to dictionary (there should be only one)
            if (quizData.startNodeData != null)
            {
                startNodeDict[quizData.startNodeData.GUID] = quizData.startNodeData;
            }

            // Add multiple choice nodes
            foreach (var mc in quizData.multipleChoiceNodes)
            {
                questionNodes[mc.GUID] = mc;
            }

            // Add true/false nodes
            foreach (var tf in quizData.trueFalseNodes)
            {
                questionNodes[tf.GUID] = tf;
            }

            // Build adjacency list for edges
            foreach (var edge in quizData.edges)
            {
                if (string.IsNullOrEmpty(edge.outputNodeGUID))
                {
                    Debug.LogError("Edge has a null or empty outputNodeGUID!");
                    continue;
                }
                if (!adjacencyList.ContainsKey(edge.outputNodeGUID))
                {
                    adjacencyList[edge.outputNodeGUID] = new List<EdgeData>();
                }
                adjacencyList[edge.outputNodeGUID].Add(edge);
            }

        }

        private void StartQuiz()
        {
            currentScore = 0;
            startTime = Time.time;
            Debug.Log("Quiz started!");
        }

        private void RunQuizSimulation()
        {
            // Find start node (assuming just one)
            if (quizData.startNodeData == null)
            {
                Debug.LogError("No start node found!");
                return;
            }

            string currentNodeGUID = quizData.startNodeData.GUID;

            // Follow from start node’s output edges to find first question node
            currentNodeGUID = MoveToNextNodeFrom(currentNodeGUID);

            // Run until no next node (no edges) or no node found
            while (!string.IsNullOrEmpty(currentNodeGUID))
            {
                // Present question
                var questionData = questionNodes[currentNodeGUID];
                PresentQuestion(questionData);

                // Simulate user selecting an answer:
                int chosenAnswerIndex = SimulateUserAnswer(questionData);

                // Check correctness and update score
                bool correct = CheckAnswer(questionData, chosenAnswerIndex);
                if (correct)
                {
                    currentScore += questionData.PointValue;
                    Debug.Log("Correct! Current Score: " + currentScore);
                }
                else
                {
                    Debug.Log("Incorrect! Current Score: " + currentScore);
                }

                // Move to next node based on chosen answer
                currentNodeGUID = MoveToNextNodeFrom(currentNodeGUID, chosenAnswerIndex);
            }

            Debug.Log("Quiz finished! Final Score: " + currentScore);
        }

        private string MoveToNextNodeFrom(string currentGUID, int chosenAnswerIndex = 0)
        {
            // If no edges from this node, we’re at the end
            if (!adjacencyList.ContainsKey(currentGUID))
            {
                return string.Empty;
            }

            var edgesFromThisNode = adjacencyList[currentGUID];

            // For multiple-choice: the chosen answer’s index corresponds to a specific edge port
            // For true/false: chosen answer 0 = True port, 1 = False port

            // We assume the edges are labelled or known to correspond to the answer order. 
            // For MC nodes, edges might be "Answer 1", "Answer 2", etc.  
            // For TF nodes, edges might be "True" and "False".

            // Find the edge that corresponds to the chosen answer. This logic depends on how 
            // ports are named. For MC: "Answer 1", "Answer 2", ... 
            // For TF: "True" (chosenAnswerIndex=0), "False" (chosenAnswerIndex=1).

            foreach (var edge in edgesFromThisNode)
            {
                if (IsPortMatchingAnswer(edge.outputPortName, edge.inputPortName, questionNodes.ContainsKey(currentGUID) ? questionNodes[currentGUID] : null, chosenAnswerIndex))
                {
                    // Return the GUID of the input node we connect to next
                    return edge.inputNodeGUID;
                }
            }

            // If we can't find a matching edge, return empty string to end the quiz
            return string.Empty;
        }

        private bool IsPortMatchingAnswer(string outputPortName, string inputPortName, BaseQuestionNodeData nodeData, int chosenAnswerIndex)
        {
            // Simplified logic:
            // For MC nodes: ports are "Answer 1", "Answer 2", "Answer 3", "Answer 4"
            // chosenAnswerIndex = 0 should match "Answer 1", and so forth.
            // For TF nodes: chosenAnswerIndex=0 -> "True", =1 -> "False".

            // We only need to check the outputPortName of the current node’s edges, 
            // because that’s what determines which answer leads where.

            string expectedPortName;

            // If this is a MC node, answers might be "Answer X"
            // If TF node, answers might be "True"/"False"
            if (nodeData is MultipleChoiceNodeData)
            {
                expectedPortName = $"Answer {chosenAnswerIndex + 1}";
            }
            else if (nodeData is TrueFalseNodeData)
            {
                expectedPortName = chosenAnswerIndex == 0 ? "True" : "False";
            }
            else
            {
                // If it's StartNode or something else (no question), just pick the first edge
                // This handles the jump from StartNode to first question.
                return true;
            }

            return outputPortName == expectedPortName;
        }

        private void PresentQuestion(BaseQuestionNodeData nodeData)
        {
            Debug.Log("Question: " + nodeData.QuestionText);
            if (nodeData is MultipleChoiceNodeData mc)
            {
                for (int i = 0; i < mc.Answers.Length; i++)
                {
                    Debug.Log((i + 1) + ": " + mc.Answers[i]);
                }
            }
            else if (nodeData is TrueFalseNodeData tf)
            {
                Debug.Log("1: True");
                Debug.Log("2: False");
            }
            else
            {
                Debug.LogWarning("Encountered a non-question node in PresentQuestion");
            }
        }

        private int SimulateUserAnswer(BaseQuestionNodeData nodeData)
        {
            // In a real scenario, you’d wait for player input via UI.
            // For demo, just pick the first answer always.
            // For True/False, that means always pick True (index 0).
            // For MultipleChoice, always pick answer 0 as well.
            return 0;
        }

        private bool CheckAnswer(BaseQuestionNodeData nodeData, int chosenIndex)
        {
            if (nodeData is MultipleChoiceNodeData mc)
            {
                return chosenIndex == mc.CorrectAnswerIndex;
            }
            else if (nodeData is TrueFalseNodeData tf)
            {
                return chosenIndex == tf.CorrectAnswerIndex;
            }

            // If it's not a question node, there's no correct/incorrect concept.
            return false;
        }

        private void FinishQuiz()
        {
            float endTime = Time.time;
            float totalTime = endTime - startTime;

            Debug.Log("Recording student performance...");

            if (analytics == null)
            {
                Debug.LogWarning("No analytics object assigned. Create and assign a StudentPerformanceAnalytics ScriptableObject to track performance.");
                return;
            }

            analytics.RecordAttempt(testStudentID, currentScore, totalTime);
            // Optionally save analytics:
            // analytics.SaveAnalytics(Application.dataPath + "/StudentAnalytics.json");
        }
    }
}
