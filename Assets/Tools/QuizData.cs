using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizGraphEditor
{
    [Serializable]
    public class QuizData
    {
        public List<QuestionNodeData> questionNodes = new List<QuestionNodeData>();
        public StartNodeData startNodeData;
        public List<EdgeData> edges = new List<EdgeData>();
    }

    [Serializable]
    public class QuestionNodeData
    {
        public string GUID;
        public string QuestionText;
        public string[] Answers;
        public int CorrectAnswerIndex;
        public string Category;
        public Difficulty DifficultyLevel;
        public float TimeLimit;
        public int PointValue;
        public string Explanation;
        public QuestionType questionType;
        public Vector2 Position;
    }

    [Serializable]
    public class StartNodeData
    {
        public string GUID;
        public Vector2 Position;
    }

    [Serializable]
    public class EdgeData
    {
        public string outputNodeGUID;
        public string outputPortName;
        public string inputNodeGUID;
        public string inputPortName;
    }
}
