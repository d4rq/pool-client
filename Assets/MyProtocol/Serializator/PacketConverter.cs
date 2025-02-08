using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MyProtocol.Serializator
{
    public class PacketConverter
    {
        public static T Deserialize<T>(Packet packet, bool strict = false)
        {
            var fields = GetFields(typeof(T));
            var instance = Activator.CreateInstance<T>();

            if (fields.Count == 0)
            {
                return instance;
            }

            foreach (var tuple in fields)
            {
                var field = tuple.Item1;
                var packetFieldId = tuple.Item2;

                if (!packet.HasField(packetFieldId))
                {
                    if (strict)
                    {
                        throw new Exception($"Couldn't get field[{packetFieldId}] for {field.Name}");
                    }

                    continue;
                }

                var fieldType = field.FieldType;

                if (fieldType == typeof(List<string>))
                {
                    // Десериализуем массив байтов в List<string>
                    var rawData = packet.GetValueRaw(packetFieldId);
                    var stringList = DeserializeStringList(rawData);
                    field.SetValue(instance, stringList);
                }
                else
                {
                    var value = typeof(Packet)
                        .GetMethod("GetValue")?
                        .MakeGenericMethod(fieldType)
                        .Invoke(packet, new object[] { packetFieldId });

                    if (value == null)
                    {
                        if (strict)
                        {
                            throw new Exception($"Couldn't get value for field[{packetFieldId}] for {field.Name}");
                        }

                        continue;
                    }

                    field.SetValue(instance, value);
                }
            }

            return instance;
        }

        private static List<string> DeserializeStringList(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                var count = reader.ReadInt32(); // Читаем количество строк
                var stringList = new List<string>(count);

                for (int i = 0; i < count; i++)
                {
                    stringList.Add(reader.ReadString()); // Читаем каждую строку
                }

                return stringList;
            }
        }

        public static Packet Serialize(PacketType type, int subtype, object obj, bool strict = false)
        {
            var typeInfo = PacketTypeManager.GetType(type);
            return Serialize(typeInfo.Item1, (byte)subtype, obj, strict);
        }
        public static Packet Serialize(PacketType type, object obj, bool strict = false)
        {
            var t = PacketTypeManager.GetType(type);
            return Serialize(t.Item1, t.Item2, obj, strict);
        }

        public static Packet Serialize(byte type, byte subtype, object obj, bool strict = false)
        {
            var fields = GetFields(obj.GetType());

            if (strict)
            {
                var usedUp = new List<byte>();

                foreach (var field in fields)
                {
                    if (usedUp.Contains(field.Item2))
                    {
                        throw new Exception("One field used two times.");
                    }

                    usedUp.Add(field.Item2);
                }
            }

            var packet = Packet.Create(type, subtype);

            foreach (var field in fields)
            {
                var value = field.Item1.GetValue(obj);

                if (value is List<string> stringList)
                {
                    // Сериализуем List<string> в массив байтов
                    var serializedList = SerializeStringList(stringList);
                    packet.SetValueRaw(field.Item2, serializedList);
                }
                else
                {
                    packet.SetValue(field.Item2, value ?? "asd");
                }
            }

            return packet;
        }

        private static byte[] SerializeStringList(List<string> stringList)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(stringList.Count); // Записываем количество строк
                foreach (var str in stringList)
                {
                    writer.Write(str); // Записываем каждую строку
                }
                return ms.ToArray();
            }
        }

        private static List<Tuple<FieldInfo, byte>> GetFields(Type t)
        {
            return t.GetFields(BindingFlags.Instance |
                               BindingFlags.NonPublic |
                               BindingFlags.Public)
                .Where(field => field.GetCustomAttribute<FieldAttribute>() != null)
                .Select(field => Tuple.Create(field, field.GetCustomAttribute<FieldAttribute>().FieldID))
                .ToList();
        }
    }
}
