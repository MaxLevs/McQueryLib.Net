using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MCQueryLib
{
    public class Response
    {
        public static Handshake ParseChallenge(byte[] data)
        {
            var res = new Handshake
            {
                Type = data[0],
                ChallengeToken =
                    BitConverter.GetBytes(Int32.Parse(new ASCIIEncoding().GetString(data, 5, data.Length - 6))),
                SessionId = new byte[4]
            };
            Buffer.BlockCopy(data, 1, res.SessionId, 0, 4);
            
            if (BitConverter.IsLittleEndian)
                res.ChallengeToken = res.ChallengeToken.Reverse().ToArray();
            
            return res;
        }

        public static State ParseBasicState(byte[] data)
        {
            var res = new State {Type = data[0], SessionId = new byte[4]};
            Buffer.BlockCopy(data, 1, res.SessionId, 0, 4);
            string rdata = new ASCIIEncoding().GetString(data, 5, data.Length - 5);
            var fields = rdata.Split('\0');
            res.Modt = fields[0];
            res.GameType = fields[1];
            res.Map = fields[2];
            res.NumPlayers = int.Parse(fields[3]);
            res.MaxPlayers = int.Parse(fields[4]);
            // res.HostPort = Int32.Parse(fields[5]);
            // res.HostIp = fields[6];
            
            return res;
        }

        public static State ParseFullState(byte[] data)
        {
            var res = new State {Type = data[0], SessionId = new byte[4]};
            Buffer.BlockCopy(data, 1, res.SessionId, 0, 4);

            var fields = new List<string>();
            var buffer = new List<char>();
            var pointer = 16;
            while (true)
            {
                if (data[pointer] == 0)
                {
                    fields.Add(new string(buffer.ToArray()));
                    buffer.Clear();
                    pointer++;
                }

                if (data[pointer] == 0)
                {
                    buffer.Clear();
                    pointer++;
                    break;
                }
                
                buffer.Add((char) data[pointer]);
                pointer++;
            }

            res.Modt = fields[1];
            res.GameType = fields[3];
            res.GameId = fields[5];
            res.Version = fields[7];
            res.Plugins = fields[9];
            res.Map = fields[11];
            res.NumPlayers = int.Parse(fields[13]);
            res.MaxPlayers = int.Parse(fields[15]);
            res.HostPort = int.Parse(fields[17]);
            res.HostIp = fields[19];

            var players = new List<string>();
            pointer += 10;
            while (true)
            {
                if (data[pointer] == 0)
                {
                    var name = new string(buffer.ToArray());
                    buffer.Clear();
                    
                    if (name == "")
                    {
                        break;
                    }
                    
                    players.Add(name);
                    pointer++;
                }

                if (data[pointer] == 0)
                {
                    buffer.Clear();
                    break;
                }
                
                buffer.Add((char) data[pointer]);
                pointer++;
            }

            res.Players = players.ToArray();
            
            return res;
        }
    }
}