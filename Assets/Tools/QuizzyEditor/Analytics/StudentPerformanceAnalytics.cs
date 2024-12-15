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

    [CreateAssetMenu(fileName = "StudentPerformanceAnalytics", menuName = "Quiz/Student Performance Analytics")]
    public class StudentPerformanceAnalytics : ScriptableObject
    {
        public List<StudentPerformance> studentPerformances = new List<StudentPerformance>();

        /// <summary>
        /// Records a new quiz attempt for a student.
        /// </summary>
        /// <param name="studentID">The unique ID of the student.</param>
        /// <param name="score">The score achieved in the quiz.</param>
        /// <param name="timeTaken">The time taken to complete the quiz.</param>
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

        /// <summary>
        /// Saves the analytics data to a JSON file.
        /// </summary>
        /// <param name="filePath">The file path to save the data.</param>
        public void SaveAnalytics(string filePath)
        {
            var jsonData = JsonUtility.ToJson(this, true);
            System.IO.File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Loads analytics data from a JSON file.
        /// </summary>
        /// <param name="filePath">The file path to load the data from.</param>
        public void LoadAnalytics(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                var jsonData = System.IO.File.ReadAllText(filePath);
                JsonUtility.FromJsonOverwrite(jsonData, this);
            }
            else
            {
                Debug.LogError($"File not found at path: {filePath}");
            }
        }
    }
}
