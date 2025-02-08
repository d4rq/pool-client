using MyProtocol.Serializator;

namespace MyProtocol.Packets
{
    public class PacketStrikeBall
    {
        [Field(1)]
        public string PlayerName;
        [Field(2)]
        public float Power;

        [Field(3)]
        public Vector2D Direction;

        [Field(4)]
        public int BallNumber;
    }
}