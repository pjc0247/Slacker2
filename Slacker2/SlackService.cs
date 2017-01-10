using System;
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

			Console.WriteLine("Trying to connect to the Slack server....");
			Slack.Connect(
				(LoginResponse response) =>
				{
					if (response.ok == false)
					{
						Console.WriteLine("Auth error : " + response.error);
					}
					
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
				
				var members = chInfo.members
					.Select(x => GetUser(x)).ToArray();

				// datetime -> slack_ts
				string tsString = ((message.ts.ToUniversalTime().Ticks - 621355968000000000m) / 10000000m).ToString("0.000000");

				OnSlackMessage?.Invoke(new SlackMessage()
				{
					Slack = this,

					Channel = new SlackChannel()
					{
						Id = chInfo.id,
						Name = chInfo.name,
						IsPublicOpened = chInfo.is_open,
						Topic = chInfo.topic.value,
						Members = members
					},

					Sender = GetUser(message.user),
					Message = message.text,

					Timestamp = tsString
				});
			}
			catch (Exception e) { Console.WriteLine(e); }
		}

		public void AddReaction(string channel, string messageTimestamp, string reactionName)
		{
			Console.WriteLine(channel);
			Slack.AddReaction(
				_ => { },
				reactionName,
				channel,
				messageTimestamp);
		}

		public void SendMessage(string channel, string message)
		{
			Console.WriteLine("[Send] " + message);

			Slack.PostMessage(
				_ => { Console.WriteLine(_.ts); },
				channel,
				message,
				as_user: true);
		}
		public void SendColoredMessage(string channel, string message, string colorHex, string title, string description)
		{
			Slack.PostMessage(
				_ => { },
				channel,
				message,
				as_user: true,
				attachments: new Attachment[]{
					new Attachment() {
						title = title,
						text = description,
						color = colorHex
					}
				});;	
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
