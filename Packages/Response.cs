using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MCQueryLib.Data;
using MCQueryLib.State;

namespace MCQueryLib.Packages
{
    /// <summary>
    /// This class parses Minecraft Query response packages for getting data from it
    /// Wiki: https://wiki.vg/Query
    /// </summary>
    public static class Response
    {
	    public static byte ParseType(byte[] data)
	    {
		    return data[0];
	    }
	    
	    public static SessionId ParseSessionId(byte[] data)
	    {
		    if (data.Length < 1)
			    throw new IncorrectPackageDataException(data);
		    
			var sessionIdBytes = new byte[4];
			Buffer.BlockCopy(data, 1, sessionIdBytes, 0, 4);
			return new SessionId(sessionIdBytes);
	    }
	    
	    /// <summary>
	    /// Parses response package and returns ChallengeToken
	    /// </summary>
	    /// <param name="data">byte[] package</param>
	    /// <returns>byte[] array which contains ChallengeToken as big-endian</returns>
        public static byte[] ParseHandshake(byte[] data)
        {
		    if (data.Length < 5)
			    throw new IncorrectPackageDataException(data);
		    
            var response = BitConverter.GetBytes(int.Parse(Encoding.ASCII.GetString(data, 5, data.Length - 6)));
            if (BitConverter.IsLittleEndian)
            {
                response = response.Reverse().ToArray();
            }
            
            return response;
        }

        public static ServerBasicState ParseBasicState(byte[] data)
        {
			if (data.Length != 0)
			{
				data = data.Skip(5).ToArray();

				string stringData = Encoding.ASCII.GetString(data);
				string[] informations = stringData.Split(new string[] { "\0" }, StringSplitOptions.None);

				//0 = MOTD
				//1 = GameType
				//2 = Map
				//3 = Number of Players
				//4 = Maxnumber of Players
				//5 = Host Port
				//6 = Host IP

				if (informations[5].StartsWith(":k"))
				{
					informations[5] = informations[5].Substring(2);
				}

				var serverInfo = new ServerBasicState
				{
					Motd = informations[0],
					GameType = informations[1],
					Map = informations[2],
					PlayerCount = int.Parse(informations[3]),
					MaxPlayers = int.Parse(informations[4]),
					Address = informations[5],
					Port = informations[6] //TODO: Port is currently missing... It needs to be fixed.
				};

				return serverInfo;
			}

			return null;
        }
        
        public static ServerFullState ParseFullState(byte[] data)
        {
			data = data.Skip(16).ToArray();

			string stringData = Encoding.ASCII.GetString(data);

			//This array should contain an array with server informations and an array with playernames
			string[] informations = stringData.Split(new[] {"player_\0\0"}, StringSplitOptions.None);

			string[] serverInfoArr = informations[0].Split(new[] { "\0" }, StringSplitOptions.None);
			
			// todo: something goes wrong here, may be player field is undefined
			string[] playerList = informations[1].Split(new[] { "\0" }, StringSplitOptions.None)
				.Where(s => !string.IsNullOrEmpty(s)).ToArray();

			//Split serverInfo to key - value pair.

			Dictionary<string, string> serverDict = new Dictionary<string, string>();

			for (int i = 0; i < serverInfoArr.Length; i += 2)
			{
				serverDict.Add(serverInfoArr[i], serverInfoArr[i + 1]);
			}

			//0 = MOTD
			//1 = GameType
			//2 = Map
			//3 = Number of Players
			//4 = Maxnumber of Players
			//5 = Host Port
			//6 = Host IP

			ServerFullState fullState = new ServerFullState
			{
				Motd = serverDict["hostname"],
				GameType = serverDict["gametype"],
				Map = serverDict["map"],
				PlayerCount = int.Parse(serverDict["numplayers"]),
				MaxPlayers = int.Parse(serverDict["maxplayers"]),
				PlayerList =  playerList,
				Plugins = serverDict["plugins"],
				Address = serverDict["hostip"],
				Port = int.Parse(serverDict["hostport"]),
				Version = serverDict["version"]
			};

            return fullState;
        }
    }

    public class IncorrectPackageDataException : Exception
    {
	    public byte[] data { get; }

	    public IncorrectPackageDataException(byte[] data)
	    {
		    this.data = data;
	    }

	    protected IncorrectPackageDataException(SerializationInfo info, StreamingContext context, byte[] data) : base(info, context)
	    {
		    this.data = data;
	    }

	    public IncorrectPackageDataException(string? message, byte[] data) : base(message)
	    {
		    this.data = data;
	    }

	    public IncorrectPackageDataException(string? message, Exception? innerException, byte[] data) : base(message, innerException)
	    {
		    this.data = data;
	    }
    }
}