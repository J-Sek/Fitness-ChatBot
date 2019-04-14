using Fitness.ChatBot.Dialogs.Answer;
using Fitness.ChatBot.Dialogs.TargetSetup;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitness.ChatBot.Advice;
using Fitness.ChatBot.Dialogs.Greeting;

namespace Fitness.ChatBot.Dialogs.Commands
{
    public class StatsCommand : IBotCommand
    {
        public string Intent { get; } = Intents.Stats;
        
        private readonly IStatePropertyAccessor<GreetingState> _greetingStateAccessor;
        private readonly IStatePropertyAccessor<AnswerState> _answersStateAccessor;
        private readonly IStatePropertyAccessor<TargetSetupState> _targetSetupStateAccessor;
        private readonly IDisplayAdvice _displayAdvice;

        public StatsCommand(UserState userState, IDisplayAdvice displayAdvice)
        {
            _displayAdvice = displayAdvice;
            _greetingStateAccessor = userState.CreateProperty<GreetingState>(nameof(GreetingState));
            _answersStateAccessor = userState.CreateProperty<AnswerState>(nameof(AnswerState));
            _targetSetupStateAccessor = userState.CreateProperty<TargetSetupState>(nameof(TargetSetupState));
        }

        public async Task Handle(DialogContext ctx)
        {
            var answerState = await _answersStateAccessor.GetAsync(ctx.Context);
            var targets = await _targetSetupStateAccessor.GetAsync(ctx.Context) ?? new TargetSetupState();

            if ((answerState?.Questions.Count ?? 0) == 0)
            {
                await ctx.Context.Senddd("You need to answer questions at least once");
            }
            else
            {
                var allQuestions = answerState.Questions.ToArray();
                allQuestions.Reverse();
                var lastWeek = allQuestions.Take(7).ToArray();

                await ctx.Context.Senddd("These are your results from last 7 days:");
                await ctx.Context.Senddd(string.Join("\n", new []
                {
                    $"- Activity Habits [target: **{targets.Activity}**] : avg. **{(lastWeek.Select(x => x.ActivityScore).Cast<int>().Average() * 5):0.0}**",
                    $"- Food Habits [target: **{targets.Food}**] : avg. **{(lastWeek.Select(x => x.FoodScore).Cast<int>().Average() * 5):0.0}**",
                    $"- Sleep Habits [target: **{targets.Sleep}**] : avg. **{(lastWeek.Select(x => x.SleepScore).Cast<int>().Average() * 5):0.0}**",
                }));

                if (allQuestions.Length > 3)
                {
                    var spearmanMatrix = MathNet.Numerics.Statistics.Correlation.SpearmanMatrix(
                        allQuestions.Select(a => (double) a.FoodScore).ToArray(),
                        allQuestions.Select(a => (double) a.SleepScore).ToArray(),
                        allQuestions.Select(a => (double) a.ActivityScore).ToArray());

                    var foodToActivity = spearmanMatrix.At(0, 2);
                    var sleepToActivity = spearmanMatrix.At(1, 2);

                    var userState = await _greetingStateAccessor.GetAsync(ctx.Context);
                    userState.DisplayedArticles = userState.DisplayedArticles ?? new List<string>();

                    if (Math.Abs(foodToActivity - sleepToActivity) < .30)
                    {
                        //rather equal
                        await ctx.Context.Senddd("It seems that food and sleep is equally important for your training, this is good balance.");
                    }
                    else if (foodToActivity > sleepToActivity)
                    {
                        //food is more important factor
                        await ctx.Context.Senddd("It seems that food very important factor for your trainings. Remember about importance of good sleep too.");
                        await _displayAdvice.ShowSleepAdvice(ctx, userState.DisplayedArticles);
                    }
                    else
                    {
                        //sleep is more important factor
                        await ctx.Context.Senddd("Sleep is most important factor for your trainings. If you take care of your diet too you can get even further with your training results.");
                        await _displayAdvice.ShowFoodAdvice(ctx, userState.DisplayedArticles);
                    }

                    await _greetingStateAccessor.SetAsync(ctx.Context, userState);
                }
            }

            if (ctx.ActiveDialog != null)
            {
                await ctx.RepromptDialogAsync();
            }
        }

    }
}