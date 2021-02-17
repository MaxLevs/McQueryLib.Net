using System.Collections.Generic;
using System.Linq;

namespace MCQueryLib
{
    public class State
    {
        public string Modt { get; set; }
        public string GameType { get; set; }
        public string GameId { get; set; }
        public string Version { get; set; }
        public string Plugins { get; set; }
        public string Map { get; set; }
        public int NumPlayers { get; set; }
        public int MaxPlayers { get; set; }

        private string[] _players = null;
        public string[] Players
        {
            get => _players;

            set
            {
                if (_players != null)
                {
                    foreach (var player in _players)
                    {
                        if (!value.Contains(player))
                        {
                            // emit Log out event
                        }
                    }
                    
                    foreach (var player in value)
                    {
                        if (!_players.Contains(player))
                        {
                            // emit Log in event
                        }
                    }
                }
                
                _players = value;
            }
        }

        public int HostPort { get; set; }
        public string HostIp { get; set; }
    }
}