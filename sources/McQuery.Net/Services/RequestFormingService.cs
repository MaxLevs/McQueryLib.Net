#nullable enable
using MCQueryLib.Data;
using MCQueryLib.Data.Packages;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MCQueryLib.Services
{
	/// <summary>
	/// This class builds Minecraft Query Packages for requests
	/// Wiki: https://wiki.vg/Query
	/// </summary>
	public static class RequestFormingService
	{
		private static readonly byte[] MagicConst = { 0xfe, 0xfd };
		private static readonly byte[] ChallengeRequestConst = { 0x09 };
		private static readonly byte[] StatusRequestConst = { 0x00 };

		public static Request HandshakeRequestPackage(SessionId sessionId)
		{
			var data = new List<byte>(224);
			data.AddRange(MagicConst);
			data.AddRange(ChallengeRequestConst);
			sessionId.WriteTo(data);

			var request = new Request(data.ToArray());
			return request;
		}

		public static Request GetBasicStatusRequestPackage(SessionId sessionId, ChallengeToken challengeToken)
		{
			if (challengeToken == null)
			{
				throw new ChallengeTokenIsNullException();
			}

			var data = new List<byte>(416);
			data.AddRange(MagicConst);
			data.AddRange(StatusRequestConst);
			sessionId.WriteTo(data);
			challengeToken.WriteTo(data);

			var request = new Request(data.ToArray());
			return request;
		}

		public static Request GetFullStatusRequestPackage(SessionId sessionId, ChallengeToken challengeToken)
		{
			if (challengeToken == null)
			{
				throw new ChallengeTokenIsNullException();
			}

			var data = new List<byte>(544);
			data.AddRange(MagicConst);
			data.AddRange(StatusRequestConst);
			sessionId.WriteTo(data);
			challengeToken.WriteTo(data);
			data.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 }); // Padding

			var request = new Request(data.ToArray());
			return request;
		}
	}

	public class ChallengeTokenIsNullException : Exception
	{
		public ChallengeTokenIsNullException()
		{
		}

		protected ChallengeTokenIsNullException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public ChallengeTokenIsNullException(string? message) : base(message)
		{
		}

		public ChallengeTokenIsNullException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}