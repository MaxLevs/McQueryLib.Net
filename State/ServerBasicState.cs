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
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
    }
}