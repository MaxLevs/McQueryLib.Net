using System;
using System.Net;
using System.Buffers;
using System.Buffers.Binary;
using System.Linq;
using System.Net.Sockets;
using System.Text;

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
            var req = new Request(RequestType.Handshake);
            byte[] package = req.GetBytes();
            try
            {
                Client.Send(package, package.Length);
            }
            catch (Exception e)
            {
                // retry connection later
            }
            
            var ipEndPoint = HostPort;
            var data = Client.Receive(ref ipEndPoint);
            ChallengeToken = Response.ParseChallenge(data).ChallengeToken;
            
            Online = true;
        }

        public State GetBasicStat()
        {
            var req = new Request(RequestType.BasicStats, ChallengeToken);
            byte[] package = req.GetBytes();
            try
            {
                Client.Send(package, package.Length);
            }
            catch (Exception e)
            {
                Online = false;
                Handshake();
                return GetBasicStat();
            }

            var ipEndPoint = HostPort;
            var data = Client.Receive(ref ipEndPoint);

            return Response.ParseBasicState(data);
        }

        public State GetFullStat()
        {
            var req = new Request(RequestType.FullStats, ChallengeToken);
            byte[] package = req.GetBytes();
            try
            {
                Client.Send(package, package.Length);
            }
            catch (Exception e)
            {
                Online = false;
                Handshake();
                return GetFullStat();
            }

            var ipEndPoint = HostPort;
            var data = Client.Receive(ref ipEndPoint);

            return Response.ParseFullState(data);
        }
    }
}