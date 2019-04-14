using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fitness.ChatBot.Dialogs;
using Fitness.ChatBot.Dialogs.Answer;
using Fitness.ChatBot.Dialogs.Greeting;
using Fitness.ChatBot.Dialogs.TargetSetup;
using Fitness.ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Fitness.ChatBot
{
    public class FitnessBot : IBot
    {
        public static readonly string LuisConfiguration = "FitnessChatBotHackathon_core-bot-LUIS";

        private readonly IStatePropertyAccessor<GreetingState> _greetingStateAccessor;
        private readonly IStatePropertyAccessor<AnswerState> _answersStateAccessor;
        private readonly IStatePropertyAccessor<TargetSetupState> _targetSetupStateAccessor;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly IEnumerable<IBotCommand> _botCommands;
        private readonly ActiveConversationsStore _activeConversationsStore;
        private readonly BotServices _services;

        private DialogSet Dialogs { get; set; }

        public FitnessBot(BotServices services, UserState userState, ConversationState conversationState, IEnumerable<IBotCommand> botCommands, ActiveConversationsStore activeConversationsStore)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _botCommands = botCommands;
            _activeConversationsStore = activeConversationsStore;

            _greetingStateAccessor = _userState.CreateProperty<GreetingState>(nameof(GreetingState));
            _answersStateAccessor = _userState.CreateProperty<AnswerState>(nameof(AnswerState));
            _targetSetupStateAccessor = _userState.CreateProperty<TargetSetupState>(nameof(TargetSetupState));
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));

            if (!_services.LuisServices.ContainsKey(LuisConfiguration))
            {
                throw new InvalidOperationException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
            }

            Dialogs = new DialogSet(_dialogStateAccessor);
            Dialogs.Add(new GreetingDialog(_greetingStateAccessor));
            Dialogs.Add(new AnswerDialog(_answersStateAccessor));
            Dialogs.Add(new TargetSetupDialog(_targetSetupStateAccessor));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            var dc = await Dialogs.CreateContextAsync(turnContext);

            var conversationReference = activity.GetConversationReference();
            await _activeConversationsStore.Check(conversationReference);

            if (activity.Type == ActivityTypes.Message)
            {
                var luisResults = await _services.LuisServices[LuisConfiguration].RecognizeAsync(dc.Context, cancellationToken);

                var topScoringIntent = luisResults?.GetTopScoringIntent();
                var topIntent = topScoringIntent.Value.intent;

                await UpdateGreetingState(luisResults, dc.Context);

                if (await AcceptCommand(dc, topIntent))
                {
                    await _conversationState.SaveChangesAsync(turnContext);
                    await _userState.SaveChangesAsync(turnContext);
                    return; // Bypass the dialog.
                }

                var dialogResult = await dc.ContinueDialogAsync();

                // if no one has responded,
                if (!dc.Context.Responded) // TODO: Check if we need `else` block
                {
                    switch (dialogResult.Status)
                    {
                        case DialogTurnStatus.Empty:
                            switch (topIntent)
                            {
                                case Intents.Greeting:
                                    var greetingState = await _greetingStateAccessor.GetAsync(turnContext, () => new GreetingState());

                                    if (!greetingState.SayingGreetingRecently())
                                    {
                                        await dc.BeginDialogAsync(nameof(GreetingDialog));
                                    }

                                    if (greetingState.Completed())
                                    {
                                        await dc.BeginDialogAsync(nameof(AnswerDialog));
                                    }

                                    break;

                                case Intents.Start:
                                    await dc.BeginDialogAsync(nameof(AnswerDialog));
                                    break;

                                case Intents.Target:
                                    await dc.BeginDialogAsync(nameof(TargetSetupDialog));
                                    break;
                                
                                case Intents.None:
                                default:
                                    await dc.Context.Senddd("I didn't understand what you just said to me.");
                                    break;
                            }

                            break;

                        case DialogTurnStatus.Waiting:
                            // The active dialog is waiting for a response from the user, so do nothing.
                            break;

                        case DialogTurnStatus.Complete:
                            await dc.EndDialogAsync();
                            break;

                        default:
                            await dc.CancelAllDialogsAsync();
                            break;
                    }
                }
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (activity.MembersAdded != null)
                {
                    foreach (var member in activity.MembersAdded)
                    {
                        if (member.Id != activity.Recipient.Id)
                        {
                            await dc.Context.Senddd("Welcome new user");
                            await dc.Context.Senddd("Please, type **?** or **help** to list available commands");
                        }
                        else
                        {
                            await _activeConversationsStore.RemoveOldConversations(conversationReference.User.Id);
                        }
                    }
                }
                await _activeConversationsStore.SaveReference(conversationReference);
            }

            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }

        private async Task<bool> AcceptCommand(DialogContext dc, string topIntent)
        {
            var command = _botCommands.FirstOrDefault(x => x.Intent == topIntent);
            if (command == null)
            {
                return false;
            }
            await command.Handle(dc);
            return true;
        }

        private async Task UpdateGreetingState(RecognizerResult luisResult, ITurnContext turnContext)
        {
            if (luisResult.Entities != null && luisResult.Entities.HasValues)
            {
                // Get latest GreetingState
                var greetingState = await _greetingStateAccessor.GetAsync(turnContext, () => new GreetingState());
                var entities = luisResult.Entities;

                // Supported LUIS Entities
                string[] userNameEntities = { "userName", "userName_patternAny" };
                string[] userLocationEntities = { "userLocation", "userLocation_patternAny" };

                // Update any entities
                // Note: Consider a confirm dialog, instead of just updating.
                foreach (var name in userNameEntities)
                {
                    // Check if we found valid slot values in entities returned from LUIS.
                    if (entities[name] != null)
                    {
                        // Capitalize and set new user name.
                        var newName = (string)entities[name][0];
                        greetingState.Name = char.ToUpper(newName[0]) + newName.Substring(1);
                        break;
                    }
                }

                foreach (var city in userLocationEntities)
                {
                    if (entities[city] != null)
                    {
                        // Capitalize and set new city.
                        var newCity = (string)entities[city][0];
                        greetingState.City = char.ToUpper(newCity[0]) + newCity.Substring(1);
                        break;
                    }
                }

                // Set the new values into state.
                await _greetingStateAccessor.SetAsync(turnContext, greetingState);
            }
        }
    }
}