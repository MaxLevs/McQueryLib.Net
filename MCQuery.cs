using System;
using System.Net;
using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace MCQueryLib
{
    public class McQuery
    {
        public IPEndPoint HostPort { get; }
        public UdpClient Client { get; private set; }
        public Int32 ChallengeToken { get; private set; }
        public bool Online { get; private set; } = false;
        
        public McQuery(IPAddress host, int port)
        {
            HostPort = new IPEndPoint(host, port);
            Client = new UdpClient();
            TryConnect();
        }

        public void TryConnect()
        {
            try
            {
                Client.Connect(HostPort);
            }
            
            catch (Exception)
            {
                Client.Close();
                Online = false;
                // retry connection later
            }
            
            Handshake();
        }

        public void Handshake()
        {
            // send package
            byte[] package = { 0xfe,0xfd,  (byte)PackageType.Challenge,  0x00,0x00,0x00,0x01 };
            Client.Send(package, package.Length);
            
            var ipEndPoint = HostPort;
            byte[] data = Client.Receive(ref ipEndPoint);
            // parse data and get challenge token
            ChallengeToken = Int32.Parse(new ASCIIEncoding().GetString(data, 5, data.Length - 6));
            
            Online = true;
        }

        public void GetSimpleStat()
        {
            
        }

        public void GetFullStat()
        {
            
        }
    }
}