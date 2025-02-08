namespace MyProtocol
{
    public class PacketConstants
    {
        public const byte PacketStartByte1 = 0xAF;
        public const byte PacketStartByte2 = 0xAA;
        public const byte PacketStartByte3 = 0xAF;
        public const byte PacketEncStartByte1 = 0x95;
        public const byte PacketEncStartByte2 = 0xAA;
        public const byte PacketEncStartByte3 = 0xFF;
        public const byte PacketEndByte1 = 0xFF;
        public const byte PacketEndByte2 = 0x00;
    }
}