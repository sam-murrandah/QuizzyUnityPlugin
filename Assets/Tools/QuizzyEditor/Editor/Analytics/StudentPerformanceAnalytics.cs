using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace QuizGraphEditor
{
    [CreateAssetMenu(fileName = "StudentPerformanceAnalytics", menuName = "Quiz/Student Performance Analytics")]
    public class StudentPerformanceAnalytics : ScriptableObject
    {
        public List<StudentPerformance> studentPerformances = new List<StudentPerformance>();



        /// <summary>
        /// Records a new quiz attempt for a student.
        /// </summary>
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
                ParsedAttemptDate = DateTime.Now, // Use the helper property
                Score = score,
                TimeTaken = timeTaken
            });
        }


        /// <summary>
        /// Saves the analytics data to a JSON file.
        /// </summary>
        public void SaveAnalytics(string filePath)
        {
            var jsonData = JsonUtility.ToJson(this, true);
            File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Loads analytics data from a JSON file.
        /// </summary>
        public void LoadAnalytics(string filePath)
        {
            if (File.Exists(filePath))
            {
                var jsonData = File.ReadAllText(filePath);
                JsonUtility.FromJsonOverwrite(jsonData, this);
            }
            else
            {
                Debug.LogError($"File not found at path: {filePath}");
            }
        }

        /// <summary>
        /// Exports the analytics data to a CSV file.
        /// </summary>
        public void ExportToCsv(string filePath)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("StudentID,AttemptDate,Score,TimeTaken");

                foreach (var student in studentPerformances)
                {
                    foreach (var attempt in student.Attempts)
                    {
                        sb.AppendLine($"{student.StudentID},{attempt.AttemptDate:yyyy-MM-dd HH:mm:ss},{attempt.Score},{attempt.TimeTaken:F2}");
                    }
                }

                File.WriteAllText(filePath, sb.ToString());
                Debug.Log($"Analytics successfully exported to CSV: {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to export analytics to CSV: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the analytics data to a JSON file.
        /// </summary>
        public void ExportToJson(string filePath)
        {
            try
            {
                var json = JsonUtility.ToJson(this, true);
                File.WriteAllText(filePath, json);
                Debug.Log($"Analytics successfully exported to JSON: {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to export analytics to JSON: {ex.Message}");
            }
        }
    }

    [Serializable]
    public class StudentPerformance
    {
        public string StudentID;
        public List<QuizAttempt> Attempts = new List<QuizAttempt>();
    }

    [Serializable]
    public class QuizAttempt
    {
        public string AttemptDate;
        public int Score;
        public float TimeTaken;

        // Helper property for easier access
        public DateTime ParsedAttemptDate
        {
            get => DateTime.TryParse(AttemptDate, out var parsedDate) ? parsedDate : DateTime.MinValue;
            set => AttemptDate = value.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
