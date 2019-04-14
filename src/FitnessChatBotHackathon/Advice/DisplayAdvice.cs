using System;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fitness.ChatBot.Advice
{
    public interface IDisplayAdvice
    {
        Task ShowFoodAdvice(DialogContext ctx, List<string> stateDisplayedArticles);
        Task ShowSleepAdvice(DialogContext ctx, List<string> stateDisplayedArticles);
    }

    public class DisplayAdvice : IDisplayAdvice
    {
        public async Task ShowFoodAdvice(DialogContext ctx, List<string> stateDisplayedArticles)
        {
            var cardFileName = SelectNextUnread(new[] { "Food1.json", "Food2.json", "Food3.json", "Food4.json" }, stateDisplayedArticles);
            if (cardFileName == null)
            {
                await DisplayNoMoreArticlesAbout(ctx, "food");
                return;
            }

            stateDisplayedArticles.Add(cardFileName);
            
            await ctx.Context.Senddd(ArticleFoundMessage());
            await ShowCardWithTip(ctx, cardFileName);
        }

        public async Task ShowSleepAdvice(DialogContext ctx, List<string> stateDisplayedArticles)
        {
            var cardFileName = SelectNextUnread(new[] { "Sleep1.json", "Sleep2.json", "Sleep3.json", "Sleep4.json" }, stateDisplayedArticles);
            if (cardFileName == null)
            {
                await DisplayNoMoreArticlesAbout(ctx, "sleep");
                return;
            }
            
            stateDisplayedArticles.Add(cardFileName);
            
            await ctx.Context.Senddd(ArticleFoundMessage());
            await ShowCardWithTip(ctx, cardFileName);
        }

        private static async Task DisplayNoMoreArticlesAbout(DialogContext ctx, string articleSubject)
        {
            await ctx.Context.Senddd($"It seems that I have shown you all articles I had about {articleSubject}");
        }

        private string ArticleFoundMessage()
        {
            return SelectRandom(new[]
            {
                "I found an interesting article for you:",
                "I found an article which might be interesting:",
                "This might be interesting:",
                "This one seems to be worth reading:",
                "Another interesting article I came into:",
                "I would like you to read this one:",
                "This is good article:"
            });
        }
        
        protected static string SelectRandom(string[] answers)
        {
            var answerId = new Random().Next(0, answers.Length);

            return answers[answerId];
        }

        private async Task ShowCardWithTip(DialogContext ctx, string cardFileName)
        {
            var card = CreateAdaptiveCardAttachment(cardFileName);
            var response = CreateResponse(ctx.Context.Activity, card);
            await ctx.Context.SendActivityAsync(response);
        }

        protected string SelectNextUnread(string[] cardFileName, List<string> stateDisplayedArticles)
        {
            return cardFileName.FirstOrDefault(x => !stateDisplayedArticles.Contains(x));
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