using System;

namespace Slacker2
{
    public class SlackBotConfiguration
    {
        /// <summary>
        /// Your Slackbot AuthToken
        /// </summary>
        public string AuthToken { get; set; }
        
        /// <summary>
        /// How precise scheduler should be,
        /// Less value means more awakes.
        /// </summary>
        public int SchedulerResolution { get; set; }
        /// <summary>
        /// If true, my(bot) messages never be processed.
        /// </summary>
        public bool AlwaysIgnoreMyMessage { get; set; }

        public SlackBotConfiguration()
        {
            SchedulerResolution = 5;
            AlwaysIgnoreMyMessage = false;
        }
    }
}
