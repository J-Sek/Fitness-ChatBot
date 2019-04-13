using System;
using System.Collections.Generic;

namespace Fitness.ChatBot.Dialogs.Answer
{
    public class AnswerState
    {
        public List<QuestionsData> Questions { get; set; } = new List<QuestionsData>();
    }
    
    public class QuestionsData
    {
        public DateTime Day { get; set; }
        public bool Activity { get; set; }
        public bool Food { get; set; }
        public bool Sleep { get; set; }
    }
}