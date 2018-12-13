using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slacker2
{
    using Models;

    public class BotTask
    {
        public static async Task Create(BotService bot, SlackChannel channel, string taskName, Func<Task> task)
        {
            var slack = bot.Slack;
            var msg = await slack.SendMessage(channel.Id, $"{taskName}...");
            try
            {
                await Task.Run(task);

                await msg.Update($":ballot_box_with_check: {taskName} SUCCESS");
            }
            catch (Exception e)
            {
                await msg.Update($":ballot_box_with_check: {taskName} FAILURE");

                Console.WriteLine(e);
            }
        }
    }
}
