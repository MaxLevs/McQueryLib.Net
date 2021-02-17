using System;
using System.IO;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;

namespace MCQueryLib
{
    public class Request
    {
        public readonly byte[] Magic = {0xFE, 0xFD};

        public Int32 SessionId { get; }
        private byte[] _buffer;

        public Int32 GenerateSessionId()
        {
            byte[] buf = new byte[4];
            new Random().NextBytes(buf);
            return BitConverter.ToInt32(buf,0) & 0x0F0F0F0F;
        }
        
        public Request(RequestType type, byte[] challengeToken, byte[] payload)
        {
            SessionId = GenerateSessionId();

            switch (type)
            {
                case RequestType.Handshake:
                {
                    _buffer = new byte[7];
                    break;
                }
                
                case RequestType.BasicStats:
                {
                    _buffer = new byte[11];
                    break;
                }
                
                case RequestType.FullStats:
                {
                    _buffer = new byte[15];
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public byte[] GetBytes()
        {
            var res = new byte[_buffer.Length];
            Buffer.BlockCopy(_buffer, 0, res, 0, _buffer.Length);
            return res;
        }
    }
}