using System;
using System.Linq;

namespace MCQueryLib.Data
{
    /// <summary>
    /// This class represents SessionId filed into packages.
    /// It provides api for create random SessionId or parse it from byte[]
    /// </summary>
    public class SessionId
    {
        private readonly byte[] _sessionId;

        public SessionId (byte[] sessionId)
        {
            _sessionId = sessionId;
        }

        public static SessionId GenerateRandomId()
        {
            var sessionId = new byte[4];
            new Random().NextBytes(sessionId);
            sessionId = sessionId.Select(@byte => (byte)(@byte & 0x0F)).ToArray();
            return new SessionId(sessionId);
        }

        public string GetString()
        {
            return BitConverter.ToString(_sessionId);
        }

        public byte[] GetBytes()
        {
            var sessionId = new byte[4];
            Buffer.BlockCopy(_sessionId, 0, sessionId, 0, 4);
            return sessionId;
        }
    }
}