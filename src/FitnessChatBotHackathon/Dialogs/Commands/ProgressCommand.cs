using System;
using System.Linq;
using System.Threading.Tasks;
using Fitness.ChatBot.Dialogs.Answer;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Dialogs.Commands
{
    public class ProgressCommand : IBotCommand
    {
        public string Intent { get; } = Intents.Progress;

        private readonly UserState _userState;
        private IStatePropertyAccessor<AnswerState> _answersStateAccessor;

        public ProgressCommand(UserState userState)
        {
            _userState = userState;
            _answersStateAccessor = _userState.CreateProperty<AnswerState>(nameof(AnswerState));
        }

        public async Task Handle(DialogContext ctx)
        {
            var answerState = await _answersStateAccessor.GetAsync(ctx.Context);

            if ((answerState?.Questions.Count ?? 0) == 0)
            {
                await ctx.Context.Senddd("You need to answer questions at least once");
            }
            else
            {
                var allQuestions = answerState.Questions.ToArray();
                var last3Weeks = allQuestions.Reverse().Take(21)
                    .Select((x, i) => (stats: x, week: Math.Floor((float) i / 7)))
                    .GroupBy(x => x.week)
                    .Select(x => (
                        week: x.Key,
                        averageActivity: x.Select(a => a.stats.ActivityScore).Cast<int>().Average() * 5,
                        averageFood: x.Select(a => a.stats.FoodScore).Cast<int>().Average() * 5,
                        averageSleep: x.Select(a => a.stats.SleepScore).Cast<int>().Average() * 5
                    ))
                    .ToArray()
                    .Reverse()
                    .ToArray();

                var baseWeek = (DateProvider.CurrentDateForBot.DayOfYear - 7 + (int)(new DateTime(2019,01,01).DayOfWeek)) / 7 + 1;

                var activityTrend = last3Weeks
                    .Select((x, i) => (week: baseWeek + i, value: x.averageActivity, change: i == 0 ? "●" : ChangeOf(x.averageActivity, last3Weeks[i - 1].averageActivity)))
                    .ToArray();
                var foodTrend = last3Weeks
                    .Select((x, i) => (week: baseWeek + i, value: x.averageFood, change: i == 0 ? "●" : ChangeOf(x.averageFood, last3Weeks[i - 1].averageFood)))
                    .ToArray();
                var sleepTrend = last3Weeks
                    .Select((x, i) => (week: baseWeek + i, value: x.averageSleep, change: i == 0 ? "●" : ChangeOf(x.averageSleep, last3Weeks[i - 1].averageSleep)))
                    .ToArray();

                await ctx.Context.Senddd($"These are your results from last {Math.Max(21, allQuestions.Length)} days:");
                await ctx.Context.Senddd(string.Join("\n", new []
                    {
                        $"- Activity Habits [target: 6]\n\n   - {string.Join("\n   - ", activityTrend.Select(x => $"{x.change} week {x.week}: **{x.value:0.0}**"))}",
                        $"- Food Habits [target: 5]\n\n   - {string.Join("\n   - ", foodTrend.Select(x => $"{x.change} week {x.week}: **{x.value:0.0}**"))}",
                        $"- Sleep Habits [target: 8]\n\n   - {string.Join("\n   - ", sleepTrend.Select(x => $"{x.change} week {x.week}: **{x.value:0.0}**"))}",
                    }));
            }

            if (ctx.ActiveDialog != null)
            {
                await ctx.RepromptDialogAsync();
            }
        }

        private string ChangeOf(double a, double b)
        {
            return a < b ? "🡶" : a > b ? "🡵" : "●";
        }
    }
}