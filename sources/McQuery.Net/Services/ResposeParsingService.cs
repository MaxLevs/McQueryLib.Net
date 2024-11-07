#nullable enable
using MCQueryLib.Data;
using MCQueryLib.Data.Packages.Responses;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MCQueryLib.Services
{
	/// <summary>
	/// This class parses Minecraft Query response packages for getting data from it
	/// Wiki: https://wiki.vg/Query
	/// </summary>
	public static class ResposeParsingService
	{
		public static byte ParseType(byte[] data)
		{
			return data[0];
		}

		public static SessionId ParseSessionId(ref SequenceReader<byte> reader)
		{
			if (reader.UnreadSequence.Length < 4) throw new IncorrectPackageDataException(reader.Sequence.ToArray());
			var sessionIdBytes = new byte[4];
			Span<byte> sessionIdSpan = new(sessionIdBytes);
			reader.TryCopyTo(sessionIdSpan);
			reader.Advance(4);
			return new SessionId(sessionIdSpan.ToArray());
		}

		/// <summary>
		/// Parses response package and returns ChallengeToken
		/// </summary>
		/// <param name="rawResponse">RawResponce package</param>
		/// <returns>byte[] array which contains ChallengeToken as big-endian</returns>
		public static byte[] ParseHandshake(RawResponse rawResponse)
		{
			var data = (byte[])rawResponse.RawData.Clone();

			if (data.Length < 5) throw new IncorrectPackageDataException(data);
			var response = BitConverter.GetBytes(int.Parse(Encoding.ASCII.GetString(data, 5, rawResponse.RawData.Length - 6)));
			if (BitConverter.IsLittleEndian)
			{
				response = response.Reverse().ToArray();
			}

			return response;
		}

		public static ServerBasicStateResponse ParseBasicState(RawResponse rawResponse)
		{
			if (rawResponse.RawData.Length <= 5)
				throw new IncorrectPackageDataException(rawResponse.RawData);

			SequenceReader<byte> reader = new(new ReadOnlySequence<byte>(rawResponse.RawData));
			reader.Advance(1); // Skip Type

			var sessionId = ParseSessionId(ref reader);

			var motd = ReadString(ref reader);
			var gameType = ReadString(ref reader);
			var map = ReadString(ref reader);
			var numPlayers = int.Parse(ReadString(ref reader));
			var maxPlayers = int.Parse(ReadString(ref reader));

			if (!reader.TryReadLittleEndian(out short port))
				throw new IncorrectPackageDataException(rawResponse.RawData);

			var hostIp = ReadString(ref reader);

			ServerBasicStateResponse serverInfo = new(
				serverUUID: rawResponse.ServerUUID,
				sessionId: sessionId,
				motd: motd,
				gameType: gameType,
				map: map,
				numPlayers: numPlayers,
				maxPlayers: maxPlayers,
				hostPort: port,
				hostIp: hostIp,
				rawData: (byte[])rawResponse.RawData.Clone()
			);

			return serverInfo;
		}

		private static readonly byte[] constant1 = new byte[] { 0x73, 0x70, 0x6C, 0x69, 0x74, 0x6E, 0x75, 0x6D, 0x00, 0x80, 0x00 };
		private static readonly byte[] constant2 = new byte[] { 0x01, 0x70, 0x6C, 0x61, 0x79, 0x65, 0x72, 0x5F, 0x00, 0x00 };
		public static ServerFullStateResponse ParseFullState(RawResponse rawResponse)
		{
			if (rawResponse.RawData.Length <= 5)
				throw new IncorrectPackageDataException(rawResponse.RawData);

			var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(rawResponse.RawData));
			reader.Advance(1); // Skip Type

			var sessionId = ParseSessionId(ref reader);

			if (!reader.IsNext(constant1, advancePast: true))
				throw new IncorrectPackageDataException(rawResponse.RawData);

			var statusKeyValues = new Dictionary<string, string>();
			while (!reader.IsNext(0, advancePast: true))
			{
				var key = ReadString(ref reader);
				var value = ReadString(ref reader);
				statusKeyValues.Add(key, value);
			}

			if (!reader.IsNext(constant2, advancePast: true)) // Padding: 10 bytes constant
				throw new IncorrectPackageDataException(rawResponse.RawData);

			var players = new List<string>();
			while (!reader.IsNext(0, advancePast: true))
			{
				players.Add(ReadString(ref reader));
			}

			ServerFullStateResponse fullState = new
			(
				serverUUID: rawResponse.ServerUUID,
				sessionId: sessionId,
				motd: statusKeyValues["hostname"],
				gameType: statusKeyValues["gametype"],
				gameId: statusKeyValues["game_id"],
				version: statusKeyValues["version"],
				plugins: statusKeyValues["plugins"],
				map: statusKeyValues["map"],
				numPlayers: int.Parse(statusKeyValues["numplayers"]),
				maxPlayers: int.Parse(statusKeyValues["maxplayers"]),
				playerList: players.ToArray(),
				hostIp: statusKeyValues["hostip"],
				hostPort: int.Parse(statusKeyValues["hostport"]),
				rawData: (byte[])rawResponse.RawData.Clone()
			);

			return fullState;
		}

		private static string ReadString(ref SequenceReader<byte> reader)
		{
			if (!reader.TryReadTo(out ReadOnlySequence<byte> bytes, delimiter: 0, advancePastDelimiter: true))
				throw new IncorrectPackageDataException("Zero byte not found", reader.Sequence.ToArray());

			return Encoding.ASCII.GetString(bytes); // а точно ASCII? Может, Utf8?
		}
	}

	public class IncorrectPackageDataException : Exception
	{
		public byte[] data { get; }

		public IncorrectPackageDataException(byte[] data)
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
