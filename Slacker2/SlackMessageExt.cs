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

		public static Task Reply(this SlackMessage _this, string message)
		{
			return _this.Slack.SendMessage(
				_this.Channel.Name,
				message);
		}

		public static Task ReplyThreadMessage(this SlackMessage _this, string message)
		{
			return _this.Slack.SendThreadMessage(
				_this.Channel.Name,
				_this.Timestamp,
				message);
		}

		public static Task AddReaction(this SlackMessage _this, string reactionName)
		{
			return _this.Slack.AddReaction(
				_this.Channel.Id,
				_this.Timestamp,
				reactionName);
		}
	}
}
