using System.Threading.Tasks;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Dialogs.Commands
{
    public class ProgressCommand : IBotCommand
    {
        public string Intent { get; } = Intents.Progress;

        public async Task Handle(DialogContext ctx)
        {
            await ctx.Context.Senddd("TODO: **Progress**");

            if (ctx.ActiveDialog != null)
            {
                await ctx.RepromptDialogAsync();
            }
        }
    }
}