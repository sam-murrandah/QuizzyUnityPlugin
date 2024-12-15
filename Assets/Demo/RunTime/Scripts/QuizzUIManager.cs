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
using UnityEngine.UI;
using TMPro;

namespace QuizGraphEditor
{
    public class QuizUIManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Main Quiz UI")]
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private RectTransform answersContainer;
        [SerializeField] private GameObject answerButtonPrefab;
        [SerializeField] private TMP_Text timerText;

        [Header("Results UI")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private RectTransform resultsContainer;
        [SerializeField] private GameObject resultEntryPrefab;
        #endregion

        #region Events
        public event Action<int> OnAnswerSelected;
        #endregion

        #region Private Fields
        private readonly List<GameObject> currentAnswerButtons = new List<GameObject>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Validate references
            if (questionText == null) Debug.LogWarning("QuizUIManager: questionText is not assigned.");
            if (answersContainer == null) Debug.LogWarning("QuizUIManager: answersContainer is not assigned.");
            if (answerButtonPrefab == null) Debug.LogWarning("QuizUIManager: answerButtonPrefab is not assigned.");
            if (timerText == null) Debug.LogWarning("QuizUIManager: timerText is not assigned.");
            if (resultsPanel == null) Debug.LogWarning("QuizUIManager: resultsPanel is not assigned.");
            if (resultsContainer == null) Debug.LogWarning("QuizUIManager: resultsContainer is not assigned.");
            if (resultEntryPrefab == null) Debug.LogWarning("QuizUIManager: resultEntryPrefab is not assigned.");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Displays a question and its possible answers.
        /// </summary>
        public void ShowQuestion(string question, string[] answers)
        {
            if (resultsPanel != null) resultsPanel.SetActive(false);
            ClearAnswers();

            if (questionText != null)
                questionText.text = question;

            // Create a button for each answer
            for (int i = 0; i < answers.Length; i++)
            {
                if (answerButtonPrefab == null || answersContainer == null) break;

                var btnObj = Instantiate(answerButtonPrefab, answersContainer);
                var btnText = btnObj.GetComponentInChildren<TMP_Text>();

                if (btnText != null)
                    btnText.text = answers[i];

                int index = i;
                var btn = btnObj.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.AddListener(() => OnAnswerClicked(index));

                currentAnswerButtons.Add(btnObj);
            }

            if (timerText != null)
                timerText.gameObject.SetActive(true); // Show timer if available
        }

        /// <summary>
        /// Updates the displayed timer text.
        /// </summary>
        public void UpdateTimer(float timeRemaining)
        {
            if (timerText != null)
            {
                timerText.text = $"Time: {timeRemaining:F1}s";
            }
        }

        /// <summary>
        /// Hides the timer UI element.
        /// </summary>
        public void HideTimer()
        {
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Displays the results screen with a list of all answered questions.
        /// </summary>
        public void ShowResults(List<ResultData> results)
        {
            if (resultsContainer == null || resultsPanel == null || resultEntryPrefab == null) return;

            // Clear any previous result entries
            foreach (Transform child in resultsContainer)
            {
                Destroy(child.gameObject);
            }

            resultsPanel.SetActive(true);

            // Create an entry for each result
            foreach (var r in results)
            {
                var entry = Instantiate(resultEntryPrefab, resultsContainer);
                var texts = entry.GetComponentsInChildren<TMP_Text>();

                // Expecting your resultEntryPrefab has something like:
                // texts[0] = Question text
                // texts[1] = Your Answer
                // texts[2] = Correct Answer
                // texts[3] = Explanation

                if (texts.Length >= 4)
                {
                    texts[0].text = "Question: " + r.QuestionText;
                    texts[1].text = "Your Answer: " + r.ChosenAnswerText;
                    texts[2].text = "Correct Answer: " + r.CorrectAnswerText;
                    texts[3].text = "Explanation: " + r.Explanation;
                }
            }

            // Optional: You can add a "Play Again" or "Close" button on the resultsPanel 
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Called when an answer button is clicked.
        /// </summary>
        private void OnAnswerClicked(int index)
        {
            OnAnswerSelected?.Invoke(index);
        }

        /// <summary>
        /// Clears existing answer buttons from the UI.
        /// </summary>
        private void ClearAnswers()
        {
            foreach (var btn in currentAnswerButtons)
            {
                if (btn != null) Destroy(btn);
            }
            currentAnswerButtons.Clear();
        }
        #endregion
    }
}
