using System.Linq;
using System.Threading.Tasks;
using Fitness.ChatBot.Dialogs.Answer;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Dialogs.Commands
{
    public class StatsCommand : IBotCommand
    {
        public string Intent { get; } = Intents.Stats;

        private readonly UserState _userState;
        private IStatePropertyAccessor<AnswerState> _answersStateAccessor;

        public StatsCommand(UserState userState)
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
                allQuestions.Reverse();
                var lastWeek = allQuestions.Take(7).ToArray();

                await ctx.Context.Senddd("These are your results from last 7 days:");
                await ctx.Context.Senddd(string.Join("\n", new []
                {
                    $"- Activity Habits (target: 6) : avg. **{(lastWeek.Select(x => x.ActivityScore).Cast<int>().Average() * 5):0.0}**",
                    $"- Food Habits (target: 5) : avg. **{(lastWeek.Select(x => x.FoodScore).Cast<int>().Average() * 5):0.0}**",
                    $"- Sleep Habits (target: 8) : avg. **{(lastWeek.Select(x => x.SleepScore).Cast<int>().Average() * 5):0.0}**",
                }));
            }

            if (ctx.ActiveDialog != null)
            {
                await ctx.RepromptDialogAsync();
            }
        }
    }
}