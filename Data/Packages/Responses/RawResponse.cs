using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCQueryLib.Data.Packages.Responses
{
    public class RawResponse : IResponse
    {
        public byte[] RawData { get; }

        public RawResponse(byte[] rawData)
        {
            RawData = rawData;
        }
    }
}
