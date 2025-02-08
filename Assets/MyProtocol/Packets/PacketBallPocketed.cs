using MyProtocol.Serializator;

namespace MyProtocol.Packets
{
    public class PacketBallPocketed
    {
        [Field(1)]
        public string PlayerNames;
    
        [Field(2)]
        public int BallNumber;
    }
}