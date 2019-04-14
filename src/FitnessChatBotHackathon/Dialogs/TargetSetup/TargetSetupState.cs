using System;

namespace Fitness.ChatBot.Dialogs.TargetSetup
{
    public class TargetSetupState
    {
        public int Activity { get; set; }
        public int Food { get; set; }
        public int Sleep { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
