using MyProtocol;
using MyProtocol.Packets;
using MyProtocol.Serializator;
using UnityEngine;

public class PocketHandler : MonoBehaviour
{
    public static bool needsChange = true;
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что объект, который вошел в триггер, является шаром
        if (other.CompareTag("Ball"))
        {
            // Удаляем шар из сцены
            Destroy(other.gameObject);

            // Вы можете добавить дополнительную логику здесь, например, обновление счета
            Debug.Log("Шар попал в лузу!");
            needsChange = false;

            if (MenuInput.name == Program.currentPlayer)
            {
                var packet = PacketConverter.Serialize(PacketType.BallPocketed, new PacketBallPocketed() { BallNumber = 1, PlayerNames = MenuInput.name }).ToPacket();

                Program.client.QueuePacketSend(packet);
            }
        }
    }
}
