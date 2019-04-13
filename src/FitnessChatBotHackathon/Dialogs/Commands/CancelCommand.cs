using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Dialogs.Commands
{
    public class CancelCommand : IBotCommand
    {
        public string Intent { get; } = Intents.Cancel;

        public async Task Handle(DialogContext ctx)
        {
            if (ctx.ActiveDialog != null)
            {
                await ctx.CancelAllDialogsAsync();
                await ctx.Context.SendActivityAsync("Ok. I've canceled our last activity.");
            }
            else
            {
                await ctx.Context.SendActivityAsync("I don't have anything to cancel.");
            }
        }
    }
}