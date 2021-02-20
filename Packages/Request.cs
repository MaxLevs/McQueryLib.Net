#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
        public byte[] Data { get; private set; }
        
        private Request(){}

        public byte RequestType => Data[2];

        public static Request GetHandshakeRequest(SessionId sessionId)
        {
            var request = new Request();
            
            var data = new List<byte>();
            data.AddRange(Magic);
            data.AddRange(Challenge);
            data.AddRange(sessionId.GetBytes());
            
            request.Data = data.ToArray();
            return request;
        }

        public static Request GetBasicStatusRequest(SessionId sessionId, byte[] challengeToken)
        {
            if (challengeToken == null)
            {
                throw new ChallengeTokenIsNullException();
            }
                
            var request = new Request();
            
            var data = new List<byte>();
            data.AddRange(Magic);
            data.AddRange(Status);
            data.AddRange(sessionId.GetBytes());
            data.AddRange(challengeToken);
            
            request.Data = data.ToArray();
            return request;
        }
        
        public static Request GetFullStatusRequest(SessionId sessionId, byte[] challengeToken)
        {
            if (challengeToken == null)
            {
                throw new ChallengeTokenIsNullException();
            }
            
            var request = new Request();
            
            var data = new List<byte>();
            data.AddRange(Magic);
            data.AddRange(Status);
            data.AddRange(sessionId.GetBytes());
            data.AddRange(challengeToken);
            data.AddRange(new byte[] {0x00, 0x00, 0x00, 0x00}); // Padding
            
            request.Data = data.ToArray();
            return request;
        }
    }

    public class ChallengeTokenIsNullException : Exception
    {
        public ChallengeTokenIsNullException()
        {
        }

        protected ChallengeTokenIsNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ChallengeTokenIsNullException(string? message) : base(message)
        {
        }

        public ChallengeTokenIsNullException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}