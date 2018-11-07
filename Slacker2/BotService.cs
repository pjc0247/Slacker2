using System;
using System.Threading.Tasks;

namespace Slacker2
{
	using Models;

	public class BotService
	{
		internal SlackService Slack { get; set; }

		protected void AddReaction(SlackMessage message, string reactionName)
		{
			reactionName = reactionName.Replace(":", "");
			Slack.AddReaction(message.Channel.Id, message.Timestamp, reactionName);
		}
        protected void UpdateMessage(SlackMessage message, string messageToUpdate)
        {
            Slack.UpdateMessage(message.Channel.Id, message.Timestamp, messageToUpdate);
        }

		protected Task<SlackMessage> SendColoredMessage(string channel, string message, string colorHex, string title, string description)
		{
			return Slack.SendColoredMessage(channel, message, colorHex, title, description);
		}
		protected Task<SlackMessage> SendColoredMessage(SlackChannel channel, string message, string colorHex, string title, string description)
		{
			return Slack.SendColoredMessage(channel.Name, message, colorHex, title, description);
		}

		protected Task<SlackMessage> SendMessage(string channel, string message)
		{
			return Slack.SendMessage(channel, message);
		}
		protected Task<SlackMessage> SendMessage(SlackChannel channel, string message)
		{
			return Slack.SendMessage(channel.Name, message);
		}
		protected Task<SlackMessage> SendActionMessage(SlackChannel channel, string message, SlackInteractiveMessage messageData)
		{
			return Slack.SendActionMessage(channel.Name, message, messageData);
		}

		protected void GrantPermission(SlackUser user, string permission)
		{
			user.Permissions.Add(permission);
		}
		protected void DeletePermission(SlackUser user, string permission)
		{
			user.Permissions.Remove(permission);
		}
	}
}
