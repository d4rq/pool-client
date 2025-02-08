using MyProtocol.Serializator;

namespace MyProtocol.Packets
{
    public class PacketGameResult
    {
        [Field(1)]
        public string WinnerName;

        [Field(2)]
        public int Player1Score;

        [Field(3)]
        public int Player2Score;
    
        [Field(4)]
        public int Player3Score;
    
        [Field(5)]
        public int Player4Score;
    }
}