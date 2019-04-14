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

            await ctx.Context.Senddd("TODO: **Progress**");

            if (ctx.ActiveDialog != null)
            {
                await ctx.RepromptDialogAsync();
            }
        }
    }
}