using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt
{
    public class ActivityAnswersChoiceYesNoOptionsPrompt : AnswersChoiceYesNoOptionsPrompt
    {
        public ActivityAnswersChoiceYesNoOptionsPrompt(IStatePropertyAccessor<AnswerState> answersStateAccessor) : base(answersStateAccessor)
        {
        }

        protected override string QuestionText { get; } = "Have you been on Gym today?";

        protected override void UpdateActivityHandler(ActivityScore score, QuestionsData todaysAnswers)
        {
            todaysAnswers.ActivityScore = score;
        }
        
        
        protected override List<Choice> QuestionChoices()
        {
            return new List<Choice>
            {
                new Choice(){Synonyms = new List<string> {"1", "yes", "yea", "y", "I was", "I have been"}, Value = "Yes"},
                new Choice(){Synonyms = new List<string> {"2", "Nope", "n", "Don't ask", "not"}, Value = "No"},
                new Choice(){Synonyms = new List<string> {"3", "no time"}, Value = "Sorry, I had no time"}
            };
        }

        
        protected override ActivityScore AnswerIdToScore(int answerId)
        {
            switch (answerId)
            {
                case 0:
                    return ActivityScore.VeryGood;
                case 2:
                    return ActivityScore.Bad;
                case 1:
                default:
                    return ActivityScore.VeryBad;
            }
        }

        
        //protected override List<Choice> QuestionChoices()
        //{
        //    return new List<Choice>
        //    {
        //        new Choice("1. No"){Synonyms = new List<string> {"1", "Nope", "Don't ask", "not good"}, Value = "No, I'm too lazy"},
        //        new Choice("2. Not too well"){Synonyms = new List<string> {"2", "bad"}, Value = "No, I was too tired"},
        //        new Choice("3. Neither good nor bad ") {Synonyms = new List<string> { "3", "neutral", "normal", "normally" }, Value = "I don't fell well, need a short break"},
        //        new Choice("4. Yes, good enough"){Synonyms = new List<string> {"4", "well", "good enough"}, Value = "Yes, but I didn't do all excercises"},
        //        new Choice("5. Yes - very healthy"){Synonyms = new List<string> {"5", "Yes", "Yea", "Of course", "Sure"}, Value = "Yes and I did whole training!"},
        //    };
        //}
    }
}