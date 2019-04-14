using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fitness.ChatBot.Dialogs.Greeting;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Serilog;

namespace Fitness.ChatBot.Advice
{
    public class AdviceDialog : ComponentDialog
    {
        private readonly LuisRecognizer _luisRecognizer;
        private readonly IDisplayAdvice _advice;
        private readonly IStatePropertyAccessor<GreetingState> _greetingStateAccessor;
        private const string DisplayAdviceDialog = "DisplayAdviceDialog";

        public AdviceDialog(LuisRecognizer luisRecognizer, IDisplayAdvice advice,
            IStatePropertyAccessor<GreetingState> greetingStateAccessor) : base(nameof(AdviceDialog))
        {
            _luisRecognizer = luisRecognizer;
            _advice = advice;
            _greetingStateAccessor = greetingStateAccessor;

            var waterfallSteps = new WaterfallStep[]
            {
                SummaryStepAsync
            };

            AddDialog(new WaterfallDialog(DisplayAdviceDialog, waterfallSteps));
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResults = await _luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);

            try
            {
                var subject = luisResults.Entities["tipSubject"][0][0].ToString();
                var state = await _greetingStateAccessor.GetAsync(stepContext.Context);

                if (subject.Contains("food") || subject.Contains("diet") || subject.Contains("eat"))
                {
                    state.DisplayedArticles = state.DisplayedArticles ?? new List<string>();
                    await _advice.ShowFoodAdvice(stepContext, state.DisplayedArticles);
                }
                else if (subject.Contains("sleep"))
                {
                    state.DisplayedArticles = state.DisplayedArticles ?? new List<string>();
                    await _advice.ShowSleepAdvice(stepContext, state.DisplayedArticles);
                }
                else 
                {
                    await stepContext.Context.Senddd(MessageFactory.Text("I can give you advice on food or sleep, which one would you like?"));
                }

                await _greetingStateAccessor.SetAsync(stepContext.Context, state);
            }
            catch (Exception e)
            {
                Log.Error(e, "Cannot show advice");
            }
            
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
