using System;
using System.Collections.Generic;

namespace Slacker2.Models
{
	public class SlackUser
	{
		public string Name { get; set; }

        public string Email { get; set; }

        public bool IsBot { get; set; }
        public bool IsMe { get; set; }

		public HashSet<string> Permissions { get; }

		public SlackUser()
		{
			Permissions = new HashSet<string>();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
