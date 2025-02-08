using MyProtocol.Serializator;

namespace MyProtocol.Packets
{
    public class PacketHandshake
    {
        [Field(1)]
        public int MagicHandshakeNumber;
    }
}