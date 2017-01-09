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
    AuthToken = "xoxb-42347798839-StUHhs5b16zCx9156PKa0u5c"
};
SlackBot.Run();
```
You can find your AuthToken [here](https://api.slack.com/docs/oauth-test-tokens).

__Respond to messages__
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


__Periodic tasks__
```cs
[Schedule(1)] // interval (seconds)
public void OnTimer()
{
    Console.WriteLine("IM TIMER JOB");
}
```
