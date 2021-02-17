using System;
using System.IO;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;

namespace MCQueryLib
{
    public class Request
    {
        public readonly byte[] Magic = {0xFE, 0xFD};

        public byte[] SessionId { get; }
        private byte[] _buffer;

        public byte[] GenerateSessionId()
        {
            byte[] buf = new byte[4];
            new Random().NextBytes(buf);
            if (BitConverter.IsLittleEndian)
                buf = buf.Reverse().ToArray();
            return BitConverter.GetBytes(BitConverter.ToInt32(buf,0) & 0x0F0F0F0F);
        }
        
        public Request(RequestType type, byte[] challengeToken = null)
        {
            SessionId = GenerateSessionId();

            switch (type)
            {
                case RequestType.Handshake:
                {
                    _buffer = new byte[7];
                    Buffer.BlockCopy(Magic, 0, _buffer, 0, 2);
                    _buffer[2] = (byte) PackageType.Challenge;
                    Buffer.BlockCopy(SessionId, 0, _buffer, 3, 4);
                    break;
                }
                
                case RequestType.BasicStats:
                {
                    if (challengeToken == null)
                        throw new Exception("Challenge token is not specified");
                    
                    _buffer = new byte[11];
                    Buffer.BlockCopy(Magic, 0, _buffer, 0, 2);
                    _buffer[2] = (byte) PackageType.Status;
                    Buffer.BlockCopy(SessionId, 0, _buffer, 3, 4);
                    Buffer.BlockCopy(challengeToken, 0, _buffer, 7, 4);
                    break;
                }
                
                case RequestType.FullStats:
                {
                    if (challengeToken == null)
                        throw new Exception("Challenge token is not specified");
                    
                    _buffer = new byte[15];
                    Buffer.BlockCopy(Magic, 0, _buffer, 0, 2);
                    _buffer[2] = (byte) PackageType.Status;
                    Buffer.BlockCopy(SessionId, 0, _buffer, 3, 4);
                    Buffer.BlockCopy(challengeToken, 0, _buffer, 7, 4);
                    byte[] padding = {0x00, 0x00, 0x00, 0x00};
                    Buffer.BlockCopy(padding, 0, _buffer, 11, 4);
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

        public void SendTo(UdpClient client)
        {
            client.Send(_buffer, _buffer.Length);
        }
    }
}