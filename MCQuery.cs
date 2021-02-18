using System;
using System.Net;
using System.Buffers;
using System.Buffers.Binary;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MCQueryLib
{
    public class McQuery
    {
        public IPEndPoint HostPort { get; }
        public UdpClient Client { get; private set; }
        public byte[] ChallengeToken { get; private set; }
        public bool Online { get; private set; } = false;
        
        public McQuery(IPAddress host, int port)
        {
            HostPort = new IPEndPoint(host, port);
            Client = new UdpClient();
            TryConnect();
        }

        public void TryConnect()
        {
            bool connected = false;
            while (!connected)
            {
                try
                {
                    Console.WriteLine($"[INFO] Trying to connect to {HostPort}");
                    Client.Connect(HostPort);
                    connected = true;
                }
                
                catch (SocketException)
                {
                    connected = false;
                    Client.Close();
                    Online = false;
                }
                
                Thread.Sleep(5000);
            }
            
            Handshake();
        }

        public void Handshake()
        {
            Console.WriteLine($"[INFO] Handshake with {HostPort}");
            var req = new Request(RequestType.Handshake);
            byte[] package = req.GetBytes();
            try
            {
                Client.Send(package, package.Length);
                var ipEndPoint = HostPort;
                var data = Client.Receive(ref ipEndPoint);
                ChallengeToken = Response.ParseChallenge(data).ChallengeToken;
                Console.WriteLine($"[INFO] [{HostPort}] Received ChallengeToken: {BitConverter.ToString(ChallengeToken)}");
            }
            catch (SocketException)
            {
                Online = false;
                TryConnect();
                return;
            }
            
            Online = true;
        }

        public State GetBasicStat()
        {
            Console.WriteLine($"[INFO] [{HostPort}] Sending basic status request...");
            var req = new Request(RequestType.BasicStats, ChallengeToken);
            byte[] package = req.GetBytes();
            try
            {
                Client.Send(package, package.Length);

                var ipEndPoint = HostPort;
                var data = Client.Receive(ref ipEndPoint);

                Console.WriteLine($"[INFO] [{ipEndPoint}] Received basic status");
                return Response.ParseBasicState(data);
            }
            catch (SocketException)
            {
                Online = false;
                Handshake();
                return GetBasicStat();
            }
        }

        public State GetFullStat()
        {
            Console.WriteLine($"[INFO] [{HostPort}] Sending full status request...");
            var req = new Request(RequestType.FullStats, ChallengeToken);
            byte[] package = req.GetBytes();
            try
            {
                Client.Send(package, package.Length);

                var ipEndPoint = HostPort;
                var data = Client.Receive(ref ipEndPoint);

                Console.WriteLine($"[INFO] [{ipEndPoint}] Received full status");
                return Response.ParseFullState(data);
            }
            catch (SocketException)
            {
                Online = false;
                Handshake();
                return GetFullStat();
            }
        }
    }
}