using Microsoft.Bot.Builder;

namespace Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt
{
    public class FoodAnswersChoiceYesNoOptionsPrompt : AnswersChoiceYesNoOptionsPrompt
    {
        public FoodAnswersChoiceYesNoOptionsPrompt(IStatePropertyAccessor<AnswerState> answersStateAccessor): base(answersStateAccessor)
        {
        }

        protected override string QuestionText { get; } = "What about food? Have you eaten healthy?";

        protected override void UpdateActivityHandler(bool isPositiveAnswer, QuestionsData todaysAnswers)
        {
            todaysAnswers.Food = isPositiveAnswer;
        }
    }
}