using System;
using Fitness.ChatBot.Dialogs.Answer.AnswersChoiceYesNoOptionsPrompt;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fitness.ChatBot.Advice;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder.AI.Luis;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Fitness.ChatBot.Dialogs.Answer
{
    public class AdviceDialog : ComponentDialog
    {
        private LuisRecognizer _luisRecognizer;
        private readonly IDisplayAdvice _advice;
        private const string DisplayAdviceDialog = "DisplayAdviceDialog";

        public AdviceDialog(LuisRecognizer luisRecognizer, IDisplayAdvice advice) : base(nameof(AdviceDialog))
        {
            _luisRecognizer = luisRecognizer;
            _advice = advice;

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
                if (subject.Contains("food") || subject.Contains("diet"))
                {
                    await _advice.ShowFoodAdvice(stepContext);
                } 
                else if (subject.Contains("sleep"))
                {
                    await _advice.ShowSleepAdvice(stepContext);
                }
                else 
                {
                    await stepContext.Context.Senddd(MessageFactory.Text("I can give you advice on food or sleep, which one would you like?"));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Cannot show advice");
            }
            
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
