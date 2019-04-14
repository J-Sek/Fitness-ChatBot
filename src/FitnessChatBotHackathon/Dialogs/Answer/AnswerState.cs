using System;
using System.Collections.Generic;
using Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt;

namespace Fitness.ChatBot.Dialogs.Answer
{
    public class AnswerState
    {
        public List<QuestionsData> Questions { get; set; } = new List<QuestionsData>();
    }

    public class QuestionsData
    {
        public DateTime Day { get; set; }

        public ActivityScore ActivityScore { get; set; }
        public ActivityScore FoodScore { get; set; }
        public ActivityScore SleepScore { get; set; }
    }

    public enum ActivityScore
    {
        VeryBad = -2,
        Bad = -1,
        Neutral = 0,
        Good = 1,
        VeryGood = 2
    }
}