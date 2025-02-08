using MyProtocol.Serializator;

namespace MyProtocol.Packets
{
    public class PacketError
    {
        [Field(1)]
        public string ErrorMessage;
    }
}