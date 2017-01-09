using System;

namespace Slacker2
{
	using Models;

	public static class SlackMessageExt
	{
		public static void Reply(this SlackMessage _this, string message)
		{
			_this.Slack.SendMessage(
				_this.Channel.Name,
				message);
		}
	}
}
