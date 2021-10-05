using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCQueryLib.Data.Packages.Responses
{
	public class RawResponse : IResponse
	{
		public RawResponse(Guid serverUUID, byte[] rawData)
		{
			ServerUUID = serverUUID;
			RawData = rawData;
		}

		public Guid ServerUUID { get; }
		public byte[] RawData { get; }
	}
}
