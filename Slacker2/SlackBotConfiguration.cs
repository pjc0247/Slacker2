using System;

namespace Slacker2
{
	public class SlackBotConfiguration
	{
		public string AuthToken { get; set; }

		public int SchedulerResolution { get; set; }

		public SlackBotConfiguration()
		{
			SchedulerResolution = 5;
		}
	}
}
