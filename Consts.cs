namespace MCQueryLib
{
    public enum PackageType : byte
    {
        Status = 0x00,
        Challenge = 0x09,
    }

    public enum RequestType
    {
        Handshake = 1,
        BasicStats = 2,
        FullStats = 3,
    }
}