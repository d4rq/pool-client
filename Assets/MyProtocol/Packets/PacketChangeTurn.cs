using MyProtocol.Serializator;

namespace MyProtocol.Packets
{
    public class PacketChangeTurn
    {
        [Field(1)]
        public string PlayerName;
    }
}