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
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
        public string[] PlayerList { get; set; }
        public int Port { get; set; }
        public string Address { get; set; } 
    }
}