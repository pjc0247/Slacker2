﻿using System;

namespace Slacker2.Models
{
	public class SlackMessage
	{
		internal SlackService Slack { get; set; }

		/// <summary>
		/// user who sent the message
		/// </summary>
		public SlackUser Sender { get; set; }
		public SlackChannel Channel { get; set; }

		public string Message { get; set; }

		public string Timestamp { get; set; }
		public string ThreadTimestamp { get; set; }

		public bool IsThreadMessage
		{
			get
			{
				return ThreadTimestamp != null;
			}
		}
	}
}
