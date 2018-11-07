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
				// datetime -> slack_ts
				string tsString = ((message.ts.ToUniversalTime().Ticks - 621355968000000000m) / 10000000m).ToString("0.000000");
				string threadTsString = null;

				if (message.thread_ts != default(DateTime))
					threadTsString = ((message.thread_ts.ToUniversalTime().Ticks - 621355968000000000m) / 10000000m).ToString("0.000000");
				
				OnSlackMessage?.Invoke(new SlackMessage()
				{
					Slack = this,

					Channel = GetChannel(message.channel),
					Sender = GetUser(message.user),
					Message = message.text,

					Timestamp = tsString,
					ThreadTimestamp = threadTsString
				});
			}
			catch (Exception e) { Console.WriteLine(e); }
		}

		public Task AddReaction(string channel, string messageTimestamp, string reactionName)
		{
            var ts = new TaskCompletionSource<int>();

            if (reactionName.StartsWith(":") && reactionName.EndsWith(":"))
                reactionName = reactionName.Substring(1, reactionName.Length - 2);

			Slack.AddReaction(
				_ => { ts.SetResult(1); },
				reactionName,
				channel,
				messageTimestamp);

            return ts.Task;
		}
		public Task<SlackMessage> SendThreadMessage(string channel, string messageTimestamp, string message)
		{
            var ts = new TaskCompletionSource<SlackMessage>();

            Slack.PostMessage(
				_ => { ProcessCompletion(ts, channel, _); },
				channel,
				message,
				as_user: true,
				thread_ts: messageTimestamp);

            return ts.Task;
		}

		public Task<SlackMessage> SendMessage(string channel, string message)
		{
			Console.WriteLine("[Send] " + message);

            var ts = new TaskCompletionSource<SlackMessage>();

			Slack.PostMessage(
				_ => { ProcessCompletion(ts, channel, _); },
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

		public Task<SlackMessage> SendColoredMessage(string channel, string message, string colorHex, string title, string description)
		{
            var ts = new TaskCompletionSource<SlackMessage>();

            Slack.PostMessage(
				_ => { ProcessCompletion(ts, channel, _); },
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

            return ts.Task;
		}
		public Task<SlackMessage> SendActionMessage(string channel, string message, SlackInteractiveMessage messageData)
		{
            var ts = new TaskCompletionSource<SlackMessage>();
            var actions = new List<AttachmentAction>();

            foreach (var button in messageData.Buttons)
                actions.Add(new AttachmentAction(button.Name, button.Text));

			Slack.PostMessage(
				_ => { ProcessCompletion(ts, channel, _); },
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

            return ts.Task;
		}

        private void ProcessCompletion(TaskCompletionSource<SlackMessage> ts, string channel, PostMessageResponse msg)
        {
            try
            {
                ts.SetResult(SlackMessage.Create(this, channel, msg.message));
            }
            catch (Exception e)
            {
                ts.SetException(e);
                Console.WriteLine(e);
            }
        }

        public SlackChannel GetChannel(string channel)
        {
            Channel ch = null;

            if (Slack.DirectMessageLookup.ContainsKey(channel))
                return null; // TODO
            else if (Slack.GroupLookup.ContainsKey(channel))
                ch = Slack.GroupLookup[channel];
            else if (Slack.ChannelLookup.ContainsKey(channel))
                ch = Slack.ChannelLookup[channel];
            else if (Slack.Groups.Any(x => x.name == channel))
                ch = Slack.Groups.First(x => x.name == channel);
            else if (Slack.Channels.Any(x => x.name == channel))
                ch = Slack.Channels.First(x => x.name == channel);
            else
                return null;

            var members = ch.members
                .Select(x => GetUser(x))
                .Where(x => x != null).ToArray();

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
            if (Slack.UserLookup.ContainsKey(name) == false)
                return null;

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
