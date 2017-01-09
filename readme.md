Slacker2
====

__Create your own Slackbot w/ C#.__<br>
<br>

Quick Guide
---
__Initialization__ 
```cs
SlackBot.Configuration = new SlackBotConfiguration()
{
    AuthToken = "xoxb-AAAA-BBBBBBBB"
};
SlackBot.Run();
```
You can find your AuthToken [here](https://api.slack.com/docs/oauth-test-tokens).

__Respond to messages__<br>
Creating a new bot command is very simple. You don't need to register your new commands manually.<br>
Just define a method and let __Slacker2__ know the method is a subscriber. __Slacker2__ will find your all subscribers automatically.<br>
<br>
The example below shows that how to create a simple bot command.
```cs
public class Program : BotService {

    // put a regular expr which you want to subscribe
    [Subscribe("^hello")]
    public void OnHello(SlackMessage message) {
        message.Reply("hi there");
    }
}
```

__Accept command parameters__<br>
Since __Slacker2__ accepts regex to subscribe messages, you can also use `capturing` function.<br>
Captured values will be bound to method's parameters in order.
```cs
[Subscribe("^echo (.+)$")]
public void OnEcho(SlackMessage message, string echo) {
    message.Reply(echo);
}

[Subscribe("^sum (.+) (.+)$")]
public void OnSum(SlackMessage message, int a, int b) {
    message.Reply($"{a} + {b} = {a + b}");
}
```


__Periodic tasks__<br>
If you need to execute a method in every specific minutes, just add a `Schedule` attribute on your method.<br>
__Slacker2__ will invoke the method repeatedly by internally managed timers.
```cs
[Schedule(1)] // interval (seconds)
public void OnTimer()
{
    Console.WriteLine("IM TIMER JOB");
}
```


