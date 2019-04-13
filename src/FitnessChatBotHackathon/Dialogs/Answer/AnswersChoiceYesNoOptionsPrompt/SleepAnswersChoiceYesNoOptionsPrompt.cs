using Microsoft.Bot.Builder;

namespace Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt
{
    public class SleepAnswersChoiceYesNoOptionsPrompt : AnswersChoiceYesNoOptionsPrompt
    {
        public SleepAnswersChoiceYesNoOptionsPrompt(IStatePropertyAccessor<AnswerState> answersStateAccessor): base(answersStateAccessor)
        {
        }

        protected override string QuestionText { get; } = "Did you sleep well?";
        protected override string QuestionPositive { get; } = "That's great! Sleep is very important";
        protected override string QuestionNegative { get; } = "It's not good, maybe later I could try searching for some tips for you";

        protected override void UpdateActivityHandler(bool isPositiveAnswer, QuestionsData todaysAnswers)
        {
            todaysAnswers.Sleep = isPositiveAnswer;
        }
    }
}