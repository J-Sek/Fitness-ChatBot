using System;
using Microsoft.Bot.Builder;

namespace Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt
{
    public class ActivityAnswersChoiceYesNoOptionsPrompt : AnswersChoiceYesNoOptionsPrompt
    {
        public ActivityAnswersChoiceYesNoOptionsPrompt(IStatePropertyAccessor<AnswerState> answersStateAccessor) : base(answersStateAccessor)
        {
        }

        protected override string QuestionText { get; } = "Have you been on **Gym** today?";

        protected override void UpdateActivityHandler(bool isPositiveAnswer, QuestionsData todaysAnswers)
        {
            todaysAnswers.Activity = isPositiveAnswer;
        }
    }
}