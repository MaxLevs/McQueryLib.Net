namespace MCQueryLib
{
    public class Handshake
    {
        public byte Type { get; set; }
        public byte[] SessionId { get; set; }
        public byte[] ChallengeToken { get; set; }
    }
}