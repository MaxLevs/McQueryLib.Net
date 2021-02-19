using System.Collections.Generic;
using MCQueryLib.Data;

namespace MCQueryLib.Packages
{
    /// <summary>
    /// This class builds Minecraft Query Packages for requests
    /// Wiki: https://wiki.vg/Query
    /// </summary>
    public class Request
    {
        private static readonly byte[] Magic = { 0xfe, 0xfd };
        private static readonly byte[] Challenge = { 0x09 };
        private static readonly byte[] Status = { 0x00 };
        public readonly SessionId SessionId = SessionId.GenerateRandomId();
        public byte[] Data { get; private set; }
        
        private Request(){}

        public static Request GetHandshakeRequest()
        {
            var request = new Request();
            
            var data = new List<byte>();
            data.AddRange(Magic);
            data.AddRange(Challenge);
            data.AddRange(request.SessionId.GetBytes());
            
            request.Data = data.ToArray();
            return request;
        }

        public static Request GetBasicStatusRequest(byte[] challengeToken)
        {
            var request = new Request();
            
            var data = new List<byte>();
            data.AddRange(Magic);
            data.AddRange(Status);
            data.AddRange(request.SessionId.GetBytes());
            data.AddRange(challengeToken);
            
            request.Data = data.ToArray();
            return request;
        }
        
        public static Request GetFullStatusRequest(byte[] challengeToken)
        {
            var request = new Request();
            
            var data = new List<byte>();
            data.AddRange(Magic);
            data.AddRange(Status);
            data.AddRange(request.SessionId.GetBytes());
            data.AddRange(challengeToken);
            data.AddRange(new byte[] {0x00, 0x00, 0x00, 0x00}); // Padding
            
            request.Data = data.ToArray();
            return request;
        }
    }
}