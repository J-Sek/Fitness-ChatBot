using System;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace Fitness.ChatBot.Infrastructure
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        private readonly ActiveConversationsStore _activeConversationsStore;

        public AdapterWithErrorHandler(ICredentialProvider credentialProvider, ILogger<BotFrameworkHttpAdapter> logger, ActiveConversationsStore activeConversationsStore, ConversationState conversationState = null)
            : base(credentialProvider)
        {
            _activeConversationsStore = activeConversationsStore;
            OnTurnError = async (turnContext, exception) =>
            {
                logger.LogError($"Exception caught : {exception.Message}");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);

                        var conversationId = turnContext.Activity.GetConversationReference().Conversation.Id;
                        await _activeConversationsStore.RemoveConversationOnError(conversationId);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }
    }
}