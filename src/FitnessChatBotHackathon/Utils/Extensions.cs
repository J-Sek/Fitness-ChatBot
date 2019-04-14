using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Fitness.ChatBot.Utils
{
    public static class Extensions
    {
        public static async Task Senddd(this ITurnContext ctx, IActivity activity)
        {
            await ctx.SendWithDelay(x => x.SendActivityAsync(activity), 1000);
        }

        public static async Task Senddd(this ITurnContext ctx, string message)
        {
            await ctx.SendWithDelay(x => x.SendActivityAsync(message), 1000);
        }

        public static async Task SendWithDelay(this ITurnContext ctx, Func<ITurnContext, Task> callback, int delay)
        {
            var reply = ctx.Activity.CreateReply(string.Empty);
            reply.Type = ActivityTypes.Typing;
            await ctx.SendActivityAsync(reply);
            await Task.Delay(delay);
            await callback(ctx);
        }
    }
}