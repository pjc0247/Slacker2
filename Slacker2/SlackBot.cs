using System;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Slacker2
{
    using Models;

    public class SlackBot
    {
        public static SlackBotConfiguration Configuration { get; set; }

        private static Dictionary<Regex, SlackMessageHandler> Handlers { get; set; }
        private static Dictionary<TimeSpan, SlackScheduledTaskHandler> SchedulesTasks { get; set; }

        private static SlackService Slack { get; set; }

        private static DateTime LastScheduled { get; set; }

        private static void InitializeSchedulers()
        {
            SchedulesTasks = new Dictionary<TimeSpan, SlackScheduledTaskHandler>();

            var services = Assembly.GetEntryAssembly()
                    .GetTypes()
                    .Where(x => x.IsSubclassOf(typeof(BotService)));

            foreach (var service in services)
            {
                var inst = (BotService)Activator.CreateInstance(service);
                inst.Slack = Slack;

                var tasks = service
                    .GetMethods()
                    .Select(x => new SlackScheduledTaskHandler()
                    {
                        Handler = x,

                        ScheduleAttr = x.GetCustomAttribute<ScheduleAttribute>(),

                        ServiceInstance = inst
                    })
                    .Where(x => x.ScheduleAttr != null);

                foreach (var task in tasks)
                {
                    var interval = task.ScheduleAttr.Interval;

                    SchedulesTasks[interval] = task;
                }
            }

            var timer = new Timer(
                ExecuteScheduledTasks,
                null,
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(Configuration.SchedulerResolution));
            LastScheduled = DateTime.Now;
        }
        private static void ExecuteScheduledTasks(object _)
        {
            foreach (var pair in SchedulesTasks)
            {
                var originalInverval = pair.Key;
                var task = pair.Value;

                task.TicksLeft -= DateTime.Now - LastScheduled;

                if (task.TicksLeft <= TimeSpan.FromSeconds(0))
                {
                    task.TicksLeft = originalInverval;

                    task.Handler?.Invoke(task.ServiceInstance, new object[] { });
                }
            }

            LastScheduled = DateTime.Now;
        }

        private static void RegisterHandler(SlackMessageHandler handler)
        {
            if (handler.Pattern.GetGroupNames().Length != handler.Handler.GetParameters().Length)
            {
                Console.WriteLine($"`{handler.Pattern}` does not match to method signature({handler.Handler.ToString()}).");
                throw new InvalidOperationException("Invalid handler");
            }

            Handlers[handler.Pattern] = handler;
        }
        private static void InitializeHandlers()
        {
            Handlers = new Dictionary<Regex, SlackMessageHandler>();

            var services = Assembly.GetEntryAssembly()
                    .GetTypes()
                    .Where(x => x.IsSubclassOf(typeof(BotService)));

            foreach (var service in services)
            {
                var inst = (BotService)Activator.CreateInstance(service);
                inst.Slack = Slack;

                var subscribers = service
                    .GetMethods()
                    .Where(x => x.GetCustomAttribute<SubscribeAttribute>() != null)
                    .Select(x => new SlackMessageHandler()
                    {
                        Handler = x,

                        Pattern = new Regex(x.GetCustomAttribute<SubscribeAttribute>().Pattern),
                        Target = x.GetCustomAttribute<SubscribeAttribute>().Target,
                        Usage = x.GetCustomAttribute<UsageAttribute>()?.Message,
                        PermissionGroupName = x.GetCustomAttribute<NeedsPermissionAttribute>()?.PermissionGroupName,

                        ServiceInstance = inst
                    });

                foreach (var subscriber in subscribers)
                {
                    RegisterHandler(subscriber);
                }
            }
        }
        private static void InitializeSlack()
        {
            Slack = new SlackService(Configuration.AuthToken);

            Slack.OnSlackMessage = OnSlackMessage;
        }
        private static void OnSlackMessage(SlackMessage message)
        {
            Console.WriteLine($"[{message.Sender}] : {message.Message}");

            foreach (var handler in Handlers) 
            {
                var inst = handler.Value.ServiceInstance;
                var regex = handler.Key;
                var handlerInfo = handler.Value;
                var methodInfo = handler.Value.Handler;

                // SUBSCRIBER TARGET
                if (handlerInfo.Target == SubscribeTarget.All)
                    ; // skip
                if (handlerInfo.Target == SubscribeTarget.Myself &&
                    message.Sender.IsMe == false)
                    continue;
                if (handlerInfo.Target == SubscribeTarget.BotUser &&
                    message.Sender.IsBot == false)
                    continue;
                if (handlerInfo.Target == SubscribeTarget.OtherUser &&
                    message.Sender.IsMe)
                    continue;
                
                var matches = regex.Match(message.Message);

                if (matches.Success == false)
                    continue;
                if (handlerInfo.PermissionGroupName != null)
                {
                    if (message.Sender.Permissions.Contains(handlerInfo.PermissionGroupName) == false)
                        continue;
                }

                var parameters = methodInfo.GetParameters();
                var args = new List<object>();
                var errorContinue = false;

                try
                {
                    args.Add(message);
                    for (int i = 1; i < matches.Groups.Count; i++)
                    {
                        var type = parameters[i].ParameterType;

                        if (type == typeof(string))
                            args.Add(matches.Groups[i].Value);
                        else if (type == typeof(int))
                            args.Add(Convert.ToInt32(matches.Groups[i].Value));
                        else if (type == typeof(long))
                            args.Add(Convert.ToInt64(matches.Groups[i].Value));
                        else if (type == typeof(float))
                            args.Add(Convert.ToSingle(matches.Groups[i].Value));
                        else if (type == typeof(double))
                            args.Add(Convert.ToDouble(matches.Groups[i].Value));
                    }
                }
                catch (Exception e)
                {
                    if (handlerInfo.Usage != null)
                    {
                        Slack.SendMessage(message.Channel.Name, "@" + message.Sender + " : " + handlerInfo.Usage);
                    }
                    errorContinue = true;
                }

                if (errorContinue)
                    continue;
                try
                {
                    methodInfo?.Invoke(inst, args.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static void RegisterCommand(string pattern, SubscribeTarget target, BotService inst, MethodInfo method)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException(nameof(pattern));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var regex = new Regex(pattern);
            RegisterHandler(new SlackMessageHandler()
            {
                Pattern = regex,
                Target = target,

                ServiceInstance = inst,
                Handler = method
            });
        }
        public static void Run()
        {
            if (Configuration == null)
                throw new InvalidOperationException($"{nameof(Configuration)} -> null");
            
            InitializeSlack();
            InitializeSchedulers();
            InitializeHandlers();

            if (Handlers.Count == 0)
                throw new InvalidOperationException("No handlers found");
        }
    }
}
