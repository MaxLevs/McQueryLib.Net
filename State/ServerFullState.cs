namespace MCQueryLib.State
{
    /// <summary>
    /// Represents data which is received from FullState request
    /// </summary>
    public class ServerFullState : ServerState
    {
        public string Motd { get; set; }
        public string GameType { get; set; }
        public string GameId { get; set; }
        public string Version { get; set; }
        public string Plugins { get; set; }
        public string Map { get; set; }
        public int NumPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public string[] PlayerList { get; set; }
        public int HostPort { get; set; }
        public string HostIp { get; set; }

        public override string ToString()
        {
            return "FullStatus\n" +
                   $"| {nameof(Motd)}: {Motd}\n" +
                   $"| {nameof(GameType)}: {GameType}\n" +
                   $"| {nameof(GameId)}: {GameId}\n" +
                   $"| {nameof(Version)}: {Version}\n" +
                   $"| {nameof(Plugins)}: {Plugins}\n" +
                   $"| {nameof(Map)}: {Map}\n" +
                   $"| {nameof(NumPlayers)}: {NumPlayers}\n" +
                   $"| {nameof(MaxPlayers)}: {MaxPlayers}\n" +
                   $"| {nameof(PlayerList)}: [{string.Join(", ", PlayerList)}]\n" +
                   $"| {nameof(HostPort)}: {HostPort}\n" +
                   $"| {nameof(HostIp)}: {HostIp}";
        }
    }
}