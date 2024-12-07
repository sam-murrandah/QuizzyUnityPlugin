using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizGraphEditor
{
    [Serializable]
    public class StudentPerformance
    {
        public string StudentID;
        public List<QuizAttempt> Attempts = new List<QuizAttempt>();
    }

    [Serializable]
    public class QuizAttempt
    {
        public DateTime AttemptDate;
        public int Score;
        public float TimeTaken;
        // Additional data as needed
    }

    public class StudentPerformanceAnalytics : ScriptableObject
    {
        public List<StudentPerformance> studentPerformances = new List<StudentPerformance>();

        public void RecordAttempt(string studentID, int score, float timeTaken)
        {
            var student = studentPerformances.Find(s => s.StudentID == studentID);
            if (student == null)
            {
                student = new StudentPerformance { StudentID = studentID };
                studentPerformances.Add(student);
            }

            student.Attempts.Add(new QuizAttempt
            {
                AttemptDate = DateTime.Now,
                Score = score,
                TimeTaken = timeTaken
            });
        }

        public void SaveAnalytics(string filePath)
        {
            var jsonData = JsonUtility.ToJson(this, true);
            System.IO.File.WriteAllText(filePath, jsonData);
        }

        public void LoadAnalytics(string filePath)
        {
            var jsonData = System.IO.File.ReadAllText(filePath);
            JsonUtility.FromJsonOverwrite(jsonData, this);
        }
    }
}
