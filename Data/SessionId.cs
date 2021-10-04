using System;
using System.Linq;
using System.Collections.Generic;

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

        public static SessionId GenerateRandomId(Random rnd)
        {
            var sessionId = new byte[4];
            rnd.NextBytes(sessionId);
            for (var i = 0; i < sessionId.Length; ++i)
                sessionId[i] &= 0x0F;
            return new SessionId(sessionId);
        }

        public string GetString()
        {
            return BitConverter.ToString(_sessionId);
        }

        public byte[] GetBytes()
        {
            var sessionIdSnapshot = new byte[4];
            Buffer.BlockCopy(_sessionId, 0, sessionIdSnapshot, 0, 4);
            return sessionIdSnapshot;
        }

        public void WriteTo(List<byte> list)
        {
            list.AddRange(_sessionId);
        }

        public override bool Equals(object obj)
        {
            return obj is SessionId anotherSessionId && _sessionId.SequenceEqual(anotherSessionId._sessionId);
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(_sessionId, 0);
        }
    }
}