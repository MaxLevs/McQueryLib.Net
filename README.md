# McQueryLib.Net
.Net Core 5.0 library which implements Minecraft Query protocol. You can use it for getting statuses of servers.

WARNING! Right now library in manual mode which means you should re-request challengeToken by manual call GetHandshake(). It will be fixed in next versions.

# Example of using
```cs
public static async Task<ServerState> DoSomething(IPAddress host, int port) {
    var mcQuery = new McQuery(host, port, new Random());
    mcQuery.InitSocket();
    await mcQuery.GetHandshake();
    return await mcQuery.GetFullStatus();
}
```
