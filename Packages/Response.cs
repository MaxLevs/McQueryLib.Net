#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
			if (data.Length < 1) throw new IncorrectPackageDataException(data);
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
			if (data.Length < 5) throw new IncorrectPackageDataException(data);
			var response = BitConverter.GetBytes(int.Parse(Encoding.ASCII.GetString(data, 5, data.Length - 6)));
			if (BitConverter.IsLittleEndian)
			{
				response = response.Reverse().ToArray();
			}

			return response;
		}

		public static ServerBasicState ParseBasicState(byte[] data)
		{
			if (data.Length <= 5)
				throw new IncorrectPackageDataException(data);

			var statusValues = new Queue<string>();
			short port = -1;

			data = data.Skip(5).ToArray(); // Skip Type + SessionId
			var stream = new MemoryStream(data);

			var sb = new StringBuilder();
			int currentByte;
			int counter = 0;
			while ((currentByte = stream.ReadByte()) != -1)
			{
				if (counter > 6) break;

				if (counter == 5)
				{
					byte[] portBuffer = {(byte) currentByte, (byte) stream.ReadByte()};
					if (!BitConverter.IsLittleEndian)
						portBuffer = portBuffer.Reverse().ToArray();

					port = BitConverter.ToInt16(portBuffer); // Little-endian short
					counter++;

					continue;
				}

				if (currentByte == 0x00)
				{
					string fieldValue = sb.ToString();
					statusValues.Enqueue(fieldValue);
					sb.Clear();
					counter++;
				}
				else sb.Append((char) currentByte);
			}

			var serverInfo = new ServerBasicState
			{
				Motd = statusValues.Dequeue(),
				GameType = statusValues.Dequeue(),
				Map = statusValues.Dequeue(),
				NumPlayers = int.Parse(statusValues.Dequeue()),
				MaxPlayers = int.Parse(statusValues.Dequeue()),
				HostPort = port,
				HostIp = statusValues.Dequeue(),
			};

			return serverInfo;
		}

		public static ServerFullState ParseFullState(byte[] data)
		{
			var statusKeyValues = new Dictionary<string, string>();
			var players = new List<string>();

			var buffer = new byte[256];
			Stream stream = new MemoryStream(data);

			stream.Read(buffer, 0, 5); // Read Type + SessionID
			stream.Read(buffer, 0, 11); // Padding: 11 bytes constant
			var constant1 = new byte[] {0x73, 0x70, 0x6C, 0x69, 0x74, 0x6E, 0x75, 0x6D, 0x00, 0x80, 0x00};
			for (int i = 0; i < constant1.Length; i++)
				Debug.Assert(constant1[i] == buffer[i], "Byte mismatch at " + i + " Val :" + buffer[i]);

			var sb = new StringBuilder();
			string lastKey = string.Empty;
			int currentByte;
			while ((currentByte = stream.ReadByte()) != -1)
			{
				if (currentByte == 0x00)
				{
					if (!string.IsNullOrEmpty(lastKey))
					{
						statusKeyValues.Add(lastKey, sb.ToString());
						lastKey = string.Empty;
					}
					else
					{
						lastKey = sb.ToString();
						if (string.IsNullOrEmpty(lastKey)) break;
					}

					sb.Clear();
				}
				else sb.Append((char) currentByte);
			}

			stream.Read(buffer, 0, 10); // Padding: 10 bytes constant
			var constant2 = new byte[] {0x01, 0x70, 0x6C, 0x61, 0x79, 0x65, 0x72, 0x5F, 0x00, 0x00};
			for (int i = 0; i < constant2.Length; i++)
				Debug.Assert(constant2[i] == buffer[i], "Byte mismatch at " + i + " Val :" + buffer[i]);

			while ((currentByte = stream.ReadByte()) != -1)
			{
				if (currentByte == 0x00)
				{
					var player = sb.ToString();
					if (string.IsNullOrEmpty(player)) break;
					players.Add(player);
					sb.Clear();
				}
				else sb.Append((char) currentByte);
			}

			ServerFullState fullState = new()
			{
				Motd = statusKeyValues["hostname"],
				GameType = statusKeyValues["gametype"],
				GameId = statusKeyValues["game_id"],
				Version = statusKeyValues["version"],
				Plugins = statusKeyValues["plugins"],
				Map = statusKeyValues["map"],
				NumPlayers = int.Parse(statusKeyValues["numplayers"]),
				MaxPlayers = int.Parse(statusKeyValues["maxplayers"]),
				PlayerList = players.ToArray(),
				HostIp = statusKeyValues["hostip"],
				HostPort = int.Parse(statusKeyValues["hostport"]),
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

	    protected IncorrectPackageDataException(SerializationInfo info, StreamingContext context, byte[] data) : base(
		    info, context)
	    {
		    this.data = data;
	    }

	    public IncorrectPackageDataException(string? message, byte[] data) : base(message)
	    {
		    this.data = data;
	    }

	    public IncorrectPackageDataException(string? message, Exception? innerException, byte[] data) : base(message,
		    innerException)
	    {
		    this.data = data;
	    }
    }
}