using System;
using System.Collections.Generic;

using Slacker2;
using Slacker2.Models;

namespace SampleBotApp
{
	public class Program : BotService
	{
		[Subscribe("test (.+)")]
		[Usage("__Usage__\r\n  test param: long")]
		[NeedsPermission("HELP")]
		public void OnTest(SlackMessage message, long param)
		{ 
			Console.WriteLine("OnTEst " + param);

			Console.WriteLine(message.Channel);

			SendMessage(message.Channel, "Hi");
		}

		[Subscribe("^perm (.+)")]
		public void OnPerm(SlackMessage message, string permissionName)
		{
			GrantPermission(message.Sender, permissionName);

			SendMessage(message.Channel, "PERMISSINON");
		}

		[Subscribe("^sum (.+) (.+)$")]
		public void OnSum(SlackMessage message, int a, int b)
		{
			message.Reply($"{a} + {b} = {a + b}");
		}

		[Subscribe("^aa")]
		public void OnAA(SlackMessage message)
		{
			foreach (var user in message.Channel.Members)
			{
				Console.WriteLine(user.Name);
			}

			SendActionMessage(message.Channel, "AA");
		}

		[Schedule(1)]
		public void OnTimer()
		{
			Console.WriteLine("IM TIMER JOB");
		}

		static void Main(string[] args)
		{
			/*
			var url = SlackAPI.SlackClient.GetAuthorizeUri(
				"41105373671.102273797813",
				SlackScope.Post | SlackScope.Client | SlackScope.Read);
			System.Diagnostics.Process.Start(url.ToString());

			var code = Console.ReadLine();
			SlackAPI.SlackClient.GetAccessToken(
				(response) =>
				{
					Console.WriteLine(response.error);
				Console.WriteLine(response.access_token);
				},
				"41105373671.102273797813",
				"c10342c1f35f75a3cf3ca087f5547c43",
				"https://Functionsa0f1f77a.azurewebsites.net/api/HttpTriggerCSharp1?code=8d94a123103adc9f62f1a38bd18dccc8b19e58f8",
				code);
*/


			SlackBot.Configuration = new SlackBotConfiguration()
			{
				AuthToken = "xoxb-42347798839-StUHhs5b16zCx9156PKa0u5c"
			};
			SlackBot.Run();

			/*
			var http = new System.Net.HttpListener();
			http.Prefixes.Add("http://+:9916/test/");
			http.Start();

			var ctx = http.GetContext();

			Console.WriteLine("GOT CONNECTION");
			Console.WriteLine(ctx.Request.Url);
*/

			while (true)
				Console.ReadLine();
		}

	}
}

