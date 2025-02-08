using System;
using System.Collections.Generic;

namespace MyProtocol
{
    public static class PacketTypeManager
    {
        private static readonly Dictionary<PacketType, Tuple<byte, byte>> TypeDictionary =
            new Dictionary<PacketType, Tuple<byte, byte>>
            {
                {PacketType.Handshake, Tuple.Create((byte)0x01, (byte)0x00)},
                {PacketType.PlayerJoin, Tuple.Create((byte)0x02, (byte)0x00)},
                {PacketType.StrikeBall, Tuple.Create((byte)0x03, (byte)0x00)},
                {PacketType.GameResult, Tuple.Create((byte)0x04, (byte)0x00)},
                {PacketType.BallPocketed, Tuple.Create((byte)0x05, (byte)0x00)},
                {PacketType.ChangeTurn, Tuple.Create((byte)0x06, (byte)0x00)},
                {PacketType.PlayerList, Tuple.Create((byte)0x07, (byte)0x00)},
                {PacketType.Error, Tuple.Create((byte)0x08, (byte)0x00)}
            };

        public static void RegisterType(PacketType type, byte btype, byte bsubtype)
        {
            if (TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is already registered.");
            }

            TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
        }

        public static Tuple<byte, byte> GetType(PacketType type)
        {
            if (!TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is not registered.");
            }

            return TypeDictionary[type];
        }

        public static PacketType GetTypeFromPacket(Packet packet)
        {
            var type = packet.PacketType;
            var subtype = packet.PacketSubtype;

            foreach (var tuple in TypeDictionary)
            {
                var value = tuple.Value;

                if (value.Item1 == type && value.Item2 == subtype)
                {
                    return tuple.Key;
                }
            }

            return PacketType.Unknown;
        }

        public static void Clear()
        {
            TypeDictionary.Clear();
        }
    }
}