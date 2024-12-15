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
using System.Collections.Generic;
using UnityEngine;

namespace QuizGraphEditor
{
    public class RuntimeQuizLogic
    {
        #region Fields
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
        #endregion

        #region Events
        public event Action<string, string[]> OnQuestionPresented;
        public event Action<int, float> OnQuizFinished;
        public event Action<float> OnTimerUpdated;
        public event Action OnTimeExpired;
        public event Action<List<ResultData>> OnDisplayResults;
        #endregion

        #region Quiz Setup
        /// <summary>
        /// Loads the quiz data from a JSON file and sets up the runtime data structures.
        /// </summary>
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

        /// <summary>
        /// Starts the quiz by initializing values and presenting the first question.
        /// </summary>
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
            var nextNodeGUID = edges[0].inputNodeGUID;

            if (questionNodes.ContainsKey(nextNodeGUID))
            {
                return nextNodeGUID;
            }
            else
            {
                Debug.LogWarning("Next node after StartNode is not a question node. Check your graph.");
                return string.Empty;
            }
        }
        #endregion

        #region Quiz Logic
        /// <summary>
        /// Presents the next question in the quiz and starts the timer if applicable.
        /// </summary>
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
                Debug.LogWarning("Unknown node type encountered. Skipping...");
                MoveToNextNode(0);
                return;
            }

            StartTimer(timeLimit);

            OnQuestionPresented?.Invoke(questionText, answers);

            results.Add(new ResultData
            {
                QuestionText = questionText,
                Answers = answers,
                Explanation = explanation,
                CorrectAnswerIndex = correctIndex,
                ChosenAnswerIndex = -1,
                IsCorrect = false
            });
        }

        /// <summary>
        /// Processes the player's selected answer and updates the quiz state.
        /// </summary>
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

        /// <summary>
        /// Moves to the next node in the quiz based on the player's answer.
        /// </summary>
        private void MoveToNextNode(int selectedIndex)
        {
            if (!adjacencyList.TryGetValue(currentNodeGUID, out var edges))
            {
                Debug.Log("No edges found. Ending quiz...");
                EndQuiz();
                return;
            }

            string expectedPortName;

            if (questionNodes.TryGetValue(currentNodeGUID, out var currentNode))
            {
                if (currentNode is MultipleChoiceNodeData)
                {
                    expectedPortName = $"Answer {selectedIndex + 1}";
                }
                else if (currentNode is TrueFalseNodeData)
                {
                    expectedPortName = (selectedIndex == 0) ? "True" : "False";
                }
                else
                {
                    if (edges.Count > 0)
                    {
                        currentNodeGUID = edges[0].inputNodeGUID;
                        PresentNextQuestion();
                    }
                    else
                    {
                        EndQuiz();
                    }
                    return;
                }

                var chosenEdge = edges.Find(e => e.outputPortName == expectedPortName);

                if (chosenEdge == null)
                {
                    Debug.LogError($"No edge found for port '{expectedPortName}' on node {currentNodeGUID}. Ending quiz...");
                    EndQuiz();
                }
                else
                {
                    currentNodeGUID = chosenEdge.inputNodeGUID;
                    PresentNextQuestion();
                }
            }
            else
            {
                Debug.LogError("Current node not found in questionNodes dictionary.");
                EndQuiz();
            }
        }

        /// <summary>
        /// Ends the quiz and triggers events for displaying the final score and results.
        /// </summary>
        private void EndQuiz()
        {
            StopTimer();
            float timeTaken = Time.time - startTime;
            Debug.Log($"Quiz finished. Final score: {score}, Time taken: {timeTaken} seconds");

            OnQuizFinished?.Invoke(score, timeTaken);
            OnDisplayResults?.Invoke(results);
        }
        #endregion

        #region Timer Logic
        /// <summary>
        /// Starts a timer for the current question if a time limit is provided.
        /// </summary>
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

        /// <summary>
        /// Stops the current timer.
        /// </summary>
        private void StopTimer()
        {
            timerRunning = false;
        }

        /// <summary>
        /// Updates the timer each frame and handles time expiration scenarios.
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

                var lastResult = results[results.Count - 1];
                lastResult.IsCorrect = false;
                lastResult.ChosenAnswerIndex = -1;
                results[results.Count - 1] = lastResult;

                MoveToNextNode(0);
            }
            else
            {
                OnTimerUpdated?.Invoke(currentTimeRemaining);
            }
        }
        #endregion
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

        /// <summary>
        /// Gets the correct answer text.
        /// </summary>
        public string CorrectAnswerText =>
            (CorrectAnswerIndex >= 0 && CorrectAnswerIndex < Answers.Length) ? Answers[CorrectAnswerIndex] : "";

        /// <summary>
        /// Gets the chosen answer text.
        /// </summary>
        public string ChosenAnswerText =>
            (ChosenAnswerIndex >= 0 && ChosenAnswerIndex < Answers.Length) ? Answers[ChosenAnswerIndex] : "No Answer";
    }
}
