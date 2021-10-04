namespace MCQueryLib.Data.Packages.Responses
{
    /// <summary>
    /// Represents data which is received from BasicState request
    /// </summary>
    public class ServerBasicStateResponse : IResponse
    {
        public ServerBasicStateResponse(string motd,
                                        string gameType,
                                        string map,
                                        int numPlayers,
                                        int maxPlayers,
                                        short hostPort,
                                        string hostIp,
                                        byte[] rawData)
        {
            Motd = motd;
            GameType = gameType;
            Map = map;
            NumPlayers = numPlayers;
            MaxPlayers = maxPlayers;
            HostPort = hostPort;
            HostIp = hostIp;
            RawData = rawData;
        }

        public string Motd { get; }
        public string GameType { get; }
        public string Map { get; }
        public int NumPlayers { get; }
        public int MaxPlayers { get; }
        public short HostPort { get; }
        public string HostIp { get; }

        public byte[] RawData { get; }

        public override string ToString()
        {
            return "BasicStatus\n" +
                   $"| {nameof(Motd)}: {Motd}\n" +
                   $"| {nameof(GameType)}: {GameType}\n" +
                   $"| {nameof(Map)}: {Map}\n" +
                   $"| {nameof(NumPlayers)}: {NumPlayers}\n" +
                   $"| {nameof(MaxPlayers)}: {MaxPlayers}\n" +
                   $"| {nameof(HostPort)}: {HostPort}\n" +
                   $"| {nameof(HostIp)}: {HostIp}";
        }
    }
}