#nullable enable
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
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
        public SessionId SessionId { get; }

        private UdpClient _udpClient;
        public int ResponseWaitIntervalSecond = 10;

        private object _challengeTokenLock = new();
        private ChallengeToken _challengeToken;

        public bool IsOnline { get; set; } = false;

        public McQuery(IPAddress host, int queryPort, Random rnd)
        {
            Host = host;
            Port = queryPort;
            SessionId = SessionId.GenerateRandomId(rnd);
        }

        public void InitSocket()
        {
            try
            {
                _udpClient?.Dispose();
                _udpClient = new UdpClient(Host.ToString(), Port);
            }
            
            catch (ObjectDisposedException)
            {
                
            }
        }

        public async Task<byte[]> GetHandshake()
        {
            if (_udpClient == null)
                throw new McQuerySocketIsNotInitialised(this);

            int times = 0;
            while (true)
            {
                try
                {
                    Request handshakeRequest = Request.GetHandshakeRequest(SessionId);
                    var response = await SendResponseService.SendReceive(_udpClient, handshakeRequest.Data,
                            ResponseWaitIntervalSecond);

                    if (handshakeRequest.RequestType != Response.ParseType(response))
                        throw new McQueryWrongResponseException(handshakeRequest, response);

                    var challengeToken = Response.ParseHandshake(response);
                    _challengeToken = new ChallengeToken(challengeToken);

                    return challengeToken;
                }

                catch (ObjectDisposedException)
                {
                    ++times;

                    if (times > 5)
                    {
                        throw;
                    }
                    
                    Thread.Sleep(800);
                }
            }

        }

        public async Task<ServerBasicState> GetBasicStatus()
        {
            if (_udpClient == null)
                throw new McQuerySocketIsNotInitialised(this);

            if (!IsOnline)
                throw new McQueryServerIsOffline(this);
            
            ChallengeToken challengeToken;
            lock (_challengeTokenLock)
            {
                challengeToken = _challengeToken ?? throw new McQueryServerIsOffline(this);
            }

            int times = 0;
            while (true)
            {
                try
                {
                    Request basicStatusRequest = Request.GetBasicStatusRequest(SessionId, challengeToken);
                    var response = await SendResponseService.SendReceive(_udpClient, basicStatusRequest.Data, ResponseWaitIntervalSecond);
                    
                    if (basicStatusRequest.RequestType != Response.ParseType(response))
                        throw new McQueryWrongResponseException(basicStatusRequest, response);
                    
                    var basicStatus = Response.ParseBasicState(response);
                    return basicStatus;
                }

                catch (ObjectDisposedException)
                {
                    ++times;

                    if (times > 5)
                    {
                        throw;
                    }
                    
                    Thread.Sleep(800);
                }
            }
        }
        
        public async Task<ServerFullState> GetFullStatus()
        {
            if (_udpClient == null)
                throw new McQuerySocketIsNotInitialised(this);

            if (!IsOnline)
                throw new McQueryServerIsOffline(this);

            ChallengeToken challengeToken;
            {
                challengeToken = _challengeToken;
            }
            
            int times = 0;
            while (true)
            {
                try
                {
                    Request fullStatusRequest = Request.GetFullStatusRequest(SessionId, challengeToken);
                    var response = await SendResponseService.SendReceive(_udpClient, fullStatusRequest.Data, ResponseWaitIntervalSecond);

                    // todo: causes receiver to skip package. Rewrite to automatic pick response by raw
                    if (fullStatusRequest.RequestType != Response.ParseType(response))
                        throw new McQueryWrongResponseException(fullStatusRequest, response);
                    
                    var fullStatus = Response.ParseFullState(response);
                    return fullStatus;
                }

                catch (ObjectDisposedException)
                {
                    ++times;

                    if (times > 5)
                    {
                        throw;
                    }
                    
                    Thread.Sleep(800);
                }
            }
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
        public byte[] ReceivedResponse { get; }

        public override string Message { get; }

        public McQueryWrongResponseException(Request request, byte[] receivedResponse)
        {
            Request = request;
            ReceivedResponse = receivedResponse;
            Message = $"Response is wrong. Expected type: {request.RequestType}. Received package with type: {Response.ParseType(receivedResponse)}" + base.Message;
        }

        protected McQueryWrongResponseException(SerializationInfo info, StreamingContext context, Request request, byte[] receivedResponse) : base(info, context)
        {
            Request = request;
            ReceivedResponse = receivedResponse;
        }

        public McQueryWrongResponseException(string? message, Request request, byte[] receivedResponse) : base(message)
        {
            Request = request;
            ReceivedResponse = receivedResponse;
        }

        public McQueryWrongResponseException(string? message, Exception? innerException, Request request, byte[] receivedResponse) : base(message, innerException)
        {
            Request = request;
            ReceivedResponse = receivedResponse;
        }
    }
}