using System;

namespace Slacker2
{
    public enum SubscribeTarget
    {
        /// <summary>
        /// all users including myself
        /// </summary>
        All,
        Myself,
        BotUser,
        OtherUser
    }

	public class SubscribeAttribute : Attribute
	{
		public string Pattern { get; }
        public SubscribeTarget Target { get; }

		public SubscribeAttribute(string pattern)
		{
			Pattern = pattern;
            Target = SubscribeTarget.OtherUser;
		}
        public SubscribeAttribute(string pattern, SubscribeTarget target)
        {
            Pattern = pattern;
            Target = target;
        }
	}
}
