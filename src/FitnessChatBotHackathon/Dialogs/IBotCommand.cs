using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Dialogs
{
    public interface IBotCommand
    {
        string Intent { get; }
        Task Handle(DialogContext ctx);
    }
}