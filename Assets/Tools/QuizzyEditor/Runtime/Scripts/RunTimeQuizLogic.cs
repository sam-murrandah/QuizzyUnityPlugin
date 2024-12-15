using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizGraphEditor
{
    public class RuntimeQuizLogic
    {
        private QuizData quizData;
        private Dictionary<string, BaseQuestionNodeData> questionNodes;
        private Dictionary<string, List<EdgeData>> adjacencyList;

        private string currentNodeGUID;
        private int score;
        private float startTime;

        // Timer fields
        private float currentTimeLimit = 0f;
        private float currentTimeRemaining = 0f;
        private bool timerRunning = false;

        // Store question results for final summary
        private List<ResultData> results = new List<ResultData>();

        // Events
        public event Action<string, string[]> OnQuestionPresented;
        public event Action<int, float> OnQuizFinished;
        public event Action<float> OnTimerUpdated;
        public event Action OnTimeExpired;
        public event Action<List<ResultData>> OnDisplayResults;

        public void LoadQuiz(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError("Quiz JSON file not found at: " + filePath);
                return;
            }

            string jsonData = System.IO.File.ReadAllText(filePath);
            quizData = JsonUtility.FromJson<QuizData>(jsonData);

            // Build runtime data structures
            questionNodes = new Dictionary<string, BaseQuestionNodeData>();
            adjacencyList = new Dictionary<string, List<EdgeData>>();

            foreach (var mcNode in quizData.multipleChoiceNodes)
                questionNodes[mcNode.GUID] = mcNode;

            foreach (var tfNode in quizData.trueFalseNodes)
                questionNodes[tfNode.GUID] = tfNode;

            foreach (var edge in quizData.edges)
            {
                if (!adjacencyList.ContainsKey(edge.outputNodeGUID))
                    adjacencyList[edge.outputNodeGUID] = new List<EdgeData>();

                adjacencyList[edge.outputNodeGUID].Add(edge);
            }

            currentNodeGUID = quizData.startNodeData?.GUID;
            if (string.IsNullOrEmpty(currentNodeGUID))
            {
                Debug.LogError("No StartNode found in the quiz data.");
            }
        }

        public void StartQuiz()
        {
            if (string.IsNullOrEmpty(currentNodeGUID))
            {
                Debug.LogError("Cannot start quiz because StartNode is missing.");
                return;
            }

            score = 0;
            startTime = Time.time;
            results.Clear();

            // Move from StartNode to the first question node
            currentNodeGUID = GetFirstQuestionNodeFromStartNode(currentNodeGUID);

            // If still empty or not found, end the quiz here
            if (string.IsNullOrEmpty(currentNodeGUID))
            {
                Debug.LogError("No question nodes connected to StartNode.");
                EndQuiz();
                return;
            }

            PresentNextQuestion();
        }

        /// <summary>
        /// Given the StartNode's GUID, follow the edges to find the first question node connected.
        /// </summary>
        private string GetFirstQuestionNodeFromStartNode(string startNodeGUID)
        {
            if (string.IsNullOrEmpty(startNodeGUID)) return string.Empty;

            if (!adjacencyList.TryGetValue(startNodeGUID, out var edges) || edges.Count == 0)
            {
                // No edges from start node
                return string.Empty;
            }

            // Assuming the start node leads to exactly one next node (typical scenario)
            // If multiple, you could choose the first or implement logic to pick one.
            var nextNodeGUID = edges[0].inputNodeGUID;

            // If this next node is actually a question node, return it.
            // If it's not found in questionNodes, it may not be a question node.
            if (questionNodes.ContainsKey(nextNodeGUID))
            {
                return nextNodeGUID;
            }
            else
            {
                // If the next node is not a question node, attempt to follow its edges recursively
                // But typically, StartNode should connect directly to a question node.
                // If your graph can have multiple "transition" nodes, you need to handle that here.
                // For simplicity, assume it goes directly to a question node.
                Debug.LogWarning("Next node after StartNode is not a question node. Check your graph.");
                return string.Empty;
            }
        }


        private void PresentNextQuestion()
        {
            if (string.IsNullOrEmpty(currentNodeGUID))
            {
                EndQuiz();
                return;
            }

            if (!questionNodes.TryGetValue(currentNodeGUID, out var currentNode))
            {
                Debug.LogError($"Current node with GUID {currentNodeGUID} not found.");
                EndQuiz();
                return;
            }

            string questionText;
            string[] answers;
            float timeLimit;
            string explanation;
            int correctIndex = -1;

            if (currentNode is MultipleChoiceNodeData mcNode)
            {
                questionText = mcNode.QuestionText;
                answers = mcNode.Answers;
                timeLimit = mcNode.TimeLimit;
                explanation = mcNode.Explanation;
                correctIndex = mcNode.CorrectAnswerIndex;
            }
            else if (currentNode is TrueFalseNodeData tfNode)
            {
                questionText = tfNode.QuestionText;
                answers = tfNode.Answers;
                timeLimit = tfNode.TimeLimit;
                explanation = tfNode.Explanation;
                correctIndex = tfNode.CorrectAnswerIndex;
            }
            else
            {
                // Unknown node type; skip
                Debug.LogWarning("Unknown node type encountered. Skipping...");
                MoveToNextNode(0);
                return;
            }

            // Start the timer if timeLimit > 0
            StartTimer(timeLimit);

            // Present the question
            OnQuestionPresented?.Invoke(questionText, answers);

            // Prepare a result entry for this question
            results.Add(new ResultData
            {
                QuestionText = questionText,
                Answers = answers,
                Explanation = explanation,
                CorrectAnswerIndex = correctIndex,
                ChosenAnswerIndex = -1, // Not chosen yet
                IsCorrect = false
            });
        }

        public void SubmitAnswer(int selectedIndex)
        {
            if (!questionNodes.TryGetValue(currentNodeGUID, out var currentNode))
            {
                Debug.LogError($"Current node with GUID {currentNodeGUID} not found.");
                return;
            }

            StopTimer();

            var lastResult = results[results.Count - 1];
            lastResult.ChosenAnswerIndex = selectedIndex;

            bool isCorrect = false;
            if (currentNode is MultipleChoiceNodeData mcNode)
            {
                isCorrect = selectedIndex == mcNode.CorrectAnswerIndex;
                if (isCorrect) score += mcNode.PointValue;
            }
            else if (currentNode is TrueFalseNodeData tfNode)
            {
                isCorrect = selectedIndex == tfNode.CorrectAnswerIndex;
                if (isCorrect) score += tfNode.PointValue;
            }

            lastResult.IsCorrect = isCorrect;
            results[results.Count - 1] = lastResult;

            Debug.Log(isCorrect ? "Correct answer!" : "Incorrect answer!");

            MoveToNextNode(selectedIndex);
        }

        private void MoveToNextNode(int selectedIndex)
        {
            if (!adjacencyList.TryGetValue(currentNodeGUID, out var edges))
            {
                Debug.Log("No edges found. Ending quiz...");
                EndQuiz();
                return;
            }

            if (selectedIndex >= 0 && selectedIndex < edges.Count)
            {
                currentNodeGUID = edges[selectedIndex].inputNodeGUID;
            }
            else
            {
                Debug.LogError("Selected index is out of bounds for edges. Ending quiz...");
                EndQuiz();
                return;
            }

            PresentNextQuestion();
        }

        private void EndQuiz()
        {
            StopTimer();
            float timeTaken = Time.time - startTime;
            Debug.Log($"Quiz finished. Final score: {score}, Time taken: {timeTaken} seconds");

            OnQuizFinished?.Invoke(score, timeTaken);
            OnDisplayResults?.Invoke(results);
        }

        // Timer logic
        private void StartTimer(float timeLimit)
        {
            if (timeLimit > 0)
            {
                currentTimeLimit = timeLimit;
                currentTimeRemaining = currentTimeLimit;
                timerRunning = true;
            }
            else
            {
                timerRunning = false;
            }
        }

        private void StopTimer()
        {
            timerRunning = false;
        }

        /// <summary>
        /// Call this every frame from a MonoBehaviour (e.g., in QuizRunner's Update).
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!timerRunning) return;

            currentTimeRemaining -= deltaTime;
            if (currentTimeRemaining <= 0)
            {
                currentTimeRemaining = 0;
                timerRunning = false;
                OnTimerUpdated?.Invoke(currentTimeRemaining);
                OnTimeExpired?.Invoke();

                // Handle time expiry scenario
                var lastResult = results[results.Count - 1];
                lastResult.IsCorrect = false;
                lastResult.ChosenAnswerIndex = -1; // No choice
                results[results.Count - 1] = lastResult;

                MoveToNextNode(0);
            }
            else
            {
                OnTimerUpdated?.Invoke(currentTimeRemaining);
            }
        }
    }

    [Serializable]
    public struct ResultData
    {
        public string QuestionText;
        public string[] Answers;
        public string Explanation;
        public int CorrectAnswerIndex;
        public int ChosenAnswerIndex;
        public bool IsCorrect;

        public string CorrectAnswerText =>
            (CorrectAnswerIndex >= 0 && CorrectAnswerIndex < Answers.Length) ? Answers[CorrectAnswerIndex] : "";
        public string ChosenAnswerText =>
            (ChosenAnswerIndex >= 0 && ChosenAnswerIndex < Answers.Length) ? Answers[ChosenAnswerIndex] : "No Answer";
    }
}
