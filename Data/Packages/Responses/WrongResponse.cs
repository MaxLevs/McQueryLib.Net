using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCQueryLib.Data.Packages.Responses
{
    public class WrongResponse : IResponse
    {
        public byte[] RawData { get; }
        public string Message => "This response package can't be parsed";

        public WrongResponse(byte[] rawData)
        {
            RawData = rawData;
        }
    }
}
