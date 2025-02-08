using MyProtocol.Serializator;

namespace MyProtocol.Packets
{
    public class PacketPlayerJoin
    {
        [Field(1)]
        public string PlayerName;

        [Field(2)] 
        public string ErrorMessage = "1";
    }
}