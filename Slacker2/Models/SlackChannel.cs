using System;
using System.Collections.Generic;

namespace Slacker2.Models
{
	public class SlackChannel
	{
		public string Name { get; set; }

		public bool IsPublicOpened { get; set; }

		public string Topic { get; set; }

		public SlackUser[] Members { get; set; }
	}
}
