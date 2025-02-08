using MyProtocol.Serializator;
using System.Collections.Generic;

namespace MyProtocol.Packets
{
    public class PacketPlayerList
    {
        [Field(1)] // Указываем FieldID для этого поля
        public List<string> Players;
    }
}