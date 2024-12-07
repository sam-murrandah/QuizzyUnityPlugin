using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro

namespace QuizGraphEditor
{
    public class QuizUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private RectTransform answersContainer;
        [SerializeField] private GameObject answerButtonPrefab;

        // Event fired when an answer is chosen
        public event Action<int> OnAnswerSelected;

        private List<GameObject> currentAnswerButtons = new List<GameObject>();

        public void ShowQuestion(string question, string[] answers)
        {
            ClearAnswers();

            questionText.text = question;

            // Create a button for each answer
            for (int i = 0; i < answers.Length; i++)
            {
                var btnObj = Instantiate(answerButtonPrefab, answersContainer);
                var btnText = btnObj.GetComponentInChildren<TMP_Text>();
                btnText.text = answers[i];

                int index = i; // Capture index for callback
                btnObj.GetComponent<Button>().onClick.AddListener(() => OnAnswerClicked(index));

                currentAnswerButtons.Add(btnObj);
            }
        }

        private void OnAnswerClicked(int index)
        {
            OnAnswerSelected?.Invoke(index);
        }

        private void ClearAnswers()
        {
            foreach (var btn in currentAnswerButtons)
            {
                Destroy(btn);
            }
            currentAnswerButtons.Clear();
        }
    }
}
