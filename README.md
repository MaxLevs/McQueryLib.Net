# McQueryLib.Net
Library for .Net Core 5.0 which implements Minecraft Query protocol. You can use it for getting statuses of servers.

# Example of using
```cs
public static async void DoSomething(IEnumerable<IpEndPoint> mcServersQueryEndPoints) {
	McQueryService service = new(5, 5000, 500, 1000);

	var servers = mcServersQueryEndPoints.Select(service.RegistrateServer).ToList();

	List<Task> tasks = new();

	foreach(var server in servers)
	{
		tasks.Add(service.GetBasicStatusCommon(server));
		tasks.Add(service.GetFullStatusCommon(server));
	}

	Task.WaitAll(tasks.ToArray());

	foreach(Task<IResponse> task in tasks)
	{
		Console.WriteLine((await task).ToString());
		Console.WriteLine();
	}
}
```
