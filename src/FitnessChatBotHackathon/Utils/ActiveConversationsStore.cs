using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Serilog;

namespace Fitness.ChatBot.Utils
{
    public class ActiveConversationsStore {
        public async Task<ConversationReference[]> GetAll()
        {
            await Task.Delay(0);
            return Rv.WithRaven(session =>
                session
                    .Query<ConversationReference>()
                    .ToArray());
        }

        public async Task SaveReference(ConversationReference reference)
        {
            await Task.Delay(0);
            Rv.WithRaven(session =>
            {
                session.Store(reference, reference.Conversation.Id);
                session.SaveChanges();
            });
        }

        public async Task RemoveOldConversations(string userId)
        {
            await Task.Delay(0);
            Log.Information("Removing old conversations for user: {recipientId}", userId);
            Rv.WithRaven(session =>
            {
                var allConversations = session
                    .Query<ConversationReference>()
                    .ToArray();

                var existingConversations = allConversations
                    .Where(x => x.User.Id == userId)
                    .ToArray();

                Log.Information("Found {conversationsToRemove} old conversations to remove", existingConversations.Length);
                foreach (var c in existingConversations.Take(20)) // session limit is 30 operations
                {
                    session.Delete(c);
                }

                session.SaveChanges();
            });
        }

        public async Task RemoveConversationOnError(string conversationId)
        {
            await Task.Delay(0);
            Log.Warning("Removing {conversationWithError} conversations after error", conversationId);

            Rv.WithRaven(session =>
            {
//                session.Delete(conversationId);
                session.SaveChanges();
            });
        }

        public async Task Check(ConversationReference reference)
        {
            await Task.Delay(0);
            Rv.WithRaven(session =>
            {
                var allConversations = session
                    .Query<ConversationReference>()
                    .ToArray();

                var existingConversationsFound = allConversations
                    .Any(x => x.User.Id == reference.User.Id);

                if (!existingConversationsFound)
                {
                    session.Store(reference, reference.Conversation.Id);
                    session.SaveChanges();
                }
            });
        }
    }
}