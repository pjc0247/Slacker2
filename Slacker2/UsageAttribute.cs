using System;

namespace Slacker2
{
    public class UsageAttribute : Attribute
    {
        public string Message { get; set; }

        public UsageAttribute(string message)
        {
            Message = message;
        }
    }
}
