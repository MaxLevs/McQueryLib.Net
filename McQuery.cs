using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MCQueryLib.Data;
using MCQueryLib.Packages;
using MCQueryLib.State;
using UdpExtension;

namespace MCQueryLib
{
    /// <summary>
    /// This class provides api for getting status data from Minecraft server via Minecraft Server Query protocol.
    /// It doesn't manage networking. You should create your own manager for your own needs
    /// </summary>
    public class McQuery
    {
        public IPAddress Host { get; }
        public int Port { get; }
        public SessionId SessionId { get; } = SessionId.GenerateRandomId();
        
        private UdpClient _udpClient = null;
        public int ResponseWaitIntervalSecond = 10;
        
        private object _challengeTokenLock = new();
        private byte[] _challengeToken = null;

        public bool IsOnline { get; set; } = true;

        private void SetChallengeToken(byte[] challengeToken)
        {
            lock (_challengeTokenLock)
            {
                _challengeToken ??= new byte[4];
                Buffer.BlockCopy(challengeToken, 0, _challengeToken, 0, 4);
            }
        }

        private byte[] GetChallengeToken()
        {
            lock (_challengeTokenLock)
            {
                if (_challengeToken == null) 
                    return null;
            }
                
            var challengeToken = new byte[4];
            
            lock (_challengeTokenLock)
            {
                Buffer.BlockCopy(_challengeToken, 0, challengeToken, 0, 4);
            }

            return challengeToken;
        }
        
        public McQuery(IPAddress host, int queryPort)
        {
            Host = host;
            Port = queryPort;
        }

        public void InitSocket()
        {
            _udpClient?.Dispose();
            _udpClient = null;
            _udpClient = new UdpClient(Host.ToString(), Port);
        }

        public async Task<byte[]> GetHandshake()
        {
            if (_udpClient == null)
                throw new McQuerySocketIsNotInitialised(this);
            
            Request handshakeRequest = Request.GetHandshakeRequest(SessionId);
            var response = await SendResponseService.SendReceive(_udpClient, handshakeRequest.Data, ResponseWaitIntervalSecond);

            // todo: causes package to skip. Rewrite to automatic pick response by raw
            if (handshakeRequest.RequestType != Response.ParseType(response))
                throw new McQueryWrongResponseException(handshakeRequest, response);

            var challengeToken = Response.ParseHandshake(response);
            SetChallengeToken(challengeToken);
            
            return challengeToken;
        }

        public async Task<ServerBasicState> GetBasicStatus()
        {
            if (_udpClient == null)
                throw new McQuerySocketIsNotInitialised(this);

            if (!IsOnline)
                throw new McQueryServerIsOffline(this);

            var challengeToken = GetChallengeToken();
            
            if (challengeToken == null)
                throw new McQueryServerIsOffline(this);
            
            Request basicStatusRequest = Request.GetBasicStatusRequest(SessionId, challengeToken);
            var response = await SendResponseService.SendReceive(_udpClient, basicStatusRequest.Data, ResponseWaitIntervalSecond);
            
            // todo: causes package to skip. Rewrite to automatic pick response by raw
            if (basicStatusRequest.RequestType != Response.ParseType(response))
                throw new McQueryWrongResponseException(basicStatusRequest, response);
            
            var basicStatus = Response.ParseBasicState(response);
            return basicStatus;
        }
        
        public async Task<ServerFullState> GetFullStatus()
        {
            if (_udpClient == null)
                throw new McQuerySocketIsNotInitialised(this);

            if (!IsOnline)
                throw new McQueryServerIsOffline(this);

            var challengeToken = GetChallengeToken();
            
            Request fullStatusRequest = Request.GetFullStatusRequest(SessionId, challengeToken);
            var response = await SendResponseService.SendReceive(_udpClient, fullStatusRequest.Data, ResponseWaitIntervalSecond);

            // todo: causes package to skip. Rewrite to automatic pick response by raw
            if (fullStatusRequest.RequestType != Response.ParseType(response))
                throw new McQueryWrongResponseException(fullStatusRequest, response);
            
            var fullStatus = Response.ParseFullState(response);
            return fullStatus;
        }
    }

    public class McQueryException : Exception
    {
        public McQueryException()
        {
        }

        protected McQueryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public McQueryException(string? message) : base(message)
        {
        }

        public McQueryException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
    
    public class McQueryServerIsOffline : McQueryException
    {
        public McQuery Query { get; }
        public McQueryServerIsOffline(McQuery mcQuery)
        {
            Query = mcQuery;
        }
    }

    public class McQuerySocketIsNotInitialised : McQueryException
    {
        public McQuery Query { get; }
        
        public McQuerySocketIsNotInitialised(McQuery query)
        {
            Query = query;
        }

        protected McQuerySocketIsNotInitialised(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public McQuerySocketIsNotInitialised(string? message) : base(message)
        {
        }

        public McQuerySocketIsNotInitialised(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class McQueryWrongResponseException : McQueryException
    {
        public Request Request { get; }
        public byte[] Response { get; }

        public McQueryWrongResponseException(Request request, byte[] response)
        {
            Request = request;
            Response = response;
        }

        protected McQueryWrongResponseException(SerializationInfo info, StreamingContext context, Request request, byte[] response) : base(info, context)
        {
            Request = request;
            Response = response;
        }

        public McQueryWrongResponseException(string? message, Request request, byte[] response) : base(message)
        {
            Request = request;
            Response = response;
        }

        public McQueryWrongResponseException(string? message, Exception? innerException, Request request, byte[] response) : base(message, innerException)
        {
            Request = request;
            Response = response;
        }
    }
}