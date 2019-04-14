using System.Threading;
using System.Threading.Tasks;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Fitness.ChatBot.Dialogs.TargetSetup
{
    public class TargetSetupDialog : ComponentDialog
    {
        private const string CollectAnswersDialog = "collectTargetsDialog";

        public IStatePropertyAccessor<TargetSetupState> StateAccessor { get; }

        public TargetSetupDialog(IStatePropertyAccessor<TargetSetupState> stateAccessor) : base(nameof(TargetSetupDialog))
        {
            StateAccessor = stateAccessor;

            var waterfallSteps = new WaterfallStep[]
            {
                ActivityPromptStepAsync,
                GetActivityStepAsync,
                FoodPromptStepAsync,
                GetFoodStepAsync,
                SleepPromptStepAsync,
                GetSleepStepAsync,
                ShowSummaryAsync,
            };

            AddDialog(new WaterfallDialog(CollectAnswersDialog, waterfallSteps));
            AddDialog(new NumberPrompt<int>("activityPrompt", TargetNumberValidator));
            AddDialog(new NumberPrompt<int>("foodPrompt", TargetNumberValidator));
            AddDialog(new NumberPrompt<int>("sleepPrompt", TargetNumberValidator));
        }

        private async Task<bool> TargetNumberValidator(PromptValidatorContext<int> ctx, CancellationToken cancellationtoken)
        {
            var value = ctx.Recognized.Value;
            if (value >= 5 && value <= 10)
            {
                return true;
            }
            else
            {
                await ctx.Context.Senddd("Value need to be number between 5 and 10");
                return false;
            }
        }

        private async Task<DialogTurnResult> ActivityPromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("activityPrompt",
                new PromptOptions { Prompt = MessageFactory.Text("What is your **Activity Score** target (number between 5 - 10)?") });
        }

        private async Task<DialogTurnResult> GetActivityStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var s = await StateAccessor.GetAsync(stepContext.Context, () => new TargetSetupState());
            s.Activity = (int)stepContext.Result;
            await StateAccessor.SetAsync(stepContext.Context, s);
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> FoodPromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("foodPrompt",
                new PromptOptions { Prompt = MessageFactory.Text("What is your **Food Score** target (number between 5 - 10)?") });
        }

        private async Task<DialogTurnResult> GetFoodStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var s = await StateAccessor.GetAsync(stepContext.Context, () => new TargetSetupState());
            s.Food = (int)stepContext.Result;
            await StateAccessor.SetAsync(stepContext.Context, s);
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SleepPromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("sleepPrompt",
                new PromptOptions { Prompt = MessageFactory.Text("What is your **Sleep Score** target (number between 5 - 10)?") });
        }

        private async Task<DialogTurnResult> GetSleepStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var s = await StateAccessor.GetAsync(stepContext.Context, () => new TargetSetupState());
            s.Sleep = (int)stepContext.Result;
            await StateAccessor.SetAsync(stepContext.Context, s);
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> ShowSummaryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var s = await StateAccessor.GetAsync(stepContext.Context, () => new TargetSetupState());
            s.LastUpdate = DateProvider.CurrentDateForBot;
            await StateAccessor.SetAsync(stepContext.Context, s);

            await stepContext.Context.Senddd(MessageFactory.Text("That's all, thanks"));
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}