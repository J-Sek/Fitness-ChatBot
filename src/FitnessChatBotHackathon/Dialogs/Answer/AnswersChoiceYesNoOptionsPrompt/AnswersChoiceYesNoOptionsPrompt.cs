using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt
{

    public abstract class AnswersChoiceYesNoOptionsPrompt : ICollectAnswers
    {
        protected readonly IStatePropertyAccessor<AnswerState> AnswersStateAccessor;

        protected abstract string QuestionText { get; }
        protected virtual string MessageVeryGood { get; } = RandomVeryGoodMessage();
        protected virtual string MessageVeryBad { get; } = RandomVeryBadMessage();
        protected virtual string Neutral { get; } = RandomNeutralMessage();

        protected AnswersChoiceYesNoOptionsPrompt(IStatePropertyAccessor<AnswerState> answersStateAccessor)
        {
            AnswersStateAccessor = answersStateAccessor;
        }

        public async Task<DialogTurnResult> PromptStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("choiceYesNo", new PromptOptions
            {
                Prompt = MessageFactory.Text(QuestionText),
                Choices = QuestionChoices(),
                RetryPrompt = MessageFactory.Text("Be honest and select one")
            }, cancellationToken);
        }

        protected abstract List<Choice> QuestionChoices();

        public async Task<DialogTurnResult> ValidateStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selectedChoice = stepContext.Result as FoundChoice;
            var selectedIndex = (selectedChoice?.Index).GetValueOrDefault();

            switch (AnswerIdToScore((selectedChoice?.Index).GetValueOrDefault()))
            {
                case ActivityScore.VeryGood:
                case ActivityScore.Good:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(MessageVeryGood), cancellationToken);
                    break;
                case ActivityScore.Bad:
                case ActivityScore.VeryBad:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(MessageVeryBad), cancellationToken);
                    break;

                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(Neutral), cancellationToken);
                    break;
            }

            var score = AnswerIdToScore(selectedIndex);
            await UpdateInDatabase(stepContext, score);

            return await stepContext.NextAsync(result: null, cancellationToken: cancellationToken);
        }

        protected virtual ActivityScore AnswerIdToScore(int answerId)
        {
            switch (answerId)
            {
                case 0:
                    return ActivityScore.VeryBad;
                case 1:
                    return ActivityScore.Bad;
                case 3:
                    return ActivityScore.Good;
                case 4:
                    return ActivityScore.VeryGood;
                case 2:
                default:
                    return ActivityScore.Neutral;
            }
        }

        private async Task UpdateInDatabase(WaterfallStepContext stepContext, ActivityScore score)
        {
            var answerState = await AnswersStateAccessor.GetAsync(stepContext.Context, () => null) ?? new AnswerState();
            var todaysAnswers = answerState.Questions.FirstOrDefault(a => a.Day == DateProvider.CurrentDateForBot.Date) ?? new QuestionsData();
            UpdateActivityHandler(score, todaysAnswers);

            await AnswersStateAccessor.SetAsync(stepContext.Context, answerState);
        }

        protected abstract void UpdateActivityHandler(ActivityScore isPositiveAnswer, QuestionsData todaysAnswers);


        private static string RandomVeryGoodMessage()
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

        private static string RandomVeryBadMessage()
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

        private static string RandomNeutralMessage()
        {
            var answers = new[]
            {
                "Ok..."
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