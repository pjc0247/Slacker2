using System;
using System.Threading.Tasks;

namespace Slacker2
{
	using Models;

	public static class SlackMessageExt
	{
        public static Task Update(this SlackMessage _this, string messageToUpdate)
        {
            return _this.Slack.UpdateMessage(
                _this.Channel.Id, _this.Timestamp,
                messageToUpdate);
        }

		public static void Reply(this SlackMessage _this, string message)
		{
			_this.Slack.SendMessage(
				_this.Channel.Name,
				message);
		}

		public static void ReplyThreadMessage(this SlackMessage _this, string message)
		{
			_this.Slack.SendThreadMessage(
				_this.Channel.Name,
				_this.Timestamp,
				message);
		}

		public static void AddReaction(this SlackMessage _this, string reactionName)
		{
			_this.Slack.AddReaction(
				_this.Channel.Id,
				_this.Timestamp,
				reactionName);
		}
	}
}
