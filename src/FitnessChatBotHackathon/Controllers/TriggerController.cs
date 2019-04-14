using System.Threading;
using System.Threading.Tasks;
using Fitness.ChatBot.Dialogs.Answer;
using Fitness.ChatBot.Dialogs.Greeting;
using Fitness.ChatBot.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

namespace Fitness.ChatBot.Controllers
{
    [Route("api/trigger")]
    [ApiController]
    public class TriggerController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly ActiveConversationsStore _activeConversationsStore;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly string _appId;

        public TriggerController(IBotFrameworkHttpAdapter adapter, ICredentialProvider credentials, ActiveConversationsStore activeConversationsStore, ConversationState conversationState, UserState userState)
        {
            _adapter = adapter;
            _activeConversationsStore = activeConversationsStore;
            _conversationState = conversationState;
            _userState = userState;
            _appId = ((SimpleCredentialProvider)credentials).AppId;
        }

        [Route("ping")]
        public async Task<IActionResult> TriggerPing()
        {
            var conversations = await _activeConversationsStore.GetAll();
            foreach (var c in conversations)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, c, PingCallback, default(CancellationToken));
            }
            return Ok();
        }

        [Route("nextDay")]
        public async Task<IActionResult> TriggerDailyQuestions()
        {
            DateProvider.CurrentDateForBot = DateProvider.CurrentDateForBot.AddDays(1);
            var conversations = await _activeConversationsStore.GetAll();
            foreach (var c in conversations)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, c, QuestionsCallback, default(CancellationToken));
            }
            return Ok();
        }

        private async Task PingCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Ping", cancellationToken: cancellationToken);
        }

        private async Task QuestionsCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            var answersStateAccessor = _userState.CreateProperty<AnswerState>(nameof(AnswerState));
            var greetingStateAccessor = _userState.CreateProperty<GreetingState>(nameof(GreetingState));

            var dialogs = new DialogSet(dialogStateAccessor);
            dialogs.Add(new GreetingDialog(greetingStateAccessor));
            dialogs.Add(new AnswerDialog(answersStateAccessor));

            var dc = await dialogs.CreateContextAsync(turnContext);

            await dc.CancelAllDialogsAsync();

            await dc.BeginDialogAsync(nameof(AnswerDialog));
        }
    }
}