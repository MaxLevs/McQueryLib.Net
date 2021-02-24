using System;
using System.Collections.Generic;

namespace MCQueryLib.Data
{
    public class ChallengeToken
    {
        private readonly byte[] _challengeToken;

        public ChallengeToken(byte[] challengeToken)
        {
            _challengeToken = challengeToken;
        }

        public string GetString()
        {
            return BitConverter.ToString(_challengeToken);
        }

        public byte[] GetBytes()
        {
            var challengeTokenSnapshot = new byte[4];
            Buffer.BlockCopy(_challengeToken, 0, challengeTokenSnapshot, 0, 4);
            return challengeTokenSnapshot;
        }

        public void WriteTo(List<byte> list)
        {
            list.AddRange(_challengeToken);
        }
    }
}