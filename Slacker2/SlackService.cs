﻿using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using SlackAPI;
using SlackAPI.WebSocketMessages;

namespace Slacker2
{
	using Models;

	delegate void SlackMessageReceived(SlackMessage message);

    class SlackService
	{
		public string AuthToken { get; }

		public SlackMessageReceived OnSlackMessage { get; set; }

		private SlackSocketClient Slack { get; }
		private Dictionary<string, SlackUser> Users { get; }

		public SlackService(string authToken)
		{			
			AuthToken = authToken;

			var waitEvent = new ManualResetEvent(false);

			Users = new Dictionary<string, SlackUser>();
			Slack = new SlackSocketClient(AuthToken);
			Slack.Connect(
				(LoginResponse response) =>
				{
					Console.WriteLine("Connected");

					Console.WriteLine(Slack.MyData.name);

					waitEvent.Set();
				});

			Slack.OnMessageReceived += OnMessageReceived;
			Slack.EmitPresence(_ => { }, Presence.active);

			waitEvent.WaitOne();
			waitEvent.Dispose();
		}

		void OnMessageReceived(NewMessage message)
		{
			if (message.user == Slack.MyData.id)
				return;
			
			try
			{
				Channel chInfo = null;
				if (Slack.GroupLookup.ContainsKey(message.channel))
					chInfo = Slack.GroupLookup[message.channel];
				else
					chInfo = Slack.ChannelLookup[message.channel];

				Console.WriteLine(chInfo);
				var members = chInfo.members
					.Select(x => GetUser(x)).ToArray();

				OnSlackMessage?.Invoke(new SlackMessage()
				{
					Slack = this,

					Channel = new SlackChannel()
					{
						Name = chInfo.name,
						IsPublicOpened = chInfo.is_open,
						Topic = chInfo.topic.value,
						Members = members
					},

					Sender = GetUser(message.user),
					Message = message.text
				});
			}
			catch (Exception e) { Console.WriteLine(e); }
		}

		public void SendMessage(string channel, string message)
		{
			Console.WriteLine("[Send] " + message);

			Slack.PostMessage(
				_ => { },
				channel,
				message,
				as_user: true);
		}
		public void SendActionMessage(string channel, string message)
		{
			Slack.PostMessage(
				_ => { },
				channel,
				message,
				as_user: true,
				attachments: new Attachment[]
				{
					new Attachment() {
						title = "test",
						text = "text",
						callback_id = "AA",

						actions = new AttachmentAction[] {
							new AttachmentAction("asdf", "asdf"),
							new AttachmentAction("qwer", "qwer")
						}
					}
				});
		}

		public SlackUser GetUser(string name)
		{
			Console.WriteLine(name);
			Console.WriteLine(Slack.UserLookup.ContainsKey(name));

			var userInfo = Slack.UserLookup[name];

			if (Users.ContainsKey(name) == false)
				Users[name] = new SlackUser() { Name = userInfo.name };

			return Users[name];
		}
	}
}
