using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Fitness.ChatBot.Advice
{
    public interface IDisplayAdvice
    {
        Task ShowFoodAdvice(DialogContext ctx);
        Task ShowSleepAdvice(DialogContext ctx);
    }

    public class DisplayAdvice : IDisplayAdvice
    {

        public async Task ShowFoodAdvice(DialogContext ctx)
        {
            await ShowCardWithTip(ctx, SelectRandom(new[] {"Food2.json", "Food2.json"}));
        }

        public async Task ShowSleepAdvice(DialogContext ctx)
        {
            await ShowCardWithTip(ctx, SelectRandom(new[] {"Sleep1.json", "Sleep2.json"}));
        }

        private async Task ShowCardWithTip(DialogContext ctx, string cardFileName)
        {
            var card = CreateAdaptiveCardAttachment(cardFileName);
            var response = CreateResponse(ctx.Context.Activity, card);
            await ctx.Context.SendActivityAsync(response);
        }
        
        protected static string SelectRandom(string[] cardFileName)
        {
            var answerId = new Random().Next(0, cardFileName.Length);

            return cardFileName[answerId];
        }

        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        private Attachment CreateAdaptiveCardAttachment(string cardFile)
        {
            var adaptiveCard = File.ReadAllText(
                Path.Combine(@".\Dialogs\Welcome\Resources", cardFile)
            );

            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
    }
}