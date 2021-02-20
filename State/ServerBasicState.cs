namespace MCQueryLib.State
{
    /// <summary>
    /// Represents data which is received from BasicState request
    /// </summary>
    public class ServerBasicState : ServerState
    {
        public string Motd { get; set; }
        public string GameType { get; set; }
        public string Map { get; set; }
        public int NumPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public short HostPort { get; set; }
        public string HostIp { get; set; }

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