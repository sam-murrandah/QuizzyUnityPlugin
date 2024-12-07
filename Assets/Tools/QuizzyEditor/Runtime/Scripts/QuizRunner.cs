using QuizGraphEditor;
using UnityEngine;

public class QuizRunner : MonoBehaviour
{
    [SerializeField] private QuizUIManager uiManager;
    [SerializeField] private string quizJsonFilePath;
    [SerializeField] private StudentPerformanceAnalytics analytics;
    [SerializeField] private string studentID = "DefaultStudent";

    private RuntimeQuizLogic quizLogic;

    private void Start()
    {
        // Load quiz data, set up logic
        quizLogic = new RuntimeQuizLogic();
        quizLogic.LoadQuiz(quizJsonFilePath);

        // Hook up events
        quizLogic.OnQuestionPresented += (question, answers) =>
        {
            uiManager.ShowQuestion(question, answers);
        };
        uiManager.OnAnswerSelected += (answerIndex) => quizLogic.SubmitAnswer(answerIndex);

        quizLogic.OnQuizFinished += (score, timeTaken) =>
        {
            analytics.RecordAttempt(studentID, score, timeTaken);
            Debug.Log("Quiz finished. Score: " + score);
        };

        quizLogic.StartQuiz();
    }
}
