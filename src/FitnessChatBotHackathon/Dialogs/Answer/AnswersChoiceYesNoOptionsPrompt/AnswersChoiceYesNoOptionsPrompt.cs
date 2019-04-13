using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt
{
    public abstract class AnswersChoiceYesNoOptionsPrompt : ICollectAnswers
    {
        protected readonly IStatePropertyAccessor<AnswerState> AnswersStateAccessor;

        protected abstract string QuestionText { get; }
        protected virtual string QuestionPositive { get; } = QuestionPositiveRandom();
        protected virtual string QuestionNegative { get; } = QuestionNegativeRandom();

        protected AnswersChoiceYesNoOptionsPrompt(IStatePropertyAccessor<AnswerState> answersStateAccessor)
        {
            AnswersStateAccessor = answersStateAccessor;
        }

        public async Task<DialogTurnResult> PromptStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("choiceYesNo", new PromptOptions
            {
                Prompt = MessageFactory.Text(QuestionText),
                Choices = new List<Choice>
                {
                    new Choice("Yes"){Synonyms = new List<string> {"Yea", "Of course", "Sure"}},
                    new Choice("No"){Synonyms = new List<string> {"Nope", "Don't ask"}},
                    new Choice("I don't want to tell you") {Synonyms = new List<string> { "go away" }},
                },
                RetryPrompt = MessageFactory.Text("Be honest and select one")
            }, cancellationToken);
        }

        public async Task<DialogTurnResult> ValidateStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selectedChoice = stepContext.Result as FoundChoice;
            var isPositiveAnswer = selectedChoice?.Value?.ToLower() == "yes";
            
            if (isPositiveAnswer)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(QuestionPositive), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(QuestionNegative), cancellationToken);
            }
            
            
            await UpdateInDatabase(stepContext, isPositiveAnswer);
            
            return await stepContext.NextAsync(result: null, cancellationToken: cancellationToken);
        }

        private async Task UpdateInDatabase(WaterfallStepContext stepContext, bool isPositiveAnswer)
        {
            var answerState = await AnswersStateAccessor.GetAsync(stepContext.Context, () => null) ?? new AnswerState();
            var todaysAnswers = answerState.Questions.FirstOrDefault(a => a.Day == DateProvider.CurrentDateForBot);
            UpdateActivityHandler(isPositiveAnswer, todaysAnswers);

            await AnswersStateAccessor.SetAsync(stepContext.Context, answerState);
        }

        protected abstract void UpdateActivityHandler(bool isPositiveAnswer, QuestionsData todaysAnswers);


        private static string QuestionPositiveRandom()
        {
            var answers = new[]
            {
                "Good job",
                "Wow, you are doing amazing job!",
                "You know what matters, good job",
                "I'm impressed",
                "You always make me smile :-)",
                "I knew I can count on you"
            };

            return SelectRandom(answers);
        }

        private static string QuestionNegativeRandom()
        {
            var answers = new[]
            {
                "Well, at least you were honest...",
                "You can do better",
                "Well, it happens",
                "Learn to forgive yourself and... do not let me down next time ;-)"
            };

            return SelectRandom(answers);
        }

        protected static string SelectRandom(string[] answers)
        {
            var answerId = new Random().Next(0, answers.Length);

            return answers[answerId];
        }
    }
}