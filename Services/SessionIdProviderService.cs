using MCQueryLib.Data;
using MCQueryLib.Utills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCQueryLib.Services
{
	public class SessionIdProviderService
	{
		public SessionIdProviderService(Random random)
		{
			this.random = random;
			ReservedIds = new();
			IdCounter = new();
		}


		private readonly List<SessionId> ReservedIds;

		Random random;
		public SessionId GenerateRandomId()
		{
			byte[] sessionIdData = new byte[4];
			SessionId sessionId;

			do
			{
				random.NextBytes(sessionIdData);
				for (int i = 0; i < sessionIdData.Length; ++i)
				{
					sessionIdData[i] &= 0x0F;
				}

				sessionId = new(sessionIdData);
			}
			while (IsIdReserved(sessionId));

			ReserveId(sessionId);
			return sessionId;
		}


		private readonly ByteCounter IdCounter = new ByteCounter();

		public SessionId GetUinqueId()
		{
			byte[] sessionIdData = new byte[4];
			if (!IdCounter.GetNext(sessionIdData))
			{
				// find released sessionIds
			}

			SessionId sessionId = new(sessionIdData);
			ReserveId(sessionId);
			return sessionId;
		}

		private void ReserveId(SessionId sessionId)
		{
			ReservedIds.Add(sessionId);
		}

		public bool IsIdReserved(SessionId sessionId)
		{
			return ReservedIds.IndexOf(sessionId) != -1;
		}
	}
}
