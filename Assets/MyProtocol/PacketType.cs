namespace MyProtocol
{
    public enum PacketType
    {
        Unknown,
        Handshake,
        PlayerJoin,
        PlayerList,
        StrikeBall,
        BallPocketed,
        ChangeTurn,
        GameResult,
        Error
    }
}