using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCQueryLib.Data.Packages.Responses
{
	public class WrongResponse : IResponse
	{
		public WrongResponse(Guid serverUUID, byte[] rawData)
		{
			ServerUUID = serverUUID;
			RawData = rawData;
		}

		public byte[] RawData { get; }
		public Guid ServerUUID { get; }
		public string Message => "This response package can't be parsed";
	}
}
