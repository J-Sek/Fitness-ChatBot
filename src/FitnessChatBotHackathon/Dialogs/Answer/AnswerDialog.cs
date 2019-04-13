using Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fitness.ChatBot.Dialogs.Answer
{
    public class AnswerDialog : ComponentDialog
    {
        private const string CollectAnswersDialog = "collectAnswersDialog";

        public IStatePropertyAccessor<AnswerState> AnswersStateAccessor { get; }

        public AnswerDialog(IStatePropertyAccessor<AnswerState> answersStateAccessor) : base(nameof(AnswerDialog))
        {
            AnswersStateAccessor = answersStateAccessor;
            
            var waterfallSteps = new WaterfallStep[]
            {
                ReadyForAnswersConfirm,
                StartingAnswersStepAsync,
                new ActivityAnswersChoiceYesNoOptionsPrompt(AnswersStateAccessor).PromptStep,
                new ActivityAnswersChoiceYesNoOptionsPrompt(AnswersStateAccessor).ValidateStep,
                new FoodAnswersChoiceYesNoOptionsPrompt(AnswersStateAccessor).PromptStep,
                new FoodAnswersChoiceYesNoOptionsPrompt(AnswersStateAccessor).ValidateStep,
                new SleepAnswersChoiceYesNoOptionsPrompt(AnswersStateAccessor).PromptStep,
                new SleepAnswersChoiceYesNoOptionsPrompt(AnswersStateAccessor).ValidateStep,
                SummaryStepAsync
            };

            AddDialog(new WaterfallDialog(CollectAnswersDialog, waterfallSteps));
            AddDialog(new ChoicePrompt("choiceYesNo"));
            AddDialog(new ConfirmPrompt("confirm"));
        }

        private async Task<DialogTurnResult> ReadyForAnswersConfirm(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var answerState = await AnswersStateAccessor.GetAsync(stepContext.Context, () => null) ?? new AnswerState();
            var todaysAnswers = answerState.Questions.FirstOrDefault(a => a.Day == DateProvider.CurrentDateForBot);

            if (todaysAnswers == null)
                answerState.Questions.Add(new QuestionsData {Day = DateProvider.CurrentDateForBot});

            await AnswersStateAccessor.SetAsync(stepContext.Context, answerState, cancellationToken);

            return await stepContext.PromptAsync("confirm",
                new PromptOptions {Prompt = MessageFactory.Text("Are you ready for few short questions?")},
                cancellationToken);

        }

        private async Task<DialogTurnResult> StartingAnswersStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Good, let's start"), cancellationToken);
                //TODO: pass any data? (result param)
                return await stepContext.NextAsync(result: null, cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.CancelAllDialogsAsync(cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("That's all, thanks"), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
