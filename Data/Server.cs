using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCQueryLib.Data
{
	// todo: add cancellation token support
	public class Server : IDisposable
	{
		public Server(SessionId sessionId, IPAddress host, int port)
		{
			UUID = Guid.NewGuid();
			SessionId = sessionId;
			Host = host;
			Port = port;
			ChallengeToken = new();
			UdpClient = new UdpClient(Host.ToString(), Port);
			UdpClientSemaphoreSlim = new SemaphoreSlim(0, 1);
			UdpClientSemaphoreSlim.Release();
		}

		public Guid UUID { get; }
		public SessionId SessionId { get; }
		public IPAddress Host { get; }
		public int Port { get; }
		public ChallengeToken ChallengeToken { get; }
		public UdpClient UdpClient { get; private set; }
		public SemaphoreSlim UdpClientSemaphoreSlim { get; }

		public async void InvalidateSocket()
		{
			await UdpClientSemaphoreSlim.WaitAsync();
			UdpClient.Dispose();
			UdpClient = new UdpClient(Host.ToString(), Port);
			UdpClientSemaphoreSlim.Release();
		}

		private bool disposed = false;
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if(!this.disposed)
			{
				if(disposing)
				{
					UdpClient.Dispose();
					UdpClientSemaphoreSlim.Dispose();
					ChallengeToken.Dispose();
				}

				disposed = true;
			}
		}

		~Server()
		{
			Dispose(disposing: true);
		}

		public override bool Equals(object obj)
		{
			return obj is Server server &&
				   EqualityComparer<SessionId>.Default.Equals(SessionId, server.SessionId);
		}

		public override int GetHashCode()
		{
			return SessionId.GetHashCode();
		}
	}
}
