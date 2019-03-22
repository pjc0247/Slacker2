using System;
using System.Threading.Tasks;

namespace Slacker2
{
    using Models;

    /// <summary>
    /// </summary>
    /// <remarks>
    /// Without `Async` suffix methods do not mean 'Sync'.
    /// It's just means you don't care about its results.
    /// </remarks>
    public class BotService
    {
        internal SlackService Slack { get; set; }

        protected Task AddReactionAsync(SlackMessage message, string reactionName)
        {
            reactionName = reactionName.Replace(":", "");
            return Slack.AddReaction(message.Channel.Id, message.Timestamp, reactionName);
        }
        protected void AddReaction(SlackMessage message, string reactionName)
        {
            AddReactionAsync(message, reactionName);
        }
        protected Task UpdateMessageAsync(SlackMessage message, string messageToUpdate)
        {
            return Slack.UpdateMessage(message.Channel.Id, message.Timestamp, messageToUpdate);
        }
        protected void UpdateMessage(SlackMessage message, string messageToUpdate)
        {
            UpdateMessageAsync(message, messageToUpdate);
        }

        protected Task<SlackMessage> SendColoredMessageAsync(string channel, string message, string colorHex, string title, string description)
        {
            return Slack.SendColoredMessage(channel, message, colorHex, title, description);
        }
        protected void SendColoredMessage(string channel, string message, string colorHex, string title, string description)
        {
            SendColoredMessageAsync(channel, message, colorHex, title, description);
        }

        protected Task<SlackMessage> SendColoredMessageAsync(SlackChannel channel, string message, string colorHex, string title, string description)
        {
            return Slack.SendColoredMessage(channel.Name, message, colorHex, title, description);
        }
        protected void SendColoredMessage(SlackChannel channel, string message, string colorHex, string title, string description)
        {
            SendColoredMessageAsync(channel, message, colorHex, title, description);
        }


        protected Task<SlackMessage> SendMessageAsync(string channel, string message)
        {
            return Slack.SendMessage(channel, message);
        }
        protected void SendMessage(string channel, string message)
        {
            SendMessageAsync(channel, message);
        }

        protected Task<SlackMessage> SendMessageAsync(SlackChannel channel, string message)
        {
            return Slack.SendMessage(channel.Name, message);
        }
        protected void SendMessage(SlackChannel channel, string message)
        {
            SendMessageAsync(channel, message);
        }

        protected Task<SlackMessage> SendActionMessageAsync(SlackChannel channel, string message, SlackInteractiveMessage messageData)
        {
            return Slack.SendActionMessage(channel.Name, message, messageData);
        }
        protected void SendActionMessage(SlackChannel channel, string message, SlackInteractiveMessage messageData)
        {
            SendActionMessageAsync(channel, message, messageData);
        }

        protected Task<string> SendActionMessageAndWait(SlackChannel channel, string message, SlackInteractiveMessage messageData)
        {
            return Slack.SendActionMessageAndWait(channel.Name, message, messageData);
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
