using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt
{
    public class SleepAnswersChoiceYesNoOptionsPrompt : AnswersChoiceYesNoOptionsPrompt
    {
        public SleepAnswersChoiceYesNoOptionsPrompt(IStatePropertyAccessor<AnswerState> answersStateAccessor): base(answersStateAccessor)
        {
        }

        protected override string QuestionText { get; } = "Did you sleep well?";
        protected override string MessageVeryGood { get; } = "That's great! Sleep is very important";
        protected override string MessageVeryBad { get; } = "It's not good, maybe later I could try searching for some tips for you";

        protected override void UpdateActivityHandler(ActivityScore score, QuestionsData todaysAnswers)
        {
            todaysAnswers.SleepScore = score;
        }

        protected override List<Choice> QuestionChoices()
        {
            return new List<Choice>
            {
                new Choice("1. No, not at all"){Synonyms = new List<string> {"1", "Nope", "Don't ask", "not good", "not at all"}, Value = "Not at all"},
                new Choice("2. Not too well"){Synonyms = new List<string> {"2", "bad"}, Value = "No"},
                new Choice("3. Neither good nor bad ") {Synonyms = new List<string> { "3", "neutral", "normal", "normally" }, Value = "Neither good nor bad"},
                new Choice("4. Yes, good enough"){Synonyms = new List<string> {"4", "well", "good enough"}, Value = "Yes, good enough"},
                new Choice("5. Yes, very well"){Synonyms = new List<string> {"5", "Yes", "Yea", "Of course", "Sure", "great", "very well"}, Value = "Yes, very well"},
            };
        }
    }
}