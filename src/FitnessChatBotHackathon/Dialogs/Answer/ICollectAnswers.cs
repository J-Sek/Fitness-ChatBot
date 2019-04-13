using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Dialogs.Answer
{
    public interface ICollectAnswers
    {
        Task<DialogTurnResult> PromptStep(WaterfallStepContext stepContext, CancellationToken cancellationToken);
        Task<DialogTurnResult> ValidateStep(WaterfallStepContext stepContext, CancellationToken cancellationToken);
    }
}