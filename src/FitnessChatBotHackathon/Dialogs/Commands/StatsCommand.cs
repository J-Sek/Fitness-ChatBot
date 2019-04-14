using System.Threading.Tasks;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Dialogs.Commands
{
    public class StatsCommand : IBotCommand
    {
        public string Intent { get; } = Intents.Stats;

        public async Task Handle(DialogContext ctx)
        {
            await ctx.Context.Senddd("TODO: **STATS**");

            if (ctx.ActiveDialog != null)
            {
                await ctx.RepromptDialogAsync();
            }
        }
    }
}