# McQueryLib.Net
.Net Core 5.0 library which implements Minecraft Query protocol. You can use it for getting statuses of servers.

WARNING! Right now library in manual mode which means you should re-request challengeToken by manual call GetHandshake(). It will be fixed in next versions.

# Example of using
```cs
public static async void DoSomething(IEnumerable<IpEndPoint> mcServersQueryEndPoints) {
	McQueryService service = new(5, 5000, 500, 1000);

	var servers = mcServersQueryEndPoints.Select(service.RegistrateServer).ToList();

	List<Task> tasks = new();

	foreach(var server in servers)
	{
		tasks.Add(service.GetBasicStatus(server));
		tasks.Add(service.GetFullStatus(server));
	}

	Task.WaitAll(tasks.ToArray());

	foreach(Task<IResponse> task in tasks)
	{
		Console.WriteLine((await task).ToString());
		Console.WriteLine();
	}
}
```
