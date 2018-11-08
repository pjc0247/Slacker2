using System;
namespace Slacker2
{
    public class ScheduleAttribute : Attribute
    {
        public TimeSpan Interval { get; set; }

        public ScheduleAttribute(int interval)
        {
            Interval = TimeSpan.FromSeconds(interval);
        }
    }
}
