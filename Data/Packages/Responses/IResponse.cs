using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCQueryLib.Data.Packages.Responses
{
	public interface IResponse
	{
		public Guid ServerUUID { get; }
		public byte[] RawData { get; }
	}
}
