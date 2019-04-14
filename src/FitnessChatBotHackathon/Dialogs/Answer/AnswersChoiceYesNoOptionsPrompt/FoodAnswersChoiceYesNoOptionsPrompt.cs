using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt
{
    public class FoodAnswersChoiceYesNoOptionsPrompt : AnswersChoiceYesNoOptionsPrompt
    {
        public FoodAnswersChoiceYesNoOptionsPrompt(IStatePropertyAccessor<AnswerState> answersStateAccessor): base(answersStateAccessor)
        {
        }

        protected override string QuestionText { get; } = "What about food? Tell me how healthy have you eaten?";

        protected override void UpdateActivityHandler(ActivityScore score, QuestionsData todaysAnswers)
        {
            todaysAnswers.FoodScore = score;
        }
        
        protected override List<Choice> QuestionChoices()
        {
            return new List<Choice>
            {
                new Choice("1. No"){Synonyms = new List<string> {"1", "Nope", "Don't ask", "not good"}, Value = "Very unhealthy"},
                new Choice("2. Not too well"){Synonyms = new List<string> {"2", "bad"}, Value = "Not too well"},
                new Choice("3. Neither good nor bad ") {Synonyms = new List<string> { "3", "neutral", "normal", "normally" }, Value = "Neither good nor bad"},
                new Choice("4. Yes, good enough"){Synonyms = new List<string> {"4", "well", "good enough"}, Value = "Yes, good enough"},
                new Choice("5. Yes - very healthy"){Synonyms = new List<string> {"5", "Yes", "Yea", "Of course", "Sure"}, Value = "Yes - very healthy"},
            };
        }
    }
}