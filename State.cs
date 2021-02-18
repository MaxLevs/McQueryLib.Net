using System.Collections.Generic;
using System.Linq;

namespace MCQueryLib
{
    public class State
    {
        public byte Type { get; set; }
        public byte[] SessionId { get; set; }
        
        public string Modt { get; set; }
        public string GameType { get; set; }
        public string GameId { get; set; }
        public string Version { get; set; }
        public string Plugins { get; set; }
        public string Map { get; set; }
        public int NumPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public string[] Players { get; set; }
        public int HostPort { get; set; }
        public string HostIp { get; set; }
    }
}