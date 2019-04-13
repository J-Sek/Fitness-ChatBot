using System;

namespace Fitness.ChatBot.Dialogs.Greeting
{
    public class GreetingState
    {
        public string Name { get; set; }

        public string City { get; set; }

        public DateTime LastGreeting { get; set; }
    }

    public static class GreetingStateExtensions
    {
        public static bool Completed(this GreetingState greetingState)
        {
            return greetingState != null
                   && !string.IsNullOrWhiteSpace(greetingState.Name)
                   && !string.IsNullOrWhiteSpace(greetingState.City);
        }


        public static bool SayingGreetingRecently(this GreetingState greetingState)
        {
            if (!greetingState.Completed())
            {
                return false;
            }

            return (DateProvider.CurrentDateForBot - greetingState.LastGreeting).Minutes < 2;

        }
    }
}
