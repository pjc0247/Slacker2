using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
					Console.WriteLine("  logged in as " + Slack.MyData.name);

					waitEvent.Set();
				});

			Slack.OnMessageReceived += OnMessageReceived;
			Slack.EmitPresence(_ => { }, Presence.active);

			waitEvent.WaitOne();
			waitEvent.Dispose();
		}

		void OnMessageReceived(NewMessage message)
		{
			if (SlackBot.Configuration.AlwaysIgnoreMyMessage && 
                message.user == Slack.MyData.id)
				return;
			
			try
			{
				Channel chInfo = null;

                if (Slack.DirectMessageLookup.ContainsKey(message.channel))
                    return;
                else if (Slack.GroupLookup.ContainsKey(message.channel))
                    chInfo = Slack.GroupLookup[message.channel];
                else
                    chInfo = Slack.ChannelLookup[message.channel];
				
				var members = chInfo.members
					.Select(x => GetUser(x)).ToArray();

				// datetime -> slack_ts
				string tsString = ((message.ts.ToUniversalTime().Ticks - 621355968000000000m) / 10000000m).ToString("0.000000");
				string threadTsString = null;

				if (message.thread_ts != default(DateTime))
					threadTsString = ((message.thread_ts.ToUniversalTime().Ticks - 621355968000000000m) / 10000000m).ToString("0.000000");
				
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

					Timestamp = tsString,
					ThreadTimestamp = threadTsString
				});
			}
			catch (Exception e) { Console.WriteLine(e); }
		}

		public void AddReaction(string channel, string messageTimestamp, string reactionName)
		{
            if (reactionName.StartsWith(":") && reactionName.EndsWith(":"))
                reactionName = reactionName.Substring(1, reactionName.Length - 2);

			Slack.AddReaction(
				_ => { },
				reactionName,
				channel,
				messageTimestamp);
		}
		public void SendThreadMessage(string channel, string messageTimestamp, string message)
		{
			Slack.PostMessage(
				_ => { },
				channel,
				message,
				as_user: true,
				thread_ts: messageTimestamp);
		}

		public Task<SlackMessage> SendMessage(string channel, string message)
		{
			Console.WriteLine("[Send] " + message);

            var ts = new TaskCompletionSource<SlackMessage>();

			Slack.PostMessage(
				_ => {
                    try
                    {
                        ts.SetResult(SlackMessage.Create(this, channel, _.message));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                },
				channel,
				message,
				as_user: true);

            return ts.Task;
		}

        public Task UpdateMessage(string channel, string messageTimestamp, string messageToUpdate)
        {
            var ts = new TaskCompletionSource<object>();

            Slack.Update(
                _ => { ts.SetResult(0); },
                messageTimestamp,
                channel,
                messageToUpdate);

            return ts.Task;
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
		public void SendActionMessage(string channel, string message, SlackInteractiveMessage messageData)
		{
            var actions = new List<AttachmentAction>();

            foreach (var button in messageData.Buttons)
                actions.Add(new AttachmentAction(button.Name, button.Text));

			Slack.PostMessage(
				_ => { },
				channel,
				message,
				as_user: true,
				attachments: new Attachment[]
				{
					new Attachment() {
						title = messageData.Text,
						text = messageData.Description,
						callback_id = messageData.CallbackId,

						actions = actions.ToArray()
					}
				});
		}

        public SlackChannel GetChannelByName(string channel)
        {
            var ch = Slack.Groups
                .Where(x => x.name == channel)
                .First();
            var members = ch.members
                    .Select(x => GetUser(x)).ToArray();

            return new SlackChannel()
            {
                Id = ch.id,
                Name = ch.name,
                IsPublicOpened = !ch.IsPrivateGroup,
                Members = members,
                Topic = ch.topic.value
            };
        }
        public SlackChannel GetChannelById(string channel)
        {
            var ch = Slack.ChannelLookup[channel];
            var members = ch.members
                    .Select(x => GetUser(x)).ToArray();

            return new SlackChannel()
            {
                Id = ch.id,
                Name = ch.name,
                IsPublicOpened = !ch.IsPrivateGroup,
                Members = members,
                Topic = ch.topic.value
            };
        }
		public SlackUser GetUser(string name)
		{
			var userInfo = Slack.UserLookup[name];

            if (Users.ContainsKey(name) == false)
            {
                Users[name] = new SlackUser() {
                    Name = userInfo.name,

                    Email = userInfo.profile.email,

                    IsBot = userInfo.is_bot,
                    IsMe = userInfo.id == Slack.MyData.id
                };
            }

			return Users[name];
		}
	}
}
