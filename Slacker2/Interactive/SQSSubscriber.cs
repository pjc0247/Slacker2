using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json.Linq;

namespace Slacker2
{
    internal class SQSSubscriber
    {
        private static Thread worker;

        private static ConcurrentDictionary<string, Action<string>> pendingActions = new ConcurrentDictionary<string, Action<string>>();

        public static void Initialize(SlackBotConfiguration config)
        {
            worker = new Thread(() =>
            {
                Worker(config);
            });
            worker.Start();
        }
        public static void WaitFor(string key, Action<string> callback)
        {
            pendingActions[key] = callback;
        }

        private static void Worker(SlackBotConfiguration config)
        {
            var client = new AmazonSQSClient(
                config.AWSAccessKey, config.AWSAccessSecret, 
                Amazon.RegionEndpoint.APNortheast2);

            while (true)
            {
                try
                {
                    var req = new ReceiveMessageRequest();
                    req.QueueUrl = config.SQSUrl;
                    var resp = client.ReceiveMessage(req);

                    foreach (var msg in resp.Messages)
                    {
                        var payload = msg.Body.Split(
                            new string[] { "payload=" }, 2,
                            StringSplitOptions.None)[1];
                        var jobj = JObject.Parse(payload);

                        var key = jobj["callback_id"].ToString();
                        var selected = jobj["actions"].ToArray()[0]["name"].ToString();

                        if (pendingActions.ContainsKey(key))
                        {
                            pendingActions[key].Invoke(selected);
                            pendingActions.TryRemove(key, out _);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
