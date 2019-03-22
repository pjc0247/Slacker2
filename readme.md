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

// `SlackBot.Run` does not block the program.
// You have to hold your bot yourself.
Console.ReadLine();
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

Interactive Message
----
```
Currently, Slacker2 only supports `buton` type messages.
```
__Requirements__
__Slack__ requires a __https__ server for interactive messages. we detour this using `AWS.SQS` and `AWS.Labmda`.<br>
```
TODO
```

If you can manage a __https__ server yourself, you may want to use it without AWS stuffs.<br>
However, We don't have flexibilities for it. You have to implement yourself.<br>
__[Please edit these lines of code](https://github.com/pjc0247/Slacker2/blob/master/Slacker2/SlackService.cs#L191-L204)__

__Send a message that includes some buttons__
```cs
var choice = await SendActionMessageAndWait(
    "channel", "choose one of the following items",
    new SlackInteractiveMessage() {
        Buttons = new SlackActionButton[] {
            new SlackActionButton() {
                Name = "orange", Text = "Give me an Orange!"
            },
            new SlackActionButton() {
                Name = "banana", Text = "Give me a Banana!"
            }
        }
    });
    
if (choice == "orange") { /* ... */ }
else if (choice == "banana") { /* .... */ }
```

Advanced Features
----
__User permission__<br>
Sometimes, you may want to make dangerous functions. Such as changing machine's state or handling your private data.<br>
__Slacker2__ also provides a permission management feature. You can add the required permissions on your commands and __grant__/__remove__ each permissions to the right users.

```cs
[Subscribe("^!sys_shutdown$")]
[NeedsPermission("SYSTEM_SHUTDOWN")]
public void OnShutdown(SlackMessage message) {
    /* shutdown machine */
}
```
```cs
GrantPermission(user, "SYSTEM_SHUTDOWN");
// now `user` can invoke `!sys_shutdown` method.
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
