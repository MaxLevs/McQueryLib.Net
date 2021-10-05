using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCQueryLib.Data.Packages.Responses
{
	public class TimeoutResponse : IResponse
	{
		public TimeoutResponse(Guid serverUUID)
		{
			ServerUUID = serverUUID;
		}

		public byte[] RawData => throw new NotSupportedException();
		public Guid ServerUUID { get; }
		public string Message => "Request is timed out";
	}
}
