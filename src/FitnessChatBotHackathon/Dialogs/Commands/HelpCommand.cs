using System.Threading.Tasks;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Dialogs.Commands
{
    public class HelpCommand : IBotCommand
    {
        public string Intent { get; } = Intents.Help;

        public async Task Handle(DialogContext ctx)
        {
            // TODO: Describe our bot capabilities

            await ctx.Context.Senddd("Let me try to provide some help.");
            await ctx.Context.Senddd("I understand greetings, being asked for help, or being asked to cancel what I am doing.");

            if (ctx.ActiveDialog != null)
            {
                await ctx.RepromptDialogAsync();
            }
        }
    }
}