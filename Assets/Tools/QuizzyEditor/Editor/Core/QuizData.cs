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
    public class EdgeData
    {
        public string outputNodeGUID;
        public string outputPortName;
        public string inputNodeGUID;
        public string inputPortName;
    }

    [Serializable]
    public class QuizData
    {
        public List<MultipleChoiceNodeData> multipleChoiceNodes = new List<MultipleChoiceNodeData>();
        public List<TrueFalseNodeData> trueFalseNodes = new List<TrueFalseNodeData>();
        public StartNodeData startNodeData;
        public List<EdgeData> edges = new List<EdgeData>();
    }

    [Serializable]
    public class BaseQuestionNodeData
    {
        public string GUID;
        public string QuestionText;
        public string Category;
        public Difficulty DifficultyLevel;
        public float TimeLimit;
        public int PointValue;
        public string Explanation;
        public Vector2 Position;
    }

    [Serializable]
    public class StartNodeData
    {
        public string GUID;
        public Vector2 Position;
    }

    [Serializable]
    public class MultipleChoiceNodeData : BaseQuestionNodeData
    {
        public string[] Answers;
        public int CorrectAnswerIndex;
    }

    [Serializable]
    public class TrueFalseNodeData : BaseQuestionNodeData
    {
        public string[] Answers;
        public int CorrectAnswerIndex;
    }
}
