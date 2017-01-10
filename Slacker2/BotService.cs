﻿using System;

namespace Slacker2
{
	using Models;

	public class BotService
	{
		internal SlackService Slack { get; set; }

		protected void SendColoredMessage(string channel, string message, string colorHex, string title, string description)
		{
			Slack.SendColoredMessage(channel, message, colorHex, title, description);
		}
		protected void SendColoredMessage(SlackChannel channel, string message, string colorHex, string title, string description)
		{
			Slack.SendColoredMessage(channel.Name, mesasge, colorHex, title, description);
		}

		protected void SendMessage(string channel, string message)
		{
			Slack.SendMessage(channel, message);
		}
		protected void SendMessage(SlackChannel channel, string message)
		{
			Slack.SendMessage(channel.Name, message);
		}
		protected void SendActionMessage(SlackChannel channel, string message)
		{
			Slack.SendActionMessage(channel.Name, message);
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
