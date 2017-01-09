using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;

using SlackAPI;
using SlackAPI.WebSocketMessages;

namespace Slacker2
{
	class SlackHandler
	{
		public MethodInfo Handler { get; set; }
		public BotService ServiceInstance { get; set; }
	}
	class SlackMessageHandler : SlackHandler
	{
		public SubscribeAttribute SubscribeAttr { get; set; }
		public UsageAttribute UsageAttr { get; set; }
		public NeedsPermissionAttribute PermissionAttr { get; set; }
	}
	class SlackScheduledTaskHandler : SlackHandler
	{
		public ScheduleAttribute ScheduleAttr { get; set; }

		public TimeSpan TicksLeft { get; set; }
	}
}