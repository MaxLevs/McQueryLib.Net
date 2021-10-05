using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MCQueryLib.Data;
using MCQueryLib.Data.Packages;
using MCQueryLib.Data.Packages.Responses;

namespace MCQueryLib.Services
{
	public class McQueryService
	{

		public McQueryService(Random random, uint maxTriesBeforeSocketInvalidate, int receiveAwaitInterval, int retryAwayitShortInteval, int retryAwaitLongInterval)
		{
			sessionIdProviderService = new SessionIdProviderService(random);
			ServersTimeoutCounter = new();
			udpService = new UdpSendReceiveService(receiveAwaitInterval);

			MaxTriesBeforeSocketInvalidate = maxTriesBeforeSocketInvalidate;
			RetryAwaitShortInterval = retryAwayitShortInteval;
			RetryAwaitLongInterval = retryAwaitLongInterval;
		}

		public McQueryService(uint maxTriesBeforeSocketInvalidate, int receiveAwaitInterval, int retryAwayitShortInteval, int retryAwaitLongInterval) 
			: this(new Random(), maxTriesBeforeSocketInvalidate, receiveAwaitInterval, retryAwayitShortInteval, retryAwaitLongInterval)
		{
		}

		private readonly SessionIdProviderService sessionIdProviderService;
		private readonly UdpSendReceiveService udpService;
		private Dictionary<Server, int> ServersTimeoutCounter { get; set; }
		public uint MaxTriesBeforeSocketInvalidate { get; set; }
		public int RetryAwaitShortInterval { get; set; }
		public int RetryAwaitLongInterval { get; set; }

		public Server RegistrateServer(IPEndPoint serverEndPoint)
		{
			SessionId sessionId = sessionIdProviderService.GenerateRandomId();
			Server server = new(sessionId, serverEndPoint.Address, serverEndPoint.Port);
			ServersTimeoutCounter.Add(server, 0);
			return server;
		}

		public void DisposeServer(Server server)
		{
			ServersTimeoutCounter.Remove(server);
			server.Dispose();
		}

		private void ResetTimeoutCounter(Server server)
		{
			ServersTimeoutCounter[server] = 0;
		}

		private async Task InvalidateChallengeToken(Server server)
		{
			var request = RequestFormingService.HandshakeRequestPackage(server.SessionId);
			IResponse response;

			while(true)
			{
				response = await udpService.SendReceive(server, request);

				if (response is TimeoutResponse)
				{
					if(ServersTimeoutCounter[server] > MaxTriesBeforeSocketInvalidate)
					{
						var delayTask = Task.Delay(RetryAwaitLongInterval);

						server.InvalidateSocket();
						ResetTimeoutCounter(server);

						await delayTask;
						continue;
					}

					ServersTimeoutCounter[server]++;
					await Task.Delay(RetryAwaitShortInterval);
					continue;
				}

				break;
			}

			byte[] challengeToken = ResposeParsingService.ParseHandshake((RawResponse) response);

			server.ChallengeToken.UpdateToken(challengeToken);
		}

		public async Task<ServerBasicStateResponse> GetBasicStatus(Server server)
		{
			if (!server.ChallengeToken.IsFine)
				await InvalidateChallengeToken(server);

			IResponse response;

			while (true)
			{
				var request = RequestFormingService.GetBasicStatusRequestPackage(server.SessionId, server.ChallengeToken);
				response = await udpService.SendReceive(server, request);

				if (response is TimeoutResponse)
				{
					if(ServersTimeoutCounter[server] > MaxTriesBeforeSocketInvalidate)
					{
						var delayTask = Task.Delay(RetryAwaitLongInterval);

						server.InvalidateSocket();
						var invalidateTask = InvalidateChallengeToken(server);
						ResetTimeoutCounter(server);

						Task.WaitAll(new Task[] { delayTask, invalidateTask });
						continue;
					}

					ServersTimeoutCounter[server]++;
					await Task.Delay(RetryAwaitShortInterval);
					continue;
				}

				break;
			}

			var basicStateResponse = ResposeParsingService.ParseBasicState((RawResponse)response);
			return basicStateResponse;
		}

		public async Task<ServerFullStateResponse> GetFullStatus(Server server)
		{
			if (!server.ChallengeToken.IsFine)
				await InvalidateChallengeToken(server);

			IResponse response;

			while(true)
			{
				var request = RequestFormingService.GetFullStatusRequestPackage(server.SessionId, server.ChallengeToken);
				response = await udpService.SendReceive(server, request);

				if (response is TimeoutResponse)
				{
					if(ServersTimeoutCounter[server] > MaxTriesBeforeSocketInvalidate)
					{
						var delayTask = Task.Delay(RetryAwaitLongInterval);

						server.InvalidateSocket();
						var invalidateTask = InvalidateChallengeToken(server);
						ResetTimeoutCounter(server);

						Task.WaitAll(new Task[] { delayTask, invalidateTask });
						continue;
					}

					ServersTimeoutCounter[server]++;
					await Task.Delay(RetryAwaitShortInterval);
					continue;
				}

				break;
			}

			var fullStateResponse = ResposeParsingService.ParseFullState((RawResponse)response);
			return fullStateResponse;
		}
	}
}
