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
            await ctx.Context.Senddd("Here are some commands you could try:");
            await ctx.Context.Senddd(@"
- **target** - set your targets for Activity, Food and Sleep habits scores
- **start** - (optionally) you could prompt me to start daily questions
- **stats** - I can print snapshot of your current statistics
- **progress** - I can show you how your performance improved over time
- **cancel** - Abort current conversation
");

            if (ctx.ActiveDialog != null)
            {
                await ctx.RepromptDialogAsync();
            }
        }
    }
}