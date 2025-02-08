using MyProtocol;
using MyProtocol.Packets;
using MyProtocol.Serializator;
using UnityEngine;

public class PocketHandler : MonoBehaviour
{
    public static bool needsChange = true;
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ���������, ��� ������, ������� ����� � �������, �������� �����
        if (other.CompareTag("Ball"))
        {
            // ������� ��� �� �����
            Destroy(other.gameObject);

            // �� ������ �������� �������������� ������ �����, ��������, ���������� �����
            Debug.Log("��� ����� � ����!");
            needsChange = false;

            if (MenuInput.name == Program.currentPlayer)
            {
                var packet = PacketConverter.Serialize(PacketType.BallPocketed, new PacketBallPocketed() { BallNumber = 1, PlayerNames = MenuInput.name }).ToPacket();

                Program.client.QueuePacketSend(packet);
            }
        }
    }
}
