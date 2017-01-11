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
Creating a new bot command is very simple. Even you don't need to register your commands manually.<br>
Just define a method and let __Slacker2__ know the method is a subscriber. __Slacker2__ will find all your subscribers automatically.<br>
<br>
The example below shows that how to make a basic bot command.
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
Since `Subscribe` attribute accepts a regular expressions, you can also use `capturing` function.<br>
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
    Console.WriteLine("Minsoo chunjaeya");
}
```


Advanced Features
----
__User permission__<br>
Sometimes, you may want to make dangerous functions. Such as changing machine's state or handling your private data.<br>
__Slacker2__ also provides a permission management feature. You can add the required permissions on your commands and __grant__/__remove__ each permissions to the right users.

```cs
GrantPermission(user, "SYSTEM_SHUTDOWN");
// now `user` can invoke `!sys_shutdown` method.
```
```cs
[Subscribe("^!sys_shutdown$")]
[NeedsPermission("SYSTEM_SHUTDOWN")]
public void OnShutdown(SlackMessage message) {
    /* shutdown machine */
}
```

User permissions can be evaluated at method's body instead `NeedsPermission` attribute. 
```cs
var user = message.Sender;

if (user.Permissions.Contains("SYSTEM_SHUTDOWN"))
    ; /* shutdown machine */
else
    message.Reply("Sorry, but you don't have the proper permission.");    
```


Integrations w/ Slacker2
----
* [AWS.CloudWatch](https://github.com/pjc0247/Slacker2.CloudWatcher)
