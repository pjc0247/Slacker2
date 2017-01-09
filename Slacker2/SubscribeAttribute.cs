using System;

namespace Slacker2
{
	public class SubscribeAttribute : Attribute
	{
		public string Pattern { get; }

		public SubscribeAttribute(string pattern)
		{
			Pattern = pattern;
		}
	}
}
