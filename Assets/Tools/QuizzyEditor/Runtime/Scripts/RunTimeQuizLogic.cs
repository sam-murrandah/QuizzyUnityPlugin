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

        // Events to hook up to UI or other logic
        public event Action<string, string[]> OnQuestionPresented; // Question text, answers
        public event Action<int, float> OnQuizFinished; // Final score, time taken

        public void LoadQuiz(string filePath)
        {
            // Load the JSON quiz data
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

            // Map all nodes
            foreach (var mcNode in quizData.multipleChoiceNodes)
                questionNodes[mcNode.GUID] = mcNode;

            foreach (var tfNode in quizData.trueFalseNodes)
                questionNodes[tfNode.GUID] = tfNode;

            // Map all edges
            foreach (var edge in quizData.edges)
            {
                if (!adjacencyList.ContainsKey(edge.outputNodeGUID))
                    adjacencyList[edge.outputNodeGUID] = new List<EdgeData>();

                adjacencyList[edge.outputNodeGUID].Add(edge);
            }

            // Set the StartNode as the initial node
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

            PresentNextQuestion();
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

            if (currentNode is MultipleChoiceNodeData mcNode)
            {
                OnQuestionPresented?.Invoke(mcNode.QuestionText, mcNode.Answers);
            }
            else if (currentNode is TrueFalseNodeData tfNode)
            {
                OnQuestionPresented?.Invoke(tfNode.QuestionText, tfNode.Answers);
            }
            else
            {
                Debug.LogWarning("Unknown node type encountered. Skipping...");
                MoveToNextNode(0); // Default to first edge
            }
        }

        public void SubmitAnswer(int selectedIndex)
        {
            if (!questionNodes.TryGetValue(currentNodeGUID, out var currentNode))
            {
                Debug.LogError($"Current node with GUID {currentNodeGUID} not found.");
                return;
            }

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

            Debug.Log(isCorrect ? "Correct answer!" : "Incorrect answer!");

            // Move to the next node
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

            // For MultipleChoice: selectedIndex corresponds to the output edge port
            // For TrueFalse: 0 = True port, 1 = False port
            if (selectedIndex >= 0 && selectedIndex < edges.Count)
            {
                currentNodeGUID = edges[selectedIndex].inputNodeGUID;
            }
            else
            {
                Debug.LogError("Selected index is out of bounds for edges. Ending quiz...");
                EndQuiz();
            }

            PresentNextQuestion();
        }

        private void EndQuiz()
        {
            float timeTaken = Time.time - startTime;
            Debug.Log($"Quiz finished. Final score: {score}, Time taken: {timeTaken} seconds");
            OnQuizFinished?.Invoke(score, timeTaken);
        }
    }
}
