using System;
using System.Collections.Generic;

namespace Slacker2
{
	public class SlackInteractiveMessage
	{
		public string Text { get; set; }
		public string Description { get; set; }
		public string CallbackId { get; set; }

		public SlackActionButton[] Buttons { get; set; }
	}
}
