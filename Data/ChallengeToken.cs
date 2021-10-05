using System;
using System.Collections.Generic;

namespace MCQueryLib.Data
{
	public class ChallengeToken : IDisposable
	{
		private byte[] _challengeToken;
		private readonly int alifePeriod = 30000; // Milliseconds before revoking
		private DateTime revokeDateTime;
		public bool IsFine => _challengeToken != null && DateTime.Now < revokeDateTime;

		public ChallengeToken()
		{
			_challengeToken = null;
		}

		public ChallengeToken(byte[] challengeToken)
		{
			UpdateToken(challengeToken);
		}

		public void UpdateToken(byte[] challengeToken)
		{
			_challengeToken = (byte[]) challengeToken.Clone();
			revokeDateTime = DateTime.Now.AddMilliseconds(alifePeriod);
		}

		public string GetString()
		{
			return BitConverter.ToString(_challengeToken);
		}

		public byte[] GetBytes()
		{
			var challengeTokenSnapshot = new byte[4];
			Buffer.BlockCopy(_challengeToken, 0, challengeTokenSnapshot, 0, 4);
			return challengeTokenSnapshot;
		}

		public void WriteTo(List<byte> list)
		{
			list.AddRange(_challengeToken);
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

				}

				_challengeToken = null;

				disposed = true;
			}
		}

		~ChallengeToken()
		{
			Dispose(disposing: true);
		}
	}
}