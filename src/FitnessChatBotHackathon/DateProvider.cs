using System;

namespace Fitness.ChatBot
{
    public static class DateProvider
    {
        // TODO: BOT COMMANDS - we can use it for control flow of time for showcase? 
        public static DateTime CurrentDateForBot { get; set; } = DateTime.UtcNow;
    }
}